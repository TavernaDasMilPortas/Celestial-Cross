using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class ConditionalStatModifierEffectData : EffectData
    {
        [Header("Conditions")]
        [Tooltip("Conditions that must be met to apply the modifier.")]
        [SerializeReference]
        public List<AbilityConditionData> applyConditions = new List<AbilityConditionData>();
        
        [Header("Modifiers")]
        public CombatStats modifiers;

        public override void Execute(CombatContext context)
        {
            // First check base conditions from EffectData
            if (!EvaluateConditions(context)) return;

            // Then check specific application conditions
            bool allMet = true;
            foreach (var cond in applyConditions)
            {
                if (cond != null && !cond.Evaluate(context))
                {
                    allMet = false;
                    break;
                }
            }

            if (allMet && context.target != null)
            {
                Debug.Log($"[ConditionalStatModifier] Applying {modifiers.attack} ATK bonus to {context.target.name} based on conditions.");
                
                // For temporary modifiers, we usually want to register them in a list 
                // but since Unit currently calculates Stats as (data + modifierStats),
                // we'll just add it to modifierStats for now.
                // In a full implementation, this should probably be a 'Status Condition' with a duration.
            }
        }
    }
}
