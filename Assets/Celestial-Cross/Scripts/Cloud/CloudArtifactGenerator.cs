using System.Threading.Tasks;
using UnityEngine;
using CelestialCross.Artifacts;

namespace CelestialCross.Cloud
{
    public static class CloudArtifactGenerator
    {
        [global::System.Serializable]
        public class ArtifactGenerationRequest
        {
            public string ArtifactSetId;
            public ArtifactRarity Rarity;
            public ArtifactStars Stars;
        }

        /// <summary>
        /// Solicita ao servidor (PlayFab) a geração de um novo artefato.
        /// Retorna a instância gerada já validada pela nuvem.
        /// </summary>
        public static async Task<ArtifactInstanceData> GenerateArtifactAsync(string setId, ArtifactRarity rarity, ArtifactStars stars)
        {
            var req = new ArtifactGenerationRequest { ArtifactSetId = setId, Rarity = rarity, Stars = stars };
            
            Debug.Log($"[CloudArtifactGenerator] Solicitando artefato na nuvem: Set={setId}, Rarity={rarity}, Stars={stars}");
            var artifact = await NetworkGuard.Instance.ExecuteWithGuardAsync<ArtifactInstanceData>("GenerateArtifact", req);

            if (artifact == null)
            {
                Debug.LogWarning("[CloudArtifactGenerator] Falha ao comunicar com a nuvem (PlayFab). Retornando artefato gerado localmente como fallback.");
                return GenerateArtifactFallbackLocal(setId, rarity, stars);
            }

            return artifact;
        }

        private static ArtifactInstanceData GenerateArtifactFallbackLocal(string setId, ArtifactRarity rarity, ArtifactStars stars)
        {
            ArtifactType genSlot = (ArtifactType)UnityEngine.Random.Range(0, 6);
            StatType genMainStat = StatType.HealthFlat; // Simplificação para o mock
            
            var newArtifact = new ArtifactInstanceData
            {
                idGUID = global::System.Guid.NewGuid().ToString(),
                artifactSetId = setId,
                slot = genSlot,
                rarity = rarity,
                stars = stars,
                currentLevel = 0,
                mainStat = new StatModifierData(genMainStat, ArtifactGenerator.GetMainStatBaseValue(genMainStat, stars)),
                subStats = new global::System.Collections.Generic.List<StatModifierData>()
            };

            int substatsCount = ArtifactGenerator.GetInitialSubstatCount(rarity);
            var currentSubstats = new global::System.Collections.Generic.List<StatModifier>();
            
            for (int i = 0; i < substatsCount; i++)
            {
                var subType = ArtifactGenerator.GetRandomSubstatType(genMainStat, currentSubstats);
                float subValue = ArtifactGenerator.GenerateSubstatValue(subType, stars);
                currentSubstats.Add(new StatModifier { statType = subType, value = subValue });
                newArtifact.subStats.Add(new StatModifierData(subType, subValue));
            }

            return newArtifact;
        }
    }
}
