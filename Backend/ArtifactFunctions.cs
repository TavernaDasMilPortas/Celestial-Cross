using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using CelestialCross.Backend.Models;
using CelestialCross.Backend.Services;

namespace CelestialCross.Backend
{
    public class ArtifactFunctions
    {
        private readonly ILogger _logger;
        private static readonly Random rng = new Random();

        public ArtifactFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ArtifactFunctions>();
            GameDataService.Initialize();
        }

        public class StatModifierData
        {
            public int statType { get; set; }
            public float value { get; set; }
        }

        public class ArtifactResult
        {
            public string idGUID { get; set; }
            public string artifactSetId { get; set; }
            public int slot { get; set; }
            public int rarity { get; set; }
            public int stars { get; set; }
            public int currentLevel { get; set; }
            public StatModifierData mainStat { get; set; }
            public List<StatModifierData> subStats { get; set; }
        }

        private float RollStatValue(FloatRangeData range)
        {
            if (range == null) return 0;
            return range.min + (float)(rng.NextDouble() * (range.max - range.min));
        }
        
        private string ParseRarity(int rarityInt)
        {
            string[] rarities = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            if (rarityInt >= 0 && rarityInt < rarities.Length) return rarities[rarityInt];
            return "Common";
        }
        
        private string ParseSlot(int slotInt)
        {
            string[] slots = { "Helmet", "Chestplate", "Gloves", "Boots", "Necklace", "Ring" };
            if (slotInt >= 0 && slotInt < slots.Length) return slots[slotInt];
            return "Helmet";
        }
        
        private int ParseStatTypeEnum(string statType)
        {
            string[] stats = { "HealthFlat", "HealthPercent", "AttackFlat", "AttackPercent", "DefenseFlat", "DefensePercent", "Speed", "CriticalRate", "CriticalDamage", "EffectHitRate", "EffectResistance" };
            for(int i=0; i<stats.Length; i++) {
                if(stats[i] == statType) return i;
            }
            return 0; // Default to HealthFlat
        }

        public ArtifactResult GenerateArtifactInternal(string setId, int rarity, int stars, int? forcedSlot = null)
        {
            var tuning = GameDataService.GetArtifactTuning();
            if (tuning == null) throw new Exception("ArtifactTuning não carregado.");

            // 1. Determine Slot
            int slotIndex = forcedSlot ?? rng.Next(0, 6);
            string slotStr = ParseSlot(slotIndex);

            // 2. Determine Main Stat based on Slot Restrictions
            var restriction = tuning.allowedMainStatsPerSlot?.FirstOrDefault(r => r.key == slotStr);
            List<string> allowedStats = restriction?.value;
            if (allowedStats == null || allowedStats.Count == 0)
            {
                allowedStats = new List<string> { "HealthFlat" }; // Fallback
            }
            string mainStatStr = allowedStats[rng.Next(0, allowedStats.Count)];
            int mainStatEnum = ParseStatTypeEnum(mainStatStr);

            // 3. Roll Main Stat Value
            var statConfig = tuning.statRanges.FirstOrDefault(s => s.statType == mainStatStr);
            float mainValue = 0f;
            if (statConfig != null && stars >= 1 && stars <= 6)
            {
                var range = statConfig.mainBaseByStars[stars - 1];
                mainValue = (float)Math.Round(RollStatValue(range));
            }

            // 4. Determine Substats Count
            string rarityStr = ParseRarity(rarity);
            var subCountRange = tuning.initialSubstatCounts.FirstOrDefault(r => r.key == rarityStr)?.value;
            int subCount = 0;
            if (subCountRange != null)
            {
                subCount = rng.Next(subCountRange.min, subCountRange.max + 1);
            }

            // 5. Roll Substats (No duplicates)
            var subStatsList = new List<StatModifierData>();
            var availableSubStats = tuning.statRanges.Select(s => s.statType).Where(s => s != mainStatStr).ToList();
            
            for (int i = 0; i < subCount; i++)
            {
                if (availableSubStats.Count == 0) break;
                int rndIndex = rng.Next(0, availableSubStats.Count);
                string subStatStr = availableSubStats[rndIndex];
                availableSubStats.RemoveAt(rndIndex);

                var subConfig = tuning.statRanges.FirstOrDefault(s => s.statType == subStatStr);
                float subValue = 0f;
                if (subConfig != null && stars >= 1 && stars <= 6)
                {
                    var range = subConfig.subInitialByStars[stars - 1];
                    subValue = (float)Math.Round(RollStatValue(range));
                }

                subStatsList.Add(new StatModifierData { statType = ParseStatTypeEnum(subStatStr), value = subValue });
            }

            return new ArtifactResult
            {
                idGUID = Guid.NewGuid().ToString(),
                artifactSetId = setId,
                slot = slotIndex,
                rarity = rarity,
                stars = stars,
                currentLevel = 0,
                mainStat = new StatModifierData { statType = mainStatEnum, value = mainValue },
                subStats = subStatsList
            };
        }

        [Function("GenerateArtifact")]
        public async Task<HttpResponseData> GenerateArtifact(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("GenerateArtifact function triggered.");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var args = context["FunctionArgument"];
            if (args == null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = "Argumentos inválidos." });
                return badResp;
            }

            string setId = args["SetID"]?.ToString() ?? args["ArtifactSetId"]?.ToString() ?? "Set_Gladiator";
            
            int rarity = 0;
            if (args["Rarity"] != null)
            {
                if (args["Rarity"].Type == Newtonsoft.Json.Linq.JTokenType.Integer)
                    rarity = args["Rarity"].ToObject<int>();
                else if (args["Rarity"].Type == Newtonsoft.Json.Linq.JTokenType.String)
                {
                    string rStr = args["Rarity"].ToString();
                    if (rStr == "Common") rarity = 0;
                    else if (rStr == "Uncommon") rarity = 1;
                    else if (rStr == "Rare") rarity = 2;
                    else if (rStr == "Epic") rarity = 3;
                    else if (rStr == "Legendary") rarity = 4;
                    else int.TryParse(rStr, out rarity);
                }
            }

            int stars = 1;
            if (args["Stars"] != null)
            {
                if (args["Stars"].Type == Newtonsoft.Json.Linq.JTokenType.Integer)
                    stars = args["Stars"].ToObject<int>();
                else if (args["Stars"].Type == Newtonsoft.Json.Linq.JTokenType.String)
                {
                    string sStr = args["Stars"].ToString();
                    if (sStr == "One") stars = 1;
                    else if (sStr == "Two") stars = 2;
                    else if (sStr == "Three") stars = 3;
                    else if (sStr == "Four") stars = 4;
                    else if (sStr == "Five") stars = 5;
                    else if (sStr == "Six") stars = 6;
                    else int.TryParse(sStr, out stars);
                }
            }

            try
            {
                var artifact = GenerateArtifactInternal(setId, rarity, stars);
                
                // TODO: FASE 3 - Migrar o salvamento para o PlayFab
                
                var okResp = req.CreateResponse(HttpStatusCode.OK);
                await okResp.WriteAsJsonAsync(artifact);
                return okResp;
            }
            catch (Exception ex)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = ex.Message });
                return badResp;
            }
        }
    }
}
