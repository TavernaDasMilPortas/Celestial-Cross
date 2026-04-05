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
        [Range(1, 6)] public int stars = 1;
        [Range(1, 15)] public int currentLevel = 1;

        [Header("Stats")]
        public StatModifier mainStat;
        public List<StatModifier> subStats = new List<StatModifier>();

        // Método auxiliar caso usemos pra inicializar num save futuramente.
        public void GenerateGUID()
        {
            if (string.IsNullOrEmpty(idGUID))
                idGUID = Guid.NewGuid().ToString();
        }
    }
}
