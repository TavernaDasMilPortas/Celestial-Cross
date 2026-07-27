using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using CelestialCross.Backend.Models;

namespace CelestialCross.Backend.Services
{
    public static class GameDataService
    {
        private static MasterGameData _masterData;

        public static void Initialize()
        {
            if (_masterData != null) return;

            string jsonPath = Path.Combine(Environment.CurrentDirectory, "gamedata.json");
            if (!File.Exists(jsonPath))
            {
                // Fallback for local debugging depending on where function starts
                jsonPath = Path.Combine(AppContext.BaseDirectory, "gamedata.json");
            }

            if (File.Exists(jsonPath))
            {
                string json = File.ReadAllText(jsonPath);
                _masterData = JsonConvert.DeserializeObject<MasterGameData>(json);
                Console.WriteLine($"[GameDataService] Dados carregados com sucesso de {jsonPath}. Pets: {_masterData.petSpecies.Count}, Banners: {_masterData.banners.Count}");
            }
            else
            {
                Console.WriteLine($"[GameDataService] ERRO: Arquivo gamedata.json não encontrado em {jsonPath}");
                _masterData = new MasterGameData(); // Empty fallback to avoid null ref
            }
        }

        public static PetSpeciesConfigData GetPetSpecies(string id)
        {
            Initialize();
            return _masterData?.petSpecies?.FirstOrDefault(p => p.id == id);
        }

        public static float GetStarMultiplier(int stars)
        {
            Initialize();
            string key = stars.ToString();
            var entry = _masterData?.starMultipliers?.FirstOrDefault(k => k.key == key);
            return entry != null ? entry.value : 1.0f;
        }

        public static ArtifactTuningConfigData GetArtifactTuning()
        {
            Initialize();
            return _masterData?.artifactTuning;
        }

        public static GachaBannerConfigData GetBanner(string bannerId)
        {
            Initialize();
            return _masterData?.banners?.FirstOrDefault(b => b.bannerId == bannerId);
        }
    }
}
