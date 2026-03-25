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

        [Tooltip("If true, healing can exceed the target's Max HP.")]
        public bool allowOverheal = false;

        public override void Execute(CombatContext context)
        {
            if (context.target != null && context.target.Health != null)
            {
                float finalAmount = context.amount;

                if (canCritHeal && context.source != null)
                {
                    float luck = UnityEngine.Random.Range(0, 100);
                    if (luck < context.source.Stats.effectAccuracy)
                    {
                        CombatLogger.Log($"Cura CRÍTICA! em {context.target.name}", LogCategory.Healing);
                        finalAmount *= 2;
                    }
                }

                int finalHeal = Mathf.FloorToInt(finalAmount);
                CombatLogger.Log($"Cura em {context.target.name}: {finalHeal} HP. (Overheal: {allowOverheal})", LogCategory.Healing);
                context.target.Health.Heal(finalHeal, allowOverheal);
            }
        }

        public int GetBaseAmount(CombatContext context)
        {
            float baseAmount = amount;

            if (valueType == ValueType.Percentage)
            {
                float baseVal = baseAttribute switch {
                    AttributeCondition.AttributeType.HP => context.target?.Health?.MaxHealth ?? 0,
                    AttributeCondition.AttributeType.Attack => context.source?.Stats.attack ?? 0,
                    _ => context.source?.Stats.attack ?? 0
                };
                baseAmount = baseVal * (amount / 100f);
            }

            return Mathf.FloorToInt(baseAmount);
        }
    }
}
