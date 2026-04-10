using UnityEngine;

namespace CelestialCross.Artifacts
{
    [global::System.Serializable]
    public struct StatModifier
    {
        public StatType statType;
        public float value;

        public StatModifier(StatType statType, float value)
        {
            this.statType = statType;
            this.value = value;
        }

        // Para facilitar a leitura nos menus (ex: exibir o + ou %)
        public override string ToString()
        {
            bool isPercent = statType.ToString().Contains("Percent") || 
                             statType == StatType.CriticalRate || 
                             statType == StatType.CriticalDamage ||
                             statType == StatType.EffectResistance ||
                             statType == StatType.EffectHitRate;
            
            return $"{statType}: +{value:F0}{(isPercent ? "%" : "")}";
        }
    }
}
