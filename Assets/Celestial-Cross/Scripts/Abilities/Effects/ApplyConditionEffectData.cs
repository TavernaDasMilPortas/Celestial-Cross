using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class ApplyConditionEffectData : EffectData
    {
        public AbilityBlueprint condition;

        public override void Execute(CombatContext context)
        {
            if (context.target != null && condition != null)
            {
                var passiveManager = context.target.GetComponent<PassiveManager>();
                if (passiveManager != null)
                {
                    Debug.Log($"ApplyConditionEffect: {condition.name} em {context.target.name}");
                    passiveManager.ApplyCondition(condition, context.source);
                }
            }
        }
    }
}

