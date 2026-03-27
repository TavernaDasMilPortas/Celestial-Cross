using System;
using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Modifiers;

namespace CelestialCross.Abilities
{
    [Serializable]
    public class PassiveEffect_ScalingDistanceBonus : ModifierData
    {
        [Header("Scaling")]
        [Tooltip("Bonus applied per unit of distance between source and target.")]
        public CombatStats bonusPerUnit;

        public override void ApplyModifier(CombatContext context)
        {
            if (context.source == null || context.target == null)
            {
                Debug.LogWarning($"<color=#FFA500>[ScalingDistanceBonus]</color> Origem ou Alvo nulos! Abortando execução.");
                return;
            }

            int distance = Mathf.Abs(Mathf.RoundToInt(context.source.GridPosition.x) - Mathf.RoundToInt(context.target.GridPosition.x)) +
                           Mathf.Abs(Mathf.RoundToInt(context.source.GridPosition.y) - Mathf.RoundToInt(context.target.GridPosition.y));

            if (distance <= 0) return;

            if (bonusPerUnit.criticalChance > 0)
            {
                float totalBonusCrit = distance * bonusPerUnit.criticalChance;  
                if (!context.Variables.ContainsKey("bonus_crit_chance")) context.Variables["bonus_crit_chance"] = 0f;
                context.Variables["bonus_crit_chance"] += totalBonusCrit;       
            }

            if (bonusPerUnit.attack > 0)
            {
                int totalAtk = Mathf.RoundToInt(distance * bonusPerUnit.attack);
                context.amount += totalAtk;
            }
        }
    }
}
