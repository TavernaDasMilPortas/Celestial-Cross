using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    [Serializable]
    public class DistanceCondition : AbilityConditionData
    {
        public enum DistanceType { Min, Max, Exact }
        
        [Tooltip("Type of distance check.")]
        public DistanceType checkType = DistanceType.Min;

        [Tooltip("The value to compare against.")]
        public int distanceValue = 5;

        protected override bool OnEvaluate(CombatContext context)
        {
            if (context.source == null || context.target == null) return false;

            int dist = GetChebyshevDistance(context.source.GridPosition, context.target.GridPosition);

            return checkType switch
            {
                DistanceType.Min => dist >= distanceValue,
                DistanceType.Max => dist <= distanceValue,
                DistanceType.Exact => dist == distanceValue,
                _ => false
            };
        }

        private int GetChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }
    }
}