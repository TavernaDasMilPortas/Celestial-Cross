using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Targeting
{
    public enum AutoTargetType
    {
        ClosestEnemy,
        LowestHPEnemy,
        LowestHPAlly,
        HighestAttackEnemy,
        RandomEnemy,
        Self,
        MainTarget
    }

    [Serializable]
    public class AutoTargetingStrategy : TargetingStrategyData
    {
        public AutoTargetType targetType = AutoTargetType.ClosestEnemy;
        
        [Tooltip("The maximum number of targets this strategy should collect.")]
        [Min(1)] public int maxTargetCount = 1;

        public AutoTargetingStrategy()
        {
            RequiresManualSelection = false;
        }

        public override List<global::Unit> GetTargets(CombatContext context)
        {
            if (context.source == null) return new List<global::Unit>();

            global::Unit caster = context.source;
            // Get all units on the field
            List<global::Unit> allUnits = UnityEngine.Object.FindObjectsByType<global::Unit>(UnityEngine.FindObjectsSortMode.None).ToList();
            
            // Exclude dead units and the caster itself
            allUnits = allUnits.Where(u => u != null && u.Health != null && u.Health.CurrentHealth > 0 && u != caster).ToList();

            IEnumerable<global::Unit> sortedTargets = null;

            if (targetType == AutoTargetType.Self)
            {
                return new List<global::Unit> { context.source };
            }

            if (targetType == AutoTargetType.MainTarget)
            {
                if (context.target != null)
                    return new List<global::Unit> { context.target };
                else
                    return new List<global::Unit>();
            }

            switch (targetType)
            {
                case AutoTargetType.ClosestEnemy:
                    sortedTargets = allUnits
                        .Where(u => u.CompareTag("Enemy") != caster.CompareTag("Enemy"))
                        .OrderBy(u => Vector2Int.Distance(caster.GridPosition, u.GridPosition));
                    break;

                case AutoTargetType.LowestHPEnemy:
                    sortedTargets = allUnits
                        .Where(u => u.CompareTag("Enemy") != caster.CompareTag("Enemy"))
                        .OrderBy(u => u.Health.CurrentHealth);
                    break;

                case AutoTargetType.LowestHPAlly:
                    sortedTargets = allUnits
                        .Where(u => u.CompareTag("Enemy") == caster.CompareTag("Enemy") || u.CompareTag("Player") == caster.CompareTag("Player"))
                        .OrderBy(u => u.Health.CurrentHealth);
                    break;

                case AutoTargetType.HighestAttackEnemy:
                    sortedTargets = allUnits
                        .Where(u => u.CompareTag("Enemy") != caster.CompareTag("Enemy"))
                        .OrderByDescending(u => u.Stats.attack);
                    break;

                case AutoTargetType.RandomEnemy:
                    sortedTargets = allUnits
                        .Where(u => u.CompareTag("Enemy") != caster.CompareTag("Enemy"))
                        .OrderBy(u => UnityEngine.Random.value);
                    break;
            }

            if (sortedTargets != null && sortedTargets.Any())
            {
                return sortedTargets.Take(maxTargetCount).ToList();
            }

            return new List<global::Unit>();
        }
    }
}
