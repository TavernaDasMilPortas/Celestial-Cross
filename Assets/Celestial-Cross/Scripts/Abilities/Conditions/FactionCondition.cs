using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    public enum FactionTarget
    {
        Ally,
        Enemy
    }

    [Serializable]
    public class FactionCondition : AbilityConditionData
    {
        public FactionTarget intendedFaction;

        protected override bool OnEvaluate(CombatContext context)
        {
            if (context.source == null || context.target == null) return false;

            bool isAlly = (context.source.CompareTag("Player") && context.target.CompareTag("Player")) || 
                          (context.source.CompareTag("Enemy") && context.target.CompareTag("Enemy"));

            if (intendedFaction == FactionTarget.Ally) return isAlly;
            else return !isAlly;
        }
    }
}