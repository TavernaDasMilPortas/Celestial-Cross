using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data;

namespace CelestialCross.Gacha
{
    public class GachaService
    {
        public static GachaService Instance { get; private set; }

        private IGachaProvider _provider;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new GachaService();
                // Por padrão inicia local, pode ser trocado pelo CloudGachaProvider no futuro
                Instance._provider = new LocalGachaProvider();
            }
        }

        public void SetProvider(IGachaProvider provider)
        {
            _provider = provider;
        }

        public GachaPityState GetPityState(Account account, string bannerId)
        {
            account.EnsureInitialized();
            foreach (var state in account.GachaPityStates)
            {
                if (state.BannerID == bannerId)
                    return state;
            }
            var newState = new GachaPityState(bannerId);
            account.GachaPityStates.Add(newState);
            return newState;
        }

        public bool ConnectSupremeChoice(Account account, string bannerId, string supremeChoice)
        {
            var state = GetPityState(account, bannerId);
            state.SelectedSupremeChoice = supremeChoice;
            return true;
        }

        public async Task<List<GachaRewardEntry>> PerformPullsAsync(Account account, GachaBannerSO banner, int times)
        {
            if (_provider == null) Initialize();
            return await _provider.PullAsync(account, banner, times);
        }

        public List<GachaRewardEntry> ExecutePullsInternal(Account account, GachaBannerSO banner, int times)
        {
            List<GachaRewardEntry> results = new List<GachaRewardEntry>();
            int totalCost = banner.CostPerPull * times;

            if (account.StarMaps < totalCost)
            {
                Debug.LogWarning("[Gacha] Saldo insuficiente de Mapas das Estrelas!");
                return null;
            }

            // Consome a moeda
            account.StarMaps -= totalCost;
            var pityState = GetPityState(account, banner.BannerID);

            for (int i = 0; i < times; i++)
            {
                pityState.PullsSinceLastSupreme++;
                pityState.PullsSinceLastOverBase++;
                
                GachaRarity rolledRarity = DetermineRarity(banner, pityState);
                
                // Reseta os pities se tiver a raridade certa
                if (rolledRarity == GachaRarity.Supreme)
                    pityState.PullsSinceLastSupreme = 0;
                
                if (rolledRarity != GachaRarity.Base)
                    pityState.PullsSinceLastOverBase = 0;

                GachaRewardEntry rolledReward = SelectRewardFromPool(banner, rolledRarity, pityState);
                
                // Trata 50/50 Se era supremo foco ou nulo (Perda)
                if (rolledRarity == GachaRarity.Supreme)
                {
                    string rolledID = rolledReward.GetID();
                    bool isTargetSupreme = banner.HasEpitomizedPath && rolledID == pityState.SelectedSupremeChoice;
                    
                    if (isTargetSupreme)
                    {
                        pityState.Lost5050 = false; // Garantiu
                        pityState.SelectedSupremeChoice = ""; // Resetar a escolha (pool de foco do supreme)
                    }
                    else if (banner.HasEpitomizedPath)
                    {
                        pityState.Lost5050 = true; // Perdeu pro general pool
                    }
                    
                    // Reseta os dados focados caso tiremos Supreme independente
                    pityState.PullsSinceLastSupreme = 0;
                }

                DispatchReward(account, rolledReward);
                results.Add(rolledReward);
            }

            // Salva a conta
            AccountManager.Instance.SaveAccount();
            return results;
        }

        private GachaRarity DetermineRarity(GachaBannerSO banner, GachaPityState pity)
        {
            if (banner.BasicProbabilities == null || banner.BasicProbabilities.Count == 0)
            {
                Debug.LogError("[Gacha] Banner não possui tabela de probabilidades!");
                return GachaRarity.Base;
            }

            // Descobre qual é a menor raridade presente na tabela de probabilidades ativada neste banner
            var sortedValidRarities = new List<GachaRarityProbability>(banner.BasicProbabilities);
            sortedValidRarities.Sort((a, b) => a.Rarity.CompareTo(b.Rarity));
            GachaRarity lowestTableRarity = sortedValidRarities[0].Rarity;

            // 1. Checagem do Hard Pity (Supremo Garantido)
            if (pity.PullsSinceLastSupreme >= banner.HardPityThreshold)
            {
                // Verifica se Supremo está na tabela, senão ignora o pity
                if (banner.BasicProbabilities.Exists(p => p.Rarity == GachaRarity.Supreme))
                    return GachaRarity.Supreme;
            }

            // 2. Checagem do Garantido (Pelo menos um acima da menor raridade listada na tabela)
            bool forceAboveLowest = (pity.PullsSinceLastOverBase >= banner.GuaranteedAboveBaseEvery);

            // 3. Preparando o Sorteio Focado apenas nas Raridades presentes na Tabela
            float extraChance = 0f;
            if (pity.PullsSinceLastSupreme >= banner.SoftPityThreshold)
            {
                int over = pity.PullsSinceLastSupreme - banner.SoftPityThreshold;
                extraChance = over * 4.5f;
            }

            // Precisamos somar o total válido de chances para esse tiro específico (evita erros se a chance não somar 100%)
            float totalValidChance = 0f;
            foreach (var prob in banner.BasicProbabilities)
            {
                if (forceAboveLowest && prob.Rarity == lowestTableRarity)
                    continue; // Pula a pior raridade no tiro garantido

                float c = prob.BaseChance;
                if (prob.Rarity == GachaRarity.Supreme) c += extraChance;
                totalValidChance += c;
            }

            // Se forçando 'AboveLowest' nos deixou sem opções (ex: um banner que só tem a raridade mais baixa)
            if (totalValidChance <= 0f)
            {
                // Cai de volta para o padrão (ignora forceAboveLowest)
                forceAboveLowest = false;
                foreach (var prob in banner.BasicProbabilities)
                {
                    float c = prob.BaseChance;
                    if (prob.Rarity == GachaRarity.Supreme) c += extraChance;
                    totalValidChance += c;
                }
            }

            // 4. Sorteio ajustado dentro apenas da Tabela de Probabilidades Validada
            float rand = Random.Range(0f, totalValidChance);
            float cumulative = 0f;
            
            foreach (var prob in banner.BasicProbabilities)
            {
                if (forceAboveLowest && prob.Rarity == lowestTableRarity)
                    continue;

                float currentChance = prob.BaseChance;
                if (prob.Rarity == GachaRarity.Supreme)
                    currentChance += extraChance;

                cumulative += currentChance;
                if (rand <= cumulative)
                    return prob.Rarity;
            }

            // Fallback seguro usando sempre uma raridade validada da própria tabela
            return forceAboveLowest && sortedValidRarities.Count > 1 
                ? sortedValidRarities[1].Rarity // Pega a segunda pior raridade
                : lowestTableRarity;
        }

        private GachaRewardEntry SelectRewardFromPool(GachaBannerSO banner, GachaRarity targetRarity, GachaPityState pity)
        {
            var validPool = banner.TotalPool.FindAll(x => x.Rarity == targetRarity);
            
            // Nova Regra: Se a raridade foi Supreme e não há opções na TotalPool principal,
            // tentamos sortear diretamente da pool de escolhas em destaque (SupremeChoices).
            if (targetRarity == GachaRarity.Supreme && validPool.Count == 0 && banner.SupremeChoices != null && banner.SupremeChoices.Count > 0)
            {
                // Inclui todos os Supremes que estiverem na lista de destaques
                validPool = banner.SupremeChoices.FindAll(x => x.Rarity == GachaRarity.Supreme);
                
                // Se esqueceram de colocar 'Supreme' na label lá, usa todas as SupremeChoices pra não quebrar
                if (validPool.Count == 0)
                    validPool = new List<GachaRewardEntry>(banner.SupremeChoices); 
            }

            if (validPool.Count == 0)
                throw new global::System.Exception($"Nao ha itens no banner {banner.BannerID} para a raridade {targetRarity}");

            // Se for Supreme + Tem Path + Perdeu 50/50 anterior
            if (targetRarity == GachaRarity.Supreme && banner.HasEpitomizedPath && pity.Lost5050 && !string.IsNullOrEmpty(pity.SelectedSupremeChoice))
            {
                // Tenta achar na validPool, ou então olha na SupremeChoices caso no gacha rate-up não tenha mesclado.
                var hardGuaranteed = validPool.Find(x => x.GetID() == pity.SelectedSupremeChoice);
                if (hardGuaranteed == null && banner.SupremeChoices != null)
                {
                    hardGuaranteed = banner.SupremeChoices.Find(x => x.GetID() == pity.SelectedSupremeChoice);
                }

                if (hardGuaranteed != null) return hardGuaranteed;
            }

            // Sorteio por Peso Normal
            int totalWeight = 0;
            foreach (var item in validPool) totalWeight += item.Weight;
            
            int rand = Random.Range(0, totalWeight);
            int tempSum = 0;
            foreach(var item in validPool)
            {
                tempSum += item.Weight;
                if (rand <= tempSum) return item;
            }

            return validPool[0]; // fallback
        }

        private void DispatchReward(Account account, GachaRewardEntry entry)
        {
            if (entry.RewardType == GachaRewardType.Unit)
            {
                string unitID = entry.GetID();
                var existingUnit = account.OwnedUnits.Find(x => x.UnitID == unitID);
                if (existingUnit != null)
                {
                    existingUnit.Fragments += 20; // Repetido ganha fragmentos
                }
                else
                {
                    account.OwnedUnits.Add(new RuntimeUnitData(unitID, entry.ItemStars));
                    if (!account.OwnedUnitIDs.Contains(unitID))
                        account.OwnedUnitIDs.Add(unitID); // Legacy keep
                }
            }
            else if (entry.RewardType == GachaRewardType.Pet)
            {
                string petID = entry.GetID();
                // Instancia o Pet Data RNG com base na espécie (simplificado aqui. Depois podemos gerar os stats randomicos de HP/ATK baseados em ranges min/max)
                var newPet = new CelestialCross.Data.Pets.RuntimePetData(petID, "", entry.ItemStars, 100, 10, 10, 10, 5, 0);
                account.OwnedRuntimePets.Add(newPet);
            }
            else if (entry.RewardType == GachaRewardType.Artifact)
            {
                 // Geração do artefato com dados básicos para a pool de gacha
                 if (entry.ArtifactSet != null) {
                    CelestialCross.Artifacts.ArtifactRarity genRarity = entry.ArtifactRarity;
                    CelestialCross.Artifacts.ArtifactType genSlot = (CelestialCross.Artifacts.ArtifactType)UnityEngine.Random.Range(0, 6);
                    CelestialCross.Artifacts.ArtifactStars genStars = (CelestialCross.Artifacts.ArtifactStars)entry.ItemStars;

                    CelestialCross.Artifacts.StatType genMainStat = CelestialCross.Artifacts.StatType.HealthFlat; // Default estático ou randômico depois
                    
                    var newArtifact = new CelestialCross.Artifacts.ArtifactInstanceData
                    {
                        idGUID = global::System.Guid.NewGuid().ToString(),
                        artifactSetId = entry.ArtifactSet.id,
                        slot = genSlot,
                        rarity = genRarity,
                        stars = genStars,
                        currentLevel = 0,
                        mainStat = new CelestialCross.Artifacts.StatModifierData(genMainStat, CelestialCross.Artifacts.ArtifactGenerator.GetMainStatBaseValue(genMainStat, genStars)),
                        subStats = new global::System.Collections.Generic.List<CelestialCross.Artifacts.StatModifierData>()
                    };

                    int substatsCount = CelestialCross.Artifacts.ArtifactGenerator.GetInitialSubstatCount(genRarity);
                    var currentSubstats = new global::System.Collections.Generic.List<CelestialCross.Artifacts.StatModifier>();
                    
                    for (int i = 0; i < substatsCount; i++)
                    {
                        var subType = CelestialCross.Artifacts.ArtifactGenerator.GetRandomSubstatType(genMainStat, currentSubstats);
                        float subValue = CelestialCross.Artifacts.ArtifactGenerator.GenerateSubstatValue(subType, genStars);
                        currentSubstats.Add(new CelestialCross.Artifacts.StatModifier { statType = subType, value = subValue });
                        newArtifact.subStats.Add(new CelestialCross.Artifacts.StatModifierData(subType, subValue));
                    }

                    account.OwnedArtifacts.Add(newArtifact);
                 }
            }
        }
    }
}