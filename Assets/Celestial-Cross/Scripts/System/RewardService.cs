using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Rewards;
using CelestialCross.Data.Dungeon;
using CelestialCross.Artifacts;
using System.Threading.Tasks;
using CelestialCross.Data.Pets;
using CelestialCross.Cloud;

namespace CelestialCross.System
{
    public static class RewardService
    {
        public static async Task<RuntimeReward> CreateRuntimeRewardAsync(List<RewardDefinition> rewards)
        {
            var runtimeReward = new RuntimeReward();
            if (rewards == null || rewards.Count == 0) return runtimeReward;

            foreach (var reward in rewards)
            {
                if (reward == null) continue;

                // Add to SourceDefinitions for UI tracking
                runtimeReward.SourceDefinitions.Add(reward);

                switch (reward.Type)
                {
                    case RewardType.Money:
                        runtimeReward.Money += reward.Amount;
                        break;
                    case RewardType.Energy:
                        runtimeReward.Energy += reward.Amount;
                        break;
                    case RewardType.Stardust:
                        runtimeReward.Stardust += reward.Amount;
                        break;
                    case RewardType.XP:
                        runtimeReward.XP += reward.Amount;
                        break;
                    case RewardType.Pet:
                        if (reward.PetRef != null)
                        {
                            for (int i = 0; i < Mathf.Max(1, reward.Amount); i++)
                            {
                                var newPet = await CloudPetGenerator.GeneratePetAsync(reward.PetRef, 3); // Default 3 stars
                                if (newPet != null)
                                {
                                    runtimeReward.GeneratedPets.Add(newPet);
                                }
                            }
                        }
                        break;
                    case RewardType.Artifact:
                        if (reward.ArtifactSetRef != null)
                        {
                            for (int i = 0; i < Mathf.Max(1, reward.Amount); i++)
                            {
                                var art = await CloudArtifactGenerator.GenerateArtifactAsync(reward.ArtifactSetRef.id, ArtifactRarity.Legendary, ArtifactStars.Five);
                                if (art != null)
                                {
                                    runtimeReward.GeneratedArtifacts.Add(art);
                                }
                            }
                        }
                        break;
                    case RewardType.LootTable:
                        if (reward.LootTableRef != null)
                        {
                            for (int i = 0; i < Mathf.Max(1, reward.Amount); i++)
                            {
                                Debug.Log($"[RewardService] Gerando LootTable {reward.LootTableRef.GetType().Name} via RewardDefinition...");
                                int prevArtCount = runtimeReward.GeneratedArtifacts.Count;
                                await reward.LootTableRef.GenerateLootAsync(runtimeReward);
                                Debug.Log($"[RewardService] LootTable gerou {runtimeReward.GeneratedArtifacts.Count - prevArtCount} artefatos.");
                            }
                        }
                        break;
                }
            }

            return runtimeReward;
        }

        public static void ApplyRuntimeRewardToAccount(RuntimeReward reward)
        {
            if (reward == null || AccountManager.Instance == null || AccountManager.Instance.PlayerAccount == null)
                return;

            var account = AccountManager.Instance.PlayerAccount;

            account.Money += reward.Money;
            account.Energy += reward.Energy;
            account.Stardust += reward.Stardust;

            if (reward.GeneratedArtifacts != null && reward.GeneratedArtifacts.Count > 0)
            {
                if (account.OwnedArtifacts == null) account.OwnedArtifacts = new List<ArtifactInstanceData>();
                account.OwnedArtifacts.AddRange(reward.GeneratedArtifacts);
            }
            
            if (reward.GeneratedPets != null && reward.GeneratedPets.Count > 0)
            {
                if (account.OwnedRuntimePets == null) account.OwnedRuntimePets = new List<RuntimePetData>();
                account.OwnedRuntimePets.AddRange(reward.GeneratedPets);
            }

            // For Items and Units, we need to process them directly from the SourceDefinitions
            // because RuntimeReward doesn't currently store raw items or units.
            if (reward.SourceDefinitions != null)
            {
                foreach (var def in reward.SourceDefinitions)
                {
                    if (def == null) continue;
                    if (def.Type == RewardType.Item)
                    {
                        account.AddItem(def.ReferenceID, def.Amount);
                    }
                    else if (def.Type == RewardType.Unit && def.UnitRef != null)
                    {
                        for (int i = 0; i < Mathf.Max(1, def.Amount); i++)
                        {
                            AccountManager.Instance.AddUnitToAccount(def.UnitRef.UnitID);
                        }
                    }
                }
            }

            AccountManager.Instance.SaveAccount();
        }
    }
}
