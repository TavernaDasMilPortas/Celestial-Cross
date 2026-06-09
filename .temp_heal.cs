using System;
using UnityEngine;
using CelestialCross.Combat;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class HealEffectData : EffectData
    {
        public ValueType valueType = ValueType.Flat;
        public int amount = 10;
        
        [Tooltip("If Percentage, base calculations on this attribute (usually MaxHP of target).")]
        public AttributeCondition.AttributeType baseAttribute = AttributeCondition.AttributeType.HP;

        [Tooltip("If true, uses Effect Accuracy to double the heal.")]
        public bool canCritHeal = true;

        public override void Execute(CombatContext context)
        {
            if (context.target != null && context.target.Health != null)
            {
                int finalHeal = CalculateHeal(context);
                context.target.Health.Heal(finalHeal);
            }
        }

        private int CalculateHeal(CombatContext context)
        {
            float baseAmount = amount;

            if (valueType == ValueType.Percentage)
            {
                float baseVal = baseAttribute switch {
                    AttributeCondition.AttributeType.HP => context.target.Health.MaxHealth,
                    AttributeCondition.AttributeType.Attack => context.source.Stats.attack,
                    _ => context.source.Stats.attack
                };
                baseAmount = baseVal * (amount / 100f);
            }

            if (canCritHeal && context.source != null)
            {
                float luck = UnityEngine.Random.Range(0, 100);
                if (luck < context.source.Stats.effectAccuracy)
                {
                    Debug.Log($"[HealEffectData] Cura CRÍTICA! (Chance: {context.source.Stats.effectAccuracy}%)");
                    baseAmount *= 2;
                }
            }

            return Mathf.FloorToInt(baseAmount);
        }
    }
}
