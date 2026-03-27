using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Sirenix.OdinInspector;

namespace Celestial_Cross.Scripts.Abilities
{
    public enum ValueType { Flat, Percentage }

    [Serializable]
    public abstract class EffectData
    {
        [Header("Conditional Logic")]
        [Tooltip("Optional conditions that must be met for this effect to execute.")]
        [SerializeReference]
        public List<AbilityConditionData> conditions = new();
        
        [Tooltip("If true, all conditions must be true. If false, at least one must be true.")]
        public bool requireAllConditions = true;

        [Header("Bonus Scaling")]
        [Tooltip("If true, the effect's bonus will scale based on the distance to the target.")]
        public bool scaleWithDistance = false;

        [Tooltip("The amount to multiply the bonus by for each unit of distance.")]
        [ShowIf("scaleWithDistance")]
        public float distanceScaleFactor = 0.1f;

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
                    if (condition != null && !condition.Evaluate(context))
                    {
                        return false; // Se um falhar, o resultado é falso
                    }
                }
                return true; // Todos passaram
            }
            else
            {
                foreach (var condition in conditions)
                {
                    if (condition != null && condition.Evaluate(context))
                    {
                        return true; // Se um passar, o resultado é verdadeiro
                    }
                }
                return false; // Nenhum passou
            }
        }
    }
}
