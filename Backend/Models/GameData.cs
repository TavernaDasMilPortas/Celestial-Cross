using System.Collections.Generic;

namespace CelestialCross.Backend.Models
{
    public class FloatRangeData { public float min; public float max; }
    public class IntRangeData { public int min; public int max; }
    public class KVP_StringFloat { public string key; public float value; }
    public class KVP_StringIntRange { public string key; public IntRangeData value; }
    public class KVP_StringStringList { public string key; public List<string> value; }

    public class PetStatRangesData
    {
        public FloatRangeData health;
        public FloatRangeData attack;
        public FloatRangeData defense;
        public FloatRangeData speed;
        public FloatRangeData criticalChance;
        public FloatRangeData criticalDamage;
        public FloatRangeData effectAccuracy;
        public FloatRangeData effectResistance;
    }

    public class PetSpeciesConfigData
    {
        public string id;
        public string name;
        public PetStatRangesData statRanges;
    }

    public class StatTypeData
    {
        public string statType;
        public List<FloatRangeData> mainBaseByStars;
        public List<FloatRangeData> mainUpgradeByStars;
        public List<FloatRangeData> subInitialByStars;
        public List<FloatRangeData> subUpgradeByStars;
    }

    public class ArtifactTuningConfigData
    {
        public List<KVP_StringIntRange> initialSubstatCounts;
        public List<KVP_StringStringList> allowedMainStatsPerSlot;
        public List<StatTypeData> statRanges;
    }

    public class GachaRewardEntryData
    {
        public string rewardType;
        public string rarity;
        public string itemId;
        public int weight;
        public int minItemStars;
        public int maxItemStars;
        public int minArtifactRarity;
        public int maxArtifactRarity;
    }

    public class GachaBannerConfigData
    {
        public string bannerId;
        public int costPerPull;
        public int softPityThreshold;
        public int hardPityThreshold;
        public int guaranteedAboveBaseEvery;
        public bool hasEpitomizedPath;
        public List<KVP_StringFloat> basicProbabilities;
        public List<GachaRewardEntryData> supremeChoices;
        public List<GachaRewardEntryData> totalPool;
    }

    public class MasterGameData
    {
        public List<PetSpeciesConfigData> petSpecies;
        public List<KVP_StringFloat> starMultipliers;
        public ArtifactTuningConfigData artifactTuning;
        public List<GachaBannerConfigData> banners;
    }
}
