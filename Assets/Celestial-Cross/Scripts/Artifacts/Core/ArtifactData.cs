using System;
using System.Collections.Generic;

namespace CelestialCross.Artifacts
{
    [Serializable]
    public class StatModifierData
    {
        public StatType statType;
        public float value;

        public StatModifierData() { }

        public StatModifierData(StatType statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }
    }

    [Serializable]
    public class ArtifactInstanceData
    {
        public string idGUID;
        public string artifactSetId; // ID do ScriptableObject do Set
        public ArtifactType slot;
        public ArtifactRarity rarity;
        public int stars;
        public int currentLevel;

        public StatModifierData mainStat;
        public List<StatModifierData> subStats = new List<StatModifierData>();

        public ArtifactInstanceData()
        {
            idGUID = Guid.NewGuid().ToString();
        }
    }
}
