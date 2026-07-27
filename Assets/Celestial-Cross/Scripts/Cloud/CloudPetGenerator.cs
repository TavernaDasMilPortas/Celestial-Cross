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
        }

        /// <summary>
        /// Solicita ao servidor (PlayFab) a geração de um novo Pet com IVs e status aleatórios.
        /// Retorna a instância gerada já validada pela nuvem.
        /// </summary>
        public static async Task<RuntimePetData> GeneratePetAsync(PetSpeciesSO species, int stars)
        {
            var req = new PetGenerationRequest { 
                PetSpeciesId = species.id, 
                Stars = stars
            };
            
            Debug.Log($"[CloudPetGenerator] Solicitando Pet na nuvem: Species={species.id}, Stars={stars}");
            var pet = await NetworkGuard.Instance.ExecuteWithGuardAsync<RuntimePetData>("GeneratePet", req);

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
