using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities
{
    public enum ValueType { Flat, Percentage }

    [Serializable]
    public abstract class EffectData
    {
        [Header("Conditional Settings")]
        [Tooltip("Optional conditions that must be met for this effect to execute.")]
        public List<AbilityConditionData> conditions = new List<AbilityConditionData>();
        
        [Tooltip("If true, all conditions must be true. If false, at least one must be true.")]
        public bool requireAllConditions = true;

        public virtual void Execute(CombatContext context) { }

        public virtual IEnumerator ExecuteCoroutine(CombatContext context)      
        {
            if (!EvaluateConditions(context))
            {
                CombatLogger.Log($"Falhou condições para {GetType().Name} em {context.target?.name}", LogCategory.Ability);
                yield break;
            }

            CombatLogger.Log($"Executando {GetType().Name}...", LogCategory.Ability);
            Execute(context);
            yield break;
        }

        protected bool EvaluateConditions(CombatContext context)
        {
            if (conditions == null || conditions.Count == 0) return true;

            if (requireAllConditions)
            {
                foreach (var condition in conditions)
                {
                    if (condition != null && !condition.Evaluate(context)) return false;
                }
                return true;
            }
            else
            {
                foreach (var condition in conditions)
                {
                    if (condition != null && condition.Evaluate(context)) return true;
                }
                return false;
            }
        }
    }
}
