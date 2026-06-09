using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Artifacts;
using CelestialCross.Data.Dungeon;

namespace CelestialCross.System
{
    public static class ArtifactLootService
    {
        // NOVO: Exposto para uso na LootTableSO Genérica
        public static ArtifactInstanceData GenerateSingleFromMatrix(List<ArtifactSet> allowedSets, ArtifactDropMatrix matrix)
        {
            if (allowedSets == null || allowedSets.Count == 0) return null;
            return GenerateSingleArtifact(allowedSets, matrix);
        }

        private static ArtifactInstanceData GenerateSingleArtifact(List<ArtifactSet> allowedSets, ArtifactDropMatrix matrix)
        {
            // 1. Sortear Set
            ArtifactSet selectedSet = allowedSets[Random.Range(0, allowedSets.Count)];

            // 2. Sortear Slot (Type)
            ArtifactType selectedSlot = (ArtifactType)Random.Range(0, 6);

            // 3. RNG Raridade
            ArtifactRarity rarity = RollRarity(matrix);

            // 4. RNG Estrelas
            ArtifactStars stars = RollStars(matrix);

            StatType mainStat = DetermineMainStatForSlot(selectedSlot);

            // 6. Instanciar e Preencher Runtime Data
            var artifact = new ArtifactInstanceData
            {
                idGUID = global::System.Guid.NewGuid().ToString(),
                artifactSetId = selectedSet.id,
                slot = selectedSlot,
                rarity = rarity,
                stars = (ArtifactStars)stars,
                currentLevel = 0,
                mainStat = new StatModifierData(mainStat, ArtifactGenerator.GetMainStatBaseValue(mainStat, stars)),
                subStats = new List<StatModifierData>()
            };

            // 7. Sortear Substats
            int substatsCount = ArtifactGenerator.GetInitialSubstatCount(rarity);
            List<StatModifier> currentSubstats = new List<StatModifier>();
            
            for (int i = 0; i < substatsCount; i++)
            {
                StatType subType = ArtifactGenerator.GetRandomSubstatType(mainStat, currentSubstats);
                float subValue = ArtifactGenerator.GenerateSubstatValue(subType, stars);
                currentSubstats.Add(new StatModifier { statType = subType, value = subValue });
                artifact.subStats.Add(new StatModifierData(subType, subValue));
            }

            return artifact;
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

        private static StatType DetermineMainStatForSlot(ArtifactType slot)
        {
            List<StatType> pool = new List<StatType>
            {
                StatType.HealthFlat, StatType.HealthPercent, StatType.AttackFlat, StatType.AttackPercent, 
                StatType.DefenseFlat, StatType.DefensePercent, StatType.CriticalRate, StatType.CriticalDamage
            };
            return pool[Random.Range(0, pool.Count)];
        }
    }
}
