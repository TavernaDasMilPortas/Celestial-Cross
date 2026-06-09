using UnityEngine;
using CelestialCross.Combat;
using System;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    /// <summary>
    /// Base class for all ability and passive conditions (Inline Version).
    /// </summary>
    [Serializable]
    public abstract class AbilityConditionData
    {
        [Tooltip("If true, the condition result will be flipped (true becomes false).")]
        public bool invert = false;

        public bool Evaluate(CombatContext context)
        {
            if (context == null) return false;
            
            bool result = OnEvaluate(context);
            bool finalResult = invert ? !result : result;
            
            CombatLogger.Log($"{GetType().Name} -> {(finalResult ? "PASSOU" : "FALHOU")}", LogCategory.Condition);
            return finalResult;
        }

        protected abstract bool OnEvaluate(CombatContext context);
    }
}