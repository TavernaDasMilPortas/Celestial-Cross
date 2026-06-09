using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class StatModifierEffectData : EffectData
    {
        public CombatStats modifiers;

        public override void Execute(CombatContext context)
        {
            if (!EvaluateConditions(context)) return;

            // Optional distance scaling using EffectData base
            float multiplier = 1f;

            if (scaleWithDistance && context.source != null && context.target != null)
            {
                int distance = Mathf.Abs(Mathf.RoundToInt(context.source.GridPosition.x) - Mathf.RoundToInt(context.target.GridPosition.x)) +
                               Mathf.Abs(Mathf.RoundToInt(context.source.GridPosition.y) - Mathf.RoundToInt(context.target.GridPosition.y));
                multiplier = distance * distanceScaleFactor;

                // Evitar multiplier dar zero se distanceScaleFactor for 1 por um erro na unity
                if (distanceScaleFactor >= 1f) multiplier = distance; 
            }

            if (modifiers.attack > 0)
            {
                int totalBonusAtk = Mathf.RoundToInt(modifiers.attack * (scaleWithDistance ? multiplier : 1f));
                context.amount += totalBonusAtk;
                CombatLogger.Log($"+{totalBonusAtk} Ataque/Dano Base injetado via Passiva (Scale: {multiplier})", LogCategory.Passive);
            }

            if (modifiers.criticalChance > 0)
            {
                float totalBonusCrit = modifiers.criticalChance * (scaleWithDistance ? multiplier : 1f);
                if (!context.Variables.ContainsKey("bonus_crit_chance")) context.Variables["bonus_crit_chance"] = 0f;
                context.Variables["bonus_crit_chance"] += totalBonusCrit;
                CombatLogger.Log($"+{totalBonusCrit}% Chance Crítico injetado via Passiva (Scale: {multiplier})", LogCategory.Passive);
            }

            if (modifiers.defense > 0)
            {
                float totalBonusDef = modifiers.defense * (scaleWithDistance ? multiplier : 1f);
                if (!context.Variables.ContainsKey("bonus_defense")) context.Variables["bonus_defense"] = 0f;
                context.Variables["bonus_defense"] += totalBonusDef;
            }
        }
    }
}
