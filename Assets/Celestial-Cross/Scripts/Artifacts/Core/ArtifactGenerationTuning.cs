using System;
using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Artifacts
{
    [CreateAssetMenu(
        fileName = "ArtifactGenerationTuning",
        menuName = "Celestial Cross/Artifacts/Artifact Generation Tuning",
        order = 0)]
    public class ArtifactGenerationTuning : ScriptableObject
    {
        [Serializable]
        public struct IntRange
        {
            public int min;
            public int max;

            public IntRange(int min, int max)
            {
                this.min = min;
                this.max = max;
            }

            public int GetRandomInclusive()
            {
                int a = min;
                int b = max;
                if (b < a)
                {
                    int temp = a;
                    a = b;
                    b = temp;
                }

                // Unity int Random.Range is max-exclusive.
                return UnityEngine.Random.Range(a, b + 1);
            }
        }

        [Serializable]
        public struct FloatRange
        {
            public float min;
            public float max;

            public FloatRange(float min, float max)
            {
                this.min = min;
                this.max = max;
            }

            public float GetRandom()
            {
                float a = min;
                float b = max;
                if (b < a)
                {
                    float temp = a;
                    a = b;
                    b = temp;
                }

                return UnityEngine.Random.Range(a, b);
            }
        }

        [Serializable]
        public class StatRanges
        {
            public StatType statType;

            // Indexed by stars-1 (0..5)
            public FloatRange[] mainBaseByStars = new FloatRange[6];
            public FloatRange[] mainUpgradeByStars = new FloatRange[6];
            public FloatRange[] subInitialByStars = new FloatRange[6];
            public FloatRange[] subUpgradeByStars = new FloatRange[6];
        }

        [Header("Master Switch")]
        public bool useTuning = true;

        [Header("Initial Substat Count (by rarity)")]
        public IntRange commonInitialSubstats = new IntRange(0, 1);
        public IntRange uncommonInitialSubstats = new IntRange(1, 2);
        public IntRange rareInitialSubstats = new IntRange(2, 3);
        public IntRange epicInitialSubstats = new IntRange(3, 3);
        public IntRange legendaryInitialSubstats = new IntRange(4, 4);

        [Header("Stat Value Ranges (by stars)")]
        public List<StatRanges> statRanges = new List<StatRanges>();

        private void OnValidate()
        {
            EnsureAllStatsPresent();

            for (int i = 0; i < statRanges.Count; i++)
            {
                var ranges = statRanges[i];
                if (ranges == null)
                    continue;

                ranges.mainBaseByStars = EnsureLen6(ranges.mainBaseByStars);
                ranges.mainUpgradeByStars = EnsureLen6(ranges.mainUpgradeByStars);
                ranges.subInitialByStars = EnsureLen6(ranges.subInitialByStars);
                ranges.subUpgradeByStars = EnsureLen6(ranges.subUpgradeByStars);
            }
        }

        public int GetInitialSubstatCount(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common: return commonInitialSubstats.GetRandomInclusive();
                case ArtifactRarity.Uncommon: return uncommonInitialSubstats.GetRandomInclusive();
                case ArtifactRarity.Rare: return rareInitialSubstats.GetRandomInclusive();
                case ArtifactRarity.Epic: return epicInitialSubstats.GetRandomInclusive();
                case ArtifactRarity.Legendary: return legendaryInitialSubstats.GetRandomInclusive();
                default: return 0;
            }
        }

        public bool TryGetRanges(StatType statType, out StatRanges ranges)
        {
            for (int i = 0; i < statRanges.Count; i++)
            {
                if (statRanges[i] != null && statRanges[i].statType == statType)
                {
                    ranges = statRanges[i];
                    return true;
                }
            }

            ranges = null;
            return false;
        }

        public FloatRange GetMainBaseRange(StatType statType, int stars)
        {
            return GetRange(statType, stars, RangeKind.MainBase);
        }

        public FloatRange GetMainUpgradeRange(StatType statType, int stars)
        {
            return GetRange(statType, stars, RangeKind.MainUpgrade);
        }

        public FloatRange GetSubInitialRange(StatType statType, int stars)
        {
            return GetRange(statType, stars, RangeKind.SubInitial);
        }

        public FloatRange GetSubUpgradeRange(StatType statType, int stars)
        {
            return GetRange(statType, stars, RangeKind.SubUpgrade);
        }

        private enum RangeKind
        {
            MainBase,
            MainUpgrade,
            SubInitial,
            SubUpgrade
        }

        private FloatRange GetRange(StatType statType, int stars, RangeKind kind)
        {
            if (!TryGetRanges(statType, out StatRanges ranges) || ranges == null)
                return new FloatRange(0, 0);

            int index = Mathf.Clamp(stars - 1, 0, 5);

            switch (kind)
            {
                case RangeKind.MainBase: return SafeGet(ranges.mainBaseByStars, index);
                case RangeKind.MainUpgrade: return SafeGet(ranges.mainUpgradeByStars, index);
                case RangeKind.SubInitial: return SafeGet(ranges.subInitialByStars, index);
                case RangeKind.SubUpgrade: return SafeGet(ranges.subUpgradeByStars, index);
                default: return new FloatRange(0, 0);
            }
        }

        private static FloatRange SafeGet(FloatRange[] array, int index)
        {
            if (array == null || array.Length < 6)
                return new FloatRange(0, 0);

            return array[Mathf.Clamp(index, 0, array.Length - 1)];
        }

        private static FloatRange[] EnsureLen6(FloatRange[] array)
        {
            if (array != null && array.Length == 6)
                return array;

            var newArray = new FloatRange[6];
            if (array != null)
            {
                int copy = Mathf.Min(array.Length, 6);
                for (int i = 0; i < copy; i++)
                    newArray[i] = array[i];
            }
            return newArray;
        }

        public void EnsureAllStatsPresent()
        {
            var allTypes = (StatType[])Enum.GetValues(typeof(StatType));

            for (int i = 0; i < allTypes.Length; i++)
            {
                StatType type = allTypes[i];
                if (!TryGetRanges(type, out _))
                {
                    statRanges.Add(new StatRanges
                    {
                        statType = type,
                        mainBaseByStars = new FloatRange[6],
                        mainUpgradeByStars = new FloatRange[6],
                        subInitialByStars = new FloatRange[6],
                        subUpgradeByStars = new FloatRange[6]
                    });
                }
            }
        }

        public void ResetToGeneratorDefaults()
        {
            EnsureAllStatsPresent();

            commonInitialSubstats = new IntRange(0, 1);
            uncommonInitialSubstats = new IntRange(1, 2);
            rareInitialSubstats = new IntRange(2, 3);
            epicInitialSubstats = new IntRange(3, 3);
            legendaryInitialSubstats = new IntRange(4, 4);

            for (int i = 0; i < statRanges.Count; i++)
            {
                StatRanges ranges = statRanges[i];
                if (ranges == null)
                    continue;

                for (int stars = 1; stars <= 6; stars++)
                {
                    int index = stars - 1;

                    bool isPercentLike = IsPercentLikeStat(ranges.statType);

                    // Main base (old: deterministic)
                    float valueMultiplier = stars * 1.5f;
                    float rawMainBase = isPercentLike ? 5.0f + valueMultiplier : 100f * valueMultiplier;
                    float mainBase = Mathf.Round(rawMainBase);
                    ranges.mainBaseByStars[index] = new FloatRange(mainBase, mainBase);

                    // Sub initial (old: RNG range)
                    float minVal = stars * 1.0f;
                    float maxVal = stars * 2.0f;
                    float subMin = isPercentLike ? (minVal * 0.5f) : (minVal * 10f);
                    float subMax = isPercentLike ? (maxVal * 0.5f) : (maxVal * 10f);
                    ranges.subInitialByStars[index] = new FloatRange(subMin, subMax);

                    // Main upgrade (old: deterministic)
                    float inc = stars * 1.2f;
                    float rawMainInc = isPercentLike ? (inc * 0.5f) : (inc * 20f);
                    float mainInc = Mathf.Max(1f, Mathf.Round(rawMainInc));
                    ranges.mainUpgradeByStars[index] = new FloatRange(mainInc, mainInc);

                    // Sub upgrade (old: RNG range)
                    float minUpgrade = stars * 0.5f;
                    float maxUpgrade = stars * 1.5f;
                    float upMin = isPercentLike ? (minUpgrade * 0.5f) : (minUpgrade * 5f);
                    float upMax = isPercentLike ? (maxUpgrade * 0.5f) : (maxUpgrade * 5f);
                    ranges.subUpgradeByStars[index] = new FloatRange(upMin, upMax);
                }
            }
        }

        public static bool IsPercentLikeStat(StatType statType)
        {
            // Keep this consistent with how you display/interpret stats.
            return statType.ToString().Contains("Percent") ||
                   statType == StatType.CriticalRate ||
                   statType == StatType.CriticalDamage ||
                   statType == StatType.EffectResistance ||
                   statType == StatType.EffectHitRate;
        }
    }
}
