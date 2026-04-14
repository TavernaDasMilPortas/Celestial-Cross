using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;

namespace CelestialCross.Artifacts
{
    [CreateAssetMenu(fileName = "NewArtifactSet", menuName = "Celestial Cross/Artifacts/Artifact Set")]
    public class ArtifactSet : ScriptableObject
    {
        public string id;
        public string setName;
        [TextArea] public string description;

        [global::System.Serializable]
        public struct SlotIconMapping
        {
            public ArtifactType slot;
            public Sprite icon;
        }
        
        [Header("Icons per Slot")]
        public List<SlotIconMapping> slotIcons = new List<SlotIconMapping>();
        
        public Sprite GetIconForSlot(ArtifactType targetSlot)
        {
            if (slotIcons == null) return null;
            foreach (var mapping in slotIcons)
            {
                if (mapping.slot == targetSlot)
                    return mapping.icon;
            }
            return null;
        }

        [global::System.Serializable]
        public struct SetBonus
        {
            public int piecesRequired; // Ex: 2, 4, ou 6 peÃ§as.
            public List<StatModifier> statBonuses;
            public AbilityBlueprint passiveAbility; // Conecta diretamente a uma habilidade passiva.
        }

        public List<SetBonus> setBonuses = new List<SetBonus>();
    }
}
