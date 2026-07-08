using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using CelestialCross.Artifacts;
using CelestialCross.Data.Dungeon;
using CelestialCross.Cloud;

namespace CelestialCross.System
{
    public static class ArtifactLootService
    {
        // NOVO: Exposto para uso na LootTableSO Genérica e assíncrono
        public static async Task<ArtifactInstanceData> GenerateSingleFromMatrixAsync(List<ArtifactSet> allowedSets, ArtifactDropMatrix matrix)
        {
            if (allowedSets == null || allowedSets.Count == 0) return null;
            return await GenerateCloudArtifactAsync(allowedSets, matrix);
        }

        private static async Task<ArtifactInstanceData> GenerateCloudArtifactAsync(List<ArtifactSet> allowedSets, ArtifactDropMatrix matrix)
        {
            // 1. Sortear Set
            ArtifactSet selectedSet = allowedSets[Random.Range(0, allowedSets.Count)];

            // 2. RNG Raridade
            ArtifactRarity rarity = RollRarity(matrix);

            // 3. RNG Estrelas
            ArtifactStars stars = RollStars(matrix);

            // 4. Solicitar Nuvem
            return await CloudArtifactGenerator.GenerateArtifactAsync(selectedSet.id, rarity, stars);
        }

        private static ArtifactRarity RollRarity(ArtifactDropMatrix matrix)
        {
            float total = matrix.commonChance + matrix.uncommonChance + matrix.rareChance + matrix.epicChance + matrix.legendaryChance;
            float roll = Random.Range(0f, total);

            if (roll < matrix.commonChance) return ArtifactRarity.Common;
            roll -= matrix.commonChance;
            if (roll < matrix.uncommonChance) return ArtifactRarity.Uncommon;
            roll -= matrix.uncommonChance;
            if (roll < matrix.rareChance) return ArtifactRarity.Rare;
            roll -= matrix.rareChance;
            if (roll < matrix.epicChance) return ArtifactRarity.Epic;
            
            return ArtifactRarity.Legendary;
        }

        private static ArtifactStars RollStars(ArtifactDropMatrix matrix)
        {
            float total = matrix.oneStarChance + matrix.twoStarChance + matrix.threeStarChance + matrix.fourStarChance + matrix.fiveStarChance;
            float roll = Random.Range(0f, total);

            if (roll < matrix.oneStarChance) return ArtifactStars.One;
            roll -= matrix.oneStarChance;
            if (roll < matrix.twoStarChance) return ArtifactStars.Two;
            roll -= matrix.twoStarChance;
            if (roll < matrix.threeStarChance) return ArtifactStars.Three;
            roll -= matrix.threeStarChance;
            if (roll < matrix.fourStarChance) return (ArtifactStars)4;
            
            return (ArtifactStars)5; 
        }


    }
}
