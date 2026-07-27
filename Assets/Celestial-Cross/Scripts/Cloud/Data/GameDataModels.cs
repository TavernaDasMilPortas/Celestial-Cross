using System;
using System.Collections.Generic;

namespace CelestialCross.Cloud.Data
{
    [Serializable]
    public class FloatRangeData { public float min; public float max; }

    [Serializable]
    public class IntRangeData { public int min; public int max; }

    [Serializable]
    public class KVP_StringFloat { public string key; public float value; }

    [Serializable]
    public class KVP_StringIntRange { public string key; public IntRangeData value; }

    [Serializable]
    public class KVP_StringStringList { public string key; public List<string> value; }

    [Serializable]
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

    [Serializable]
    public class PetSpeciesConfigData
    {
        public string id;
        public string name;
        public PetStatRangesData statRanges;
    }

    [Serializable]
    public class StatTypeData
    {
        public string statType;
        public List<FloatRangeData> mainBaseByStars = new List<FloatRangeData>();
        public List<FloatRangeData> mainUpgradeByStars = new List<FloatRangeData>();
        public List<FloatRangeData> subInitialByStars = new List<FloatRangeData>();
        public List<FloatRangeData> subUpgradeByStars = new List<FloatRangeData>();
    }

    [Serializable]
    public class ArtifactTuningConfigData
    {
        public List<KVP_StringIntRange> initialSubstatCounts = new List<KVP_StringIntRange>();
        public List<KVP_StringStringList> allowedMainStatsPerSlot = new List<KVP_StringStringList>();
        public List<StatTypeData> statRanges = new List<StatTypeData>();
    }

    [Serializable]
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

    [Serializable]
    public class GachaBannerConfigData
    {
        public string bannerId;
        public int costPerPull;
        public int softPityThreshold;
        public int hardPityThreshold;
        public int guaranteedAboveBaseEvery;
        public bool hasEpitomizedPath;
        public List<KVP_StringFloat> basicProbabilities = new List<KVP_StringFloat>();
        public List<GachaRewardEntryData> supremeChoices = new List<GachaRewardEntryData>();
        public List<GachaRewardEntryData> totalPool = new List<GachaRewardEntryData>();
    }

    [Serializable]
    public class MasterGameData
    {
        public List<PetSpeciesConfigData> petSpecies = new List<PetSpeciesConfigData>();
        public List<KVP_StringFloat> starMultipliers = new List<KVP_StringFloat>();
        public ArtifactTuningConfigData artifactTuning = new ArtifactTuningConfigData();
        public List<GachaBannerConfigData> banners = new List<GachaBannerConfigData>();
    }
}
