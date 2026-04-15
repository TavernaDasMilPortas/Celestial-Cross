using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data;

namespace CelestialCross.Gacha
{
    public class GachaService
    {
        public static GachaService Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new GachaService();
            }
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

        public List<GachaRewardEntry> PerformPulls(Account account, GachaBannerSO banner, int times)
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
                    }
                    else if (banner.HasEpitomizedPath)
                    {
                        pityState.Lost5050 = true; // Perdeu pro general pool
                    }
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
             // 1. Checagem do Hard Pity (Supremo Garantido)
            if (pity.PullsSinceLastSupreme >= banner.HardPityThreshold)
                return GachaRarity.Supreme;

            // 2. Checagem do Guarantido 10 (Pelo menos um Uncommon+)
            bool forceUncommonPlus = (pity.PullsSinceLastOverBase >= banner.GuaranteedAboveBaseEvery);

            // 3. Checagem de Soft Pity e Sorteio Geral
            float rand = Random.Range(0f, 100f);
            float cumulative = 0f;
            
            // Soft Pity Ramp
            float extraChance = 0f;
            if (pity.PullsSinceLastSupreme >= banner.SoftPityThreshold)
            {
                int over = pity.PullsSinceLastSupreme - banner.SoftPityThreshold;
                extraChance = over * 4.5f; // Exemplo de curva de soft pity
            }

            foreach (var prob in banner.BasicProbabilities)
            {
                float currentChance = prob.BaseChance;
                
                if (prob.Rarity == GachaRarity.Supreme)
                    currentChance += extraChance;

                if (forceUncommonPlus && prob.Rarity == GachaRarity.Base)
                    continue; // Ignora Base se está no tiro garantido 10

                cumulative += currentChance;
                if (rand <= cumulative)
                    return prob.Rarity;
            }

            return forceUncommonPlus ? GachaRarity.Uncommon : GachaRarity.Base;
        }

        private GachaRewardEntry SelectRewardFromPool(GachaBannerSO banner, GachaRarity targetRarity, GachaPityState pity)
        {
            var validPool = banner.TotalPool.FindAll(x => x.Rarity == targetRarity);
            
            if (validPool.Count == 0)
                throw new global::System.Exception($"Nao ha itens no banner {banner.BannerID} para a raridade {targetRarity}");

            // Se for Supreme + Tem Path + Perdeu 50/50 anterior
            if (targetRarity == GachaRarity.Supreme && banner.HasEpitomizedPath && pity.Lost5050 && !string.IsNullOrEmpty(pity.SelectedSupremeChoice))
            {
                var hardGuaranteed = validPool.Find(x => x.GetID() == pity.SelectedSupremeChoice);
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