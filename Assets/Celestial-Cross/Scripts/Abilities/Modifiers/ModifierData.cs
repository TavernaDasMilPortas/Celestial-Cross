using System;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Modifiers
{
    public enum DurationType { Momentary, UntilEndOfTurn, Turns, Charges, Infinite }

    [Serializable]
    public struct ModifierDurationSettings
    {
        public DurationType type;
        [Tooltip("Number of turns or charges. Ignored for Momentary or Infinite.")]
        public int durationValue;
    }

    [Serializable]
    public abstract class ModifierData
    {
        [Header("Trigger & Execution")]
        public CombatHook triggerHook;
        
        [Header("Conditions")]
        [Tooltip("Índices das condições na AbilityBlueprint.globalConditions que precisam ser satisfeitas para este modifier atuar.")]
        public List<int> activeConditionIndices = new List<int>();

        [Header("Duration")]
        public ModifierDurationSettings durationSettings;

        // Synchronous immediate execution, replacing old Coroutines or Passive Effects
        public abstract void ApplyModifier(CombatContext context);

        public bool EvaluateConditions(CombatContext context)
        {
            if (activeConditionIndices == null || activeConditionIndices.Count == 0) return true;
            if (context.conditionPool == null || context.conditionPool.Count == 0) return true;

            foreach (int index in activeConditionIndices)
            {
                if (index >= 0 && index < context.conditionPool.Count)
                {
                    var cond = context.conditionPool[index];
                    // At the moment, we require ALL listed conditions to be true (AND logic).
                    // If you want OR logic, we can add a requireAllConditions flag like in EffectData.
                    if (cond != null && !cond.Evaluate(context)) return false;
                }
            }
            return true;
        }
    }
}