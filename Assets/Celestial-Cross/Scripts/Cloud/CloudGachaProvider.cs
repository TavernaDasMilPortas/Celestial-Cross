using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data;
using CelestialCross.Gacha;

namespace CelestialCross.Cloud
{
    public class CloudGachaProvider : IGachaProvider
    {
        [global::System.Serializable]
        private class GachaRequestData
        {
            public string BannerID;
            public int PullQuantity;
        }

        public class BackendRewardSource
        {
            public string RewardType;
            public string ItemId;
            public string Rarity;
        }

        public class BackendGachaResult
        {
            public BackendRewardSource RewardSource;
            public RuntimeUnitData ResultUnit;
            public CelestialCross.Data.Pets.RuntimePetData ResultPet;
            public CelestialCross.Artifacts.ArtifactInstanceData ResultArtifact;
            public bool IsDuplicateUnit;
            public int RolledStars;
            public string RolledArtifactRarity;
        }

        public class GachaResponse
        {
            public bool Success;
            public string ErrorMessage;
            public List<BackendGachaResult> Results;
            public Account AccountData; 
        }

        public async Task<List<RuntimeGachaResult>> PullAsync(Account account, GachaBannerSO banner, int times)
        {
            var requestData = new GachaRequestData
            {
                BannerID = banner.BannerID,
                PullQuantity = times
            };

            Debug.Log($"[CloudGachaProvider] Solicitando {times} pulls no banner {banner.BannerID} via Azure Functions...");

            var rawResponse = await NetworkGuard.Instance.ExecuteWithGuardAsync<GachaResponse>("ExecuteGacha", requestData);

            if (rawResponse == null || !rawResponse.Success)
            {
                Debug.LogWarning($"[CloudGachaProvider] Erro na nuvem: {rawResponse?.ErrorMessage}. Fallback para geração local.");
                return await GachaService.Instance.ExecutePullsInternalAsync(account, banner, times);
            }

            // 1. Sincroniza a conta local com os dados atualizados pelo servidor
            if (rawResponse.AccountData != null)
            {
                AccountManager.Instance.PlayerAccount.StarMaps = rawResponse.AccountData.StarMaps;
                AccountManager.Instance.PlayerAccount.GachaPityStates = rawResponse.AccountData.GachaPityStates;
                AccountManager.Instance.PlayerAccount.OwnedUnits = rawResponse.AccountData.OwnedUnits;
                AccountManager.Instance.PlayerAccount.OwnedRuntimePets = rawResponse.AccountData.OwnedRuntimePets;
                AccountManager.Instance.PlayerAccount.OwnedArtifacts = rawResponse.AccountData.OwnedArtifacts;
                
                // Força o salvamento local para manter o cache 
                AccountManager.Instance.SaveAccount();
            }

            // 2. Monta os resultados formatados para a UI
            List<RuntimeGachaResult> finalResults = new List<RuntimeGachaResult>();
            if (rawResponse.Results != null)
            {
                foreach (var r in rawResponse.Results)
                {
                    GachaRewardEntry entry = banner.TotalPool.Find(e => e.GetID() == r.RewardSource.ItemId);
                    if (entry == null && banner.SupremeChoices != null)
                        entry = banner.SupremeChoices.Find(e => e.GetID() == r.RewardSource.ItemId);
                    
                    RuntimeGachaResult runtimeResult = null;
                    if (r.RewardSource.RewardType == "Unit" || r.RewardSource.RewardType == "Item")
                    {
                        runtimeResult = new RuntimeGachaResult(entry, r.ResultUnit, r.IsDuplicateUnit) { RolledStars = r.RolledStars };
                    }
                    else if (r.RewardSource.RewardType == "Pet")
                    {
                        runtimeResult = new RuntimeGachaResult(entry, r.ResultPet, false) { RolledStars = r.RolledStars };
                    }
                    else if (r.RewardSource.RewardType == "Artifact")
                    {
                        global::System.Enum.TryParse(r.RolledArtifactRarity, out CelestialCross.Artifacts.ArtifactRarity parsedRarity);
                        runtimeResult = new RuntimeGachaResult(entry, r.ResultArtifact, false) { RolledStars = r.RolledStars, RolledArtifactRarity = parsedRarity };
                    }
                    
                    if (runtimeResult != null) finalResults.Add(runtimeResult);
                }
            }

            return finalResults;
        }
    }
}
