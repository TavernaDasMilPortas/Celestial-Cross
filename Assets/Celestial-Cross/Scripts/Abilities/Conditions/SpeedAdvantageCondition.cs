using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    [Serializable]
    public class SpeedAdvantageCondition : AbilityConditionData
    {
        [Tooltip("The required speed difference (Caster Speed - Target Speed).")]
        public int requiredDifference = 10;

        [Tooltip("If true, checks if Caster.Speed >= Target.Speed + X. If false, checks for any difference.")]
        public bool greaterOrEqual = true;

        protected override bool OnEvaluate(CombatContext context)
        {
            if (context.source == null || context.target == null) return false;

            int casterSpeed = context.source.Stats.speed;
            int targetSpeed = context.target.Stats.speed;

            if (greaterOrEqual)
                return casterSpeed >= targetSpeed + requiredDifference;
            
            return Mathf.Abs(casterSpeed - targetSpeed) >= requiredDifference;
        }
    }
}