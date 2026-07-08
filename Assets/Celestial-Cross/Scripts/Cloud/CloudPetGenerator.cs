using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Data.Pets;

namespace CelestialCross.Cloud
{
    public static class CloudPetGenerator
    {
        [global::System.Serializable]
        public class PetGenerationRequest
        {
            public string PetSpeciesId;
            public int Stars;
            
            public float MinHp, MaxHp;
            public float MinAtk, MaxAtk;
            public float MinDef, MaxDef;
            public float MinSpd, MaxSpd;
            public float MinCritRate, MaxCritRate;
            public float MinCritDmg, MaxCritDmg;
            public float MinEffRes, MaxEffRes;
            public float MinEffAcc, MaxEffAcc;
        }

        /// <summary>
        /// Solicita ao servidor (PlayFab) a geração de um novo Pet com IVs e status aleatórios.
        /// Retorna a instância gerada já validada pela nuvem.
        /// </summary>
        public static async Task<RuntimePetData> GeneratePetAsync(PetSpeciesSO species, int stars)
        {
            var req = new PetGenerationRequest { 
                PetSpeciesId = species.id, 
                Stars = stars,
                MinHp = species.MinBaseHealth, MaxHp = species.MaxBaseHealth,
                MinAtk = species.MinBaseAttack, MaxAtk = species.MaxBaseAttack,
                MinDef = species.MinBaseDefense, MaxDef = species.MaxBaseDefense,
                MinSpd = species.MinBaseSpeed, MaxSpd = species.MaxBaseSpeed,
                MinCritRate = species.MinBaseCriticalChance, MaxCritRate = species.MaxBaseCriticalChance,
                MinCritDmg = species.MinBaseCriticalDamage, MaxCritDmg = species.MaxBaseCriticalDamage,
                MinEffRes = species.MinBaseEffectResistance, MaxEffRes = species.MaxBaseEffectResistance,
                MinEffAcc = species.MinBaseEffectAccuracy, MaxEffAcc = species.MaxBaseEffectAccuracy
            };
            
            Debug.Log($"[CloudPetGenerator] Solicitando Pet na nuvem: Species={species.id}, Stars={stars}");
            var pet = await PlayFabCloudFunctionCaller.ExecuteFunctionAsync<RuntimePetData>("GeneratePet", req);

            if (pet == null)
            {
                Debug.LogWarning("[CloudPetGenerator] Falha ao comunicar com a nuvem. Retornando Pet gerado localmente como fallback.");
                return GeneratePetFallbackLocal(species.id, stars);
            }

            return pet;
        }

        private static RuntimePetData GeneratePetFallbackLocal(string speciesId, int stars)
        {
            // Substitui o RNG local por um mock padrão para não quebrar a UI
            return new RuntimePetData(
                speciesId, 
                string.Empty, // Nickname vazio
                stars, 
                100, // maxHealth
                10,  // attack
                10,  // defense
                10,  // speed
                5,   // critRate
                50,  // critDamage
                0,   // effectRes
                0    // effectHit
            );
        }
    }
}
