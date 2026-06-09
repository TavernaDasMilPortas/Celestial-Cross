using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    [Serializable]
    public class AttributeCondition : AbilityConditionData
    {
        public enum TargetType { Caster, Target }
        public enum AttributeType { HP, Attack, Defense, Speed, EffectAccuracy, CriticalChance }
        public enum Comparison { GreaterThan, LessThan, Equal, GreaterOrEqual, LessOrEqual }
        public enum ValueMode { Flat, Percentage }

        [Header("Targeting")]
        public TargetType targetToCheck = TargetType.Target;
        public AttributeType attribute = AttributeType.HP;

        [Header("Evaluation")]
        public Comparison comparison = Comparison.LessOrEqual;
        public ValueMode mode = ValueMode.Flat;
        public float threshold = 50f;

        protected override bool OnEvaluate(CombatContext context)
        {
            Unit unit = targetToCheck == TargetType.Caster ? context.source : context.target;
            if (unit == null) return false;

            float currentValue = GetAttributeValue(unit);
            float targetValue = threshold;

            if (mode == ValueMode.Percentage && attribute == AttributeType.HP)
            {
                // Para HP, porcentagem é baseada no HP Máximo
                currentValue = (float)unit.Health.CurrentHealth / unit.Health.MaxHealth * 100f;
            }

            return comparison switch
            {
                Comparison.GreaterThan => currentValue > targetValue,
                Comparison.LessThan => currentValue < targetValue,
                Comparison.Equal => Mathf.Approximately(currentValue, targetValue),
                Comparison.GreaterOrEqual => currentValue >= targetValue,
                Comparison.LessOrEqual => currentValue <= targetValue,
                _ => false
            };
        }

        private float GetAttributeValue(Unit unit)
        {
            return attribute switch
            {
                AttributeType.HP => unit.Health.CurrentHealth,
                AttributeType.Attack => unit.Stats.attack,
                AttributeType.Defense => unit.Stats.defense,
                AttributeType.Speed => unit.Stats.speed,
                AttributeType.EffectAccuracy => unit.Stats.effectAccuracy,
                AttributeType.CriticalChance => unit.Stats.criticalChance,
                _ => 0f
            };
        }
    }
}