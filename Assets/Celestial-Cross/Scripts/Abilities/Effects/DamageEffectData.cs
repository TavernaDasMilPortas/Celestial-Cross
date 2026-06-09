using System;
using UnityEngine;
using CelestialCross.Combat;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Combat.Execution;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class DamageEffectData : EffectData
    {
        [Tooltip("Multiplicador percentual (1.0 = 100%) do atributo base.")]
        public float multiplier = 1.0f;

        [Tooltip("If Percentage, base calculations on this attribute (HP, or Attack).")]
        public AttributeCondition.AttributeType baseAttribute = AttributeCondition.AttributeType.Attack;

        public override void Execute(CombatContext context)
        {
            if (!EvaluateConditions(context)) return;

            // DamageProcessor already handles hooks, passives, variables, crits and defense!
            DamageProcessor.ProcessAndApplyDamage(context, applyDefense: true);
        }

        public int GetBaseAmount(CombatContext context)
        {
            float baseVal = 0;
            switch(baseAttribute)
            {
                case AttributeCondition.AttributeType.HP:
                    baseVal = context.target?.Health?.CurrentHealth ?? 0; break;
                case AttributeCondition.AttributeType.Attack:
                    baseVal = context.source?.Stats.attack ?? 0; break;
                default: baseVal = context.source?.Stats.attack ?? 0; break;
            }

            return Mathf.FloorToInt(baseVal * multiplier);
        }
    }
}
