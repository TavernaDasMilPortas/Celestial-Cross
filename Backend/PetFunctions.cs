using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using CelestialCross.Backend.Models;
using CelestialCross.Backend.Services;

namespace CelestialCross.Backend
{
    public class PetFunctions
    {
        private readonly ILogger _logger;
        private static readonly Random rng = new Random();

        public PetFunctions(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PetFunctions>();
            GameDataService.Initialize(); // Ensure data is loaded
        }

        public class PetResult
        {
            public string UUID { get; set; }
            public string SpeciesID { get; set; }
            public string SpeciesName { get; set; }
            public int RarityStars { get; set; }
            public int CurrentLevel { get; set; }
            public int Health { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int Speed { get; set; }
            public int CriticalChance { get; set; }
            public int CriticalDamage { get; set; }
            public int EffectAccuracy { get; set; }
            public int EffectResistance { get; set; }
        }

        private int RollStat(FloatRangeData range, float multiplier)
        {
            if (range == null) return 0;
            float raw = range.min + (float)(rng.NextDouble() * (range.max - range.min));
            return (int)Math.Round(raw * multiplier);
        }

        public PetResult GeneratePetInternal(string speciesId, int stars)
        {
            var species = GameDataService.GetPetSpecies(speciesId);
            if (species == null)
            {
                throw new Exception($"Espécie de pet {speciesId} não encontrada no gamedata.json");
            }

            float multiplier = GameDataService.GetStarMultiplier(stars);

            var pet = new PetResult
            {
                UUID = Guid.NewGuid().ToString(),
                SpeciesID = speciesId,
                SpeciesName = species.name,
                RarityStars = stars,
                CurrentLevel = 1,
                Health = RollStat(species.statRanges.health, multiplier),
                Attack = RollStat(species.statRanges.attack, multiplier),
                Defense = RollStat(species.statRanges.defense, multiplier),
                Speed = RollStat(species.statRanges.speed, multiplier),
                CriticalChance = RollStat(species.statRanges.criticalChance, 1.0f), 
                CriticalDamage = RollStat(species.statRanges.criticalDamage, 1.0f),
                EffectAccuracy = RollStat(species.statRanges.effectAccuracy, 1.0f),
                EffectResistance = RollStat(species.statRanges.effectResistance, 1.0f)
            };
            return pet;
        }

        [Function("GeneratePet")]
        public async Task<HttpResponseData> GeneratePet(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req)
        {
            _logger.LogInformation("GeneratePet function triggered.");

            var (playFabId, context, _) = await AuthHelper.GetContextAsync(req);
            
            if (string.IsNullOrEmpty(playFabId))
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = "Jogador não autenticado." });
                return badResp;
            }

            var args = context?.FunctionArgument;
            if (args == null)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = "Argumentos inválidos." });
                return badResp;
            }

            string speciesId = args.PetSpeciesId;
            int stars = (int)(args.Stars ?? 3);

            try 
            {
                var pet = GeneratePetInternal(speciesId, stars);

                // No Gacha unificado, não precisamos salvar aqui porque o GeneratePet não é chamado pelo cliente para gerar pet e salvar, 
                // ele é chamado apenas para testar ou via painel de admin se precisar.
                // Todo salvamento real acontece no ExecuteGacha no GachaFunctions.cs
                
                var okResp = req.CreateResponse(HttpStatusCode.OK);
                await okResp.WriteAsJsonAsync(pet);
                return okResp;
            }
            catch(Exception ex)
            {
                var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResp.WriteAsJsonAsync(new { ErrorMessage = ex.Message });
                return badResp;
            }
        }
    }
}
