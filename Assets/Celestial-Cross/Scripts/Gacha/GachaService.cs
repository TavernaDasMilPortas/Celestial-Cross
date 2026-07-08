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
                // Agora inicia usando o provedor da nuvem por padrão (preparado para PlayFab)
                Instance._provider = new CelestialCross.Cloud.CloudGachaProvider();
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

        public async Task<List<RuntimeGachaResult>> PerformPullsAsync(Account account, GachaBannerSO banner, int times)
        {
            if (_provider == null) Initialize();
            return await _provider.PullAsync(account, banner, times);
        }

        public async Task<List<RuntimeGachaResult>> ExecutePullsInternalAsync(Account account, GachaBannerSO banner, int times)
        {
            List<RuntimeGachaResult> results = new List<RuntimeGachaResult>();
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
                
                GachaRarity rolledRarity;
                GachaRewardEntry rolledReward;
                var excludedRarities = new HashSet<GachaRarity>();
                int maxRerollAttempts = 10;

                do
                {
                    rolledRarity = DetermineRarity(banner, pityState, excludedRarities);
                    rolledReward = TrySelectRewardFromPool(banner, rolledRarity, pityState);

                    if (rolledReward == null)
                    {
                        excludedRarities.Add(rolledRarity);
                        maxRerollAttempts--;
                    }
                } while (rolledReward == null && maxRerollAttempts > 0);

                if (rolledReward == null)
                {
                    Debug.LogError($"[Gacha] Falha ao encontrar qualquer reward no banner {banner.BannerID} após re-rolls.");
                    continue;
                }
                
                // Reseta os pities se tiver a raridade certa
                if (rolledRarity == GachaRarity.Supreme)
                    pityState.PullsSinceLastSupreme = 0;
                
                if (rolledRarity != GachaRarity.Base)
                    pityState.PullsSinceLastOverBase = 0;

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

                var result = await DispatchRewardAsync(account, rolledReward);
                results.Add(result);
            }

            // Salva a conta
            AccountManager.Instance.SaveAccount();
            return results;
        }

        private GachaRarity DetermineRarity(GachaBannerSO banner, GachaPityState pity, HashSet<GachaRarity> excludedRarities = null)
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
                if (excludedRarities != null && excludedRarities.Contains(prob.Rarity))
                    continue;
                    
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
                    if (excludedRarities != null && excludedRarities.Contains(prob.Rarity))
                        continue;
                        
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
                if (excludedRarities != null && excludedRarities.Contains(prob.Rarity))
                    continue;
                    
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

        private GachaRewardEntry TrySelectRewardFromPool(GachaBannerSO banner, GachaRarity targetRarity, GachaPityState pity)
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
                return null;

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

        private async Task<RuntimeGachaResult> DispatchRewardAsync(Account account, GachaRewardEntry entry)
        {
            if (entry.RewardType == GachaRewardType.Unit)
            {
                int rolledStars = entry.RollItemStars(true);
                string unitID = entry.GetID();
                var existingUnit = account.OwnedUnits.Find(x => x.UnitID == unitID);
                if (existingUnit != null)
                {
                    existingUnit.Fragments += 20; // Repetido ganha fragmentos
                    string insigniaID = CelestialCross.System.ConstellationService.GetInsigniaItemID(unitID);
                    account.AddItem(insigniaID, 1);
                    Debug.Log($"[GachaService] Duplicata de {unitID} recebida na pool. Adicionada 1 Insígnia Estelar e 20 fragmentos.");
                    return new RuntimeGachaResult(entry, existingUnit, true) { RolledStars = rolledStars };
                }
                else
                {
                    var newUnit = new RuntimeUnitData(unitID, rolledStars);
                    account.OwnedUnits.Add(newUnit);
                    if (!account.OwnedUnitIDs.Contains(unitID))
                        account.OwnedUnitIDs.Add(unitID); // Legacy keep
                    return new RuntimeGachaResult(entry, newUnit, false) { RolledStars = rolledStars };
                }
            }
            else if (entry.RewardType == GachaRewardType.Pet)
            {
                int rolledStars = entry.RollItemStars(false);
                string petID = entry.GetID();
                
                var petSpecies = CelestialCross.System.GlobalCatalogs.Instance.petCatalog.GetPetSpecies(petID);
                var newPet = await CelestialCross.Cloud.CloudPetGenerator.GeneratePetAsync(petSpecies, rolledStars);
                account.OwnedRuntimePets.Add(newPet);
                return new RuntimeGachaResult(entry, newPet, false) { RolledStars = rolledStars };
            }
            else if (entry.RewardType == GachaRewardType.Artifact)
            {
                 if (entry.ArtifactSet != null) {
                    CelestialCross.Artifacts.ArtifactRarity genRarity = entry.RollArtifactRarity();
                    int rolledStars = entry.RollItemStars(false);
                    CelestialCross.Artifacts.ArtifactStars genStars = (CelestialCross.Artifacts.ArtifactStars)rolledStars;

                    var newArtifact = await CelestialCross.Cloud.CloudArtifactGenerator.GenerateArtifactAsync(entry.ArtifactSet.id, genRarity, genStars);

                    account.OwnedArtifacts.Add(newArtifact);
                    return new RuntimeGachaResult(entry, newArtifact, false) { RolledStars = rolledStars, RolledArtifactRarity = genRarity };
                 }
            }
            
            return new RuntimeGachaResult(entry, null, false);
        }
    }
}