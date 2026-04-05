using UnityEngine;
using System.Collections.Generic;

namespace CelestialCross.Artifacts
{
    [CreateAssetMenu(fileName = "NewArtifactSet", menuName = "Celestial Cross/Artifacts/Artifact Set")]
    public class ArtifactSet : ScriptableObject
    {
        public string id;
        public string setName;
        [TextArea] public string description;

        [System.Serializable]
        public struct SetBonus
        {
            public int piecesRequired; // Ex: 2, 4, ou 6 peças.
            public List<StatModifier> statBonuses;
            public string passiveAbilityName; // Podemos conectar as skills do character depois.
        }

        public List<SetBonus> setBonuses = new List<SetBonus>();
    }
}
