using UnityEngine;
using CelestialCross.Artifacts;

namespace CelestialCross.Giulia_UI
{
    public static class UIStatFormatter
    {
        public static string FormatStat(StatModifierData stat)
        {
            if (stat == null) return "N/A";
            string name = stat.statType.ToString();
            bool isPercent = name.EndsWith("Percent") || name.EndsWith("Rate") || name.EndsWith("Accuracy") || name.EndsWith("Chance");
            
            name = name.Replace("Percent", "").Replace("Flat", "");
            
            if (name == "Health") name = "Vida";
            else if (name == "Attack") name = "Ataque";
            else if (name == "Defense") name = "Defesa";
            else if (name == "Speed") name = "Veloc";
            else if (name == "CriticalRate" || name == "CriticalChance") name = "Tx Crítica";
            else if (name == "CriticalDamage") name = "Dano Crítico";
            else if (name == "EffectHitRate" || name == "EffectAccuracy") name = "Acerto de Efeito";
            else if (name == "EffectResistance") name = "Resistência";
            
            return $"{name} +{stat.value:F0}{(isPercent ? "%" : "")}";
        }
    }
}