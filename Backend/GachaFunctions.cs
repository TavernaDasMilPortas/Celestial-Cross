using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ServerModels;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Backend.Models;
using CelestialCross.Backend.Services;

namespace CelestialCross.Backend
{
    public class GachaFunctions
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private static readonly Random rng = new Random();

        public GachaFunctions(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<GachaFunctions>();
            GameDataService.Initialize();
        }

        // ==========================================
        // MODELS DE DADOS DA CONTA (ESPELHO DO UNITY)
        // ==========================================
        public class GachaPityStateData
        {
            public string BannerID { get; set; }
            public int PullsSinceLastOverBase { get; set; }
            public int PullsSinceLastSupreme { get; set; }
            public string SelectedSupremeChoice { get; set; }
            public bool Lost5050 { get; set; }
        }

        public class RuntimeUnitData
        {
            public string UnitID { get; set; }
            public int BaseStars { get; set; }
            public int Level { get; set; }
            public int ConstellationLevel { get; set; }
            public int Fragments { get; set; }
        }

        public class AccountSaveData
        {
            public int StarMaps { get; set; }
            public List<GachaPityStateData> GachaPityStates { get; set; } = new List<GachaPityStateData>();
            public List<RuntimeUnitData> OwnedUnits { get; set; } = new List<RuntimeUnitData>();
            public List<PetFunctions.PetResult> OwnedRuntimePets { get; set; } = new List<PetFunctions.PetResult>();
            public List<ArtifactFunctions.ArtifactResult> OwnedArtifacts { get; set; } = new List<ArtifactFunctions.ArtifactResult>();
            public Dictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>(); // Para insignias
        }

        public class GachaRewardEntryDataResult
        {
            public string RewardType { get; set; }
            public string ItemId { get; set; }
            public string Rarity { get; set; }
        }

        public class RuntimeGachaResultData
        {
            public GachaRewardEntryDataResult RewardSource { get; set; }
            public RuntimeUnitData ResultUnit { get; set; }
            public PetFunctions.PetResult ResultPet { get; set; }
            public ArtifactFunctions.ArtifactResult ResultArtifact { get; set; }
            public bool IsDuplicateUnit { get; set; }
            public int RolledStars { get; set; }
            public string RolledArtifactRarity { get; set; }
        }

        // ==========================================
        // LÓGICA DE RNG E PITY (COPIADA DO CLIENTE E ASSEGURADA NO SERVIDOR)
        // ==========================================

        private string DetermineRarity(GachaBannerConfigData banner, GachaPityStateData pity)
        {
            if (banner.basicProbabilities == null || banner.basicProbabilities.Count == 0) return "Base";

            // hard pity
            if (pity.PullsSinceLastSupreme >= banner.hardPityThreshold && banner.basicProbabilities.Any(p => p.key == "Supreme"))
                return "Supreme";

            bool forceAboveLowest = pity.PullsSinceLastOverBase >= banner.guaranteedAboveBaseEvery;
            
            float extraChance = 0f;
            if (pity.PullsSinceLastSupreme >= banner.softPityThreshold)
            {
                extraChance = (pity.PullsSinceLastSupreme - banner.softPityThreshold) * 4.5f;
            }

            var rarities = new List<string> { "Base", "Rare", "Epic", "Supreme" };
            string lowestRarity = banner.basicProbabilities.OrderBy(p => rarities.IndexOf(p.key)).First().key;

            float totalValidChance = 0f;
            foreach (var prob in banner.basicProbabilities)
            {
                if (forceAboveLowest && prob.key == lowestRarity) continue;
                totalValidChance += prob.value + (prob.key == "Supreme" ? extraChance : 0);
            }

            if (totalValidChance <= 0f)
            {
                forceAboveLowest = false;
                totalValidChance = banner.basicProbabilities.Sum(p => p.value + (p.key == "Supreme" ? extraChance : 0));
            }

            float rand = (float)(rng.NextDouble() * totalValidChance);
            float cumulative = 0f;

            foreach (var prob in banner.basicProbabilities)
            {
                if (forceAboveLowest && prob.key == lowestRarity) continue;
                cumulative += prob.value + (prob.key == "Supreme" ? extraChance : 0);
                if (rand <= cumulative) return prob.key;
            }

            return lowestRarity;
        }

        private GachaRewardEntryData TrySelectRewardFromPool(GachaBannerConfigData banner, string targetRarity, GachaPityStateData pity)
        {
            var validPool = banner.totalPool.Where(x => x.rarity == targetRarity).ToList();

            if (targetRarity == "Supreme" && validPool.Count == 0 && banner.supremeChoices != null)
            {
                validPool = banner.supremeChoices.Where(x => x.rarity == "Supreme").ToList();
                if (validPool.Count == 0) validPool = banner.supremeChoices;
            }

            if (validPool.Count == 0) return null;

            if (targetRarity == "Supreme" && banner.hasEpitomizedPath && pity.Lost5050 && !string.IsNullOrEmpty(pity.SelectedSupremeChoice))
            {
                var hardGuaranteed = validPool.FirstOrDefault(x => x.itemId == pity.SelectedSupremeChoice) ?? 
                                     banner.supremeChoices?.FirstOrDefault(x => x.itemId == pity.SelectedSupremeChoice);
                if (hardGuaranteed != null) return hardGuaranteed;
            }

            int totalWeight = validPool.Sum(x => x.weight);
            if (totalWeight <= 0) return validPool[0];

            int rand = rng.Next(0, totalWeight);
            int tempSum = 0;
            foreach (var item in validPool)
            {
                tempSum += item.weight;
                if (rand <= tempSum) return item;
            }
            return validPool[0];
        }

        // ==========================================
        // FUNÇÃO PRINCIPAL
        // ==========================================

        [Function("ExecuteGacha")]
        public async Task<HttpResponseData> ExecuteGacha(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("ExecuteGacha function triggered.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic context = JsonConvert.DeserializeObject(requestBody);
            
            string playFabId = context?.CallerEntityProfile?.Lineage?.MasterPlayerAccountId;
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var args = context?.FunctionArgument;
            string bannerId = args?.BannerID;
            int pullQuantity = (int)(args?.PullQuantity ?? 1);

            var banner = GameDataService.GetBanner(bannerId);
            if (banner == null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Banner inválido." });
                return badResp;
            }

            // 1. Fetch Account Data from PlayFab
            var apiSettings = new PlayFabApiSettings
            {
                TitleId = Environment.GetEnvironmentVariable("PLAYFAB_TITLE_ID") ?? "SEU_TITLE_ID",
                DeveloperSecretKey = Environment.GetEnvironmentVariable("PLAYFAB_DEV_SECRET_KEY") ?? "SUA_SECRET_KEY"
            };
            var serverApi = new PlayFabServerInstanceAPI(apiSettings);

            var getDataRequest = new GetUserDataRequest { PlayFabId = playFabId, Keys = new List<string> { "AccountData" } };
            var dataResult = await serverApi.GetUserDataAsync(getDataRequest);
            if (dataResult.Error != null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Erro ao ler a conta do jogador no PlayFab." });
                return badResp;
            }

            AccountSaveData accountData = new AccountSaveData();
            if (dataResult.Result.Data != null && dataResult.Result.Data.ContainsKey("AccountData"))
            {
                accountData = JsonConvert.DeserializeObject<AccountSaveData>(dataResult.Result.Data["AccountData"].Value) ?? new AccountSaveData();
            }

            // 2. Validate Funds
            int totalCost = banner.costPerPull * pullQuantity;
            if (accountData.StarMaps < totalCost)
            {
                var okFailResp = req.CreateResponse(HttpStatusCode.OK);
                await okFailResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "StarMaps Insuficientes." });
                return okFailResp;
            }

            accountData.StarMaps -= totalCost;
            var pityState = accountData.GachaPityStates.FirstOrDefault(p => p.BannerID == bannerId);
            if (pityState == null)
            {
                pityState = new GachaPityStateData { BannerID = bannerId };
                accountData.GachaPityStates.Add(pityState);
            }

            // 3. Roll!
            var results = new List<RuntimeGachaResultData>();
            var petFunctions = new PetFunctions(_loggerFactory);
            var artifactFunctions = new ArtifactFunctions(_loggerFactory);

            for (int i = 0; i < pullQuantity; i++)
            {
                pityState.PullsSinceLastSupreme++;
                pityState.PullsSinceLastOverBase++;

                string rolledRarity = DetermineRarity(banner, pityState);
                var rolledReward = TrySelectRewardFromPool(banner, rolledRarity, pityState);

                if (rolledReward == null) continue;

                if (rolledRarity == "Supreme") pityState.PullsSinceLastSupreme = 0;
                if (rolledRarity != "Base") pityState.PullsSinceLastOverBase = 0;

                if (rolledRarity == "Supreme")
                {
                    bool isTargetSupreme = banner.hasEpitomizedPath && rolledReward.itemId == pityState.SelectedSupremeChoice;
                    if (isTargetSupreme)
                    {
                        pityState.Lost5050 = false;
                        pityState.SelectedSupremeChoice = "";
                    }
                    else if (banner.hasEpitomizedPath)
                    {
                        pityState.Lost5050 = true;
                    }
                    pityState.PullsSinceLastSupreme = 0;
                }

                // Dispatch Reward
                var res = new RuntimeGachaResultData
                {
                    RewardSource = new GachaRewardEntryDataResult { RewardType = rolledReward.rewardType, ItemId = rolledReward.itemId, Rarity = rolledReward.rarity }
                };

                int rolledStars = rng.Next(rolledReward.minItemStars, rolledReward.maxItemStars + 1);
                res.RolledStars = rolledStars;

                if (rolledReward.rewardType == "Unit")
                {
                    var existingUnit = accountData.OwnedUnits.FirstOrDefault(u => u.UnitID == rolledReward.itemId);
                    if (existingUnit != null)
                    {
                        existingUnit.Fragments += 20;
                        res.IsDuplicateUnit = true;
                        res.ResultUnit = existingUnit;
                        
                        // Give Insignia
                        string insigniaID = "Item_StellarInsignia"; // Simulado
                        if (!accountData.Inventory.ContainsKey(insigniaID)) accountData.Inventory[insigniaID] = 0;
                        accountData.Inventory[insigniaID]++;
                    }
                    else
                    {
                        var newUnit = new RuntimeUnitData { UnitID = rolledReward.itemId, BaseStars = rolledStars, Level = 1 };
                        accountData.OwnedUnits.Add(newUnit);
                        res.IsDuplicateUnit = false;
                        res.ResultUnit = newUnit;
                    }
                }
                else if (rolledReward.rewardType == "Pet")
                {
                    var newPet = petFunctions.GeneratePetInternal(rolledReward.itemId, rolledStars);
                    accountData.OwnedRuntimePets.Add(newPet);
                    res.ResultPet = newPet;
                }
                else if (rolledReward.rewardType == "Artifact")
                {
                    int rar = rng.Next(rolledReward.minArtifactRarity, rolledReward.maxArtifactRarity + 1);
                    res.RolledArtifactRarity = ParseRarity(rar);
                    var newArt = artifactFunctions.GenerateArtifactInternal(rolledReward.itemId, rar, rolledStars);
                    accountData.OwnedArtifacts.Add(newArt);
                    res.ResultArtifact = newArt;
                }

                results.Add(res);
            }

            // 4. Save back to PlayFab
            var updateDataRequest = new UpdateUserDataRequest
            {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string> { { "AccountData", JsonConvert.SerializeObject(accountData) } }
            };

            var updateResult = await serverApi.UpdateUserDataAsync(updateDataRequest);
            if (updateResult.Error != null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { Success = false, ErrorMessage = "Erro ao salvar a conta após os pulls." });
                return badResp;
            }

            var okResp = req.CreateResponse(HttpStatusCode.OK);
            await okResp.WriteAsJsonAsync(new { Success = true, Results = results, AccountData = accountData });
            return okResp;
        }

        private string ParseRarity(int rarityInt)
        {
            string[] rarities = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            if (rarityInt >= 0 && rarityInt < rarities.Length) return rarities[rarityInt];
            return "Common";
        }
    }
}
