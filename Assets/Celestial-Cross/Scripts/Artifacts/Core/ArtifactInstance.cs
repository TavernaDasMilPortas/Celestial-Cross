using System;
using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.Artifacts
{
    [CreateAssetMenu(fileName = "NewArtifactInstance", menuName = "Celestial Cross/Artifacts/Artifact Instance")]
    public class ArtifactInstance : ScriptableObject
    {
        [Header("Meta Data")]
        public string idGUID; // Universal ID para Save Files
        
        [Header("Base Properties")]
        public ArtifactType slot;
        public ArtifactSet artifactSet; // Família (Set) a que pertence
        public ArtifactRarity rarity;
        public ArtifactStars stars = ArtifactStars.One;
        [Range(1, 15)] public int currentLevel = 1;

        [Header("Stats")]
        public StatModifier mainStat;
        public List<StatModifier> subStats = new List<StatModifier>();

        public Sprite GetIcon()
        {
            if (artifactSet == null) return null;
            return artifactSet.GetIconForSlot(slot);
        }

        // Método auxiliar caso usemos pra inicializar num save futuramente.
        public void GenerateGUID()
        {
            if (string.IsNullOrEmpty(idGUID))
                idGUID = Guid.NewGuid().ToString();
        }

        public int GetStarsAsIntClamped()
        {
            int value = (int)stars;
            if (value < 1) return 1;
            if (value > 6) return 6;
            return value;
        }

        private void OnValidate()
        {
            // Unity can deserialize old int values into enums; clamp invalid legacy/default values.
            int value = (int)stars;
            if (value < 1) stars = ArtifactStars.One;
            else if (value > 6) stars = ArtifactStars.Six;
        }
    }
}
