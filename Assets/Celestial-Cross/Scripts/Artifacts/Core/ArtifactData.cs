using System;
using System.Collections.Generic;
using UnityEngine;

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
    public class ArtifactInstanceData : ISerializationCallbackReceiver
    {
        public string idGUID;
        public string artifactSetId; // ID do ScriptableObject do Set
        public ArtifactType slot;
        public ArtifactRarity rarity;
        public ArtifactStars stars = ArtifactStars.One;
        public int currentLevel;

        public StatModifierData mainStat;
        public List<StatModifierData> subStats = new List<StatModifierData>();

        public ArtifactInstanceData()
        {
            idGUID = Guid.NewGuid().ToString();
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            int value = (int)stars;
            if (value < 1) stars = ArtifactStars.One;
            else if (value > 6) stars = ArtifactStars.Six;
        }

        public int GetStarsAsIntClamped()
        {
            int value = (int)stars;
            if (value < 1) return 1;
            if (value > 6) return 6;
            return value;
        }
    }
}
