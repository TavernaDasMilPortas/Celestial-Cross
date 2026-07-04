using System;
using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    [Serializable]
    public class RangeCondition : AbilityConditionData
    {
        public enum RangeOrigin { Caster, Target }
        public enum UnitFilter { Allies, Enemies, Both }
        public enum Comparison { GreaterOrEqual, LessOrEqual, Exact }

        [Header("Origin")]
        [Tooltip("From where the range should be measured? Caster or the Target of the effect?")]
        public RangeOrigin origin = RangeOrigin.Caster;

        [Header("Targeting Settings")]
        [Tooltip("The range around the origin to check for units.")]
        public int range = 1;
        
        [Tooltip("Who should be counted in this range?")]
        public UnitFilter filter = UnitFilter.Both;

        [Header("Count Comparison")]
        [Tooltip("Required number of units found.")]
        public int targetCount = 2;
        
        [Tooltip("How to compare the found count with targetCount.")]
        public Comparison comparison = Comparison.GreaterOrEqual;

        protected override bool OnEvaluate(CombatContext context)
        {
            Unit originUnit = (origin == RangeOrigin.Caster) ? context.source : context.target;
            if (originUnit == null) 
            {
                Debug.LogWarning($"[RangeCondition] Origin unit ({origin}) está NULL no contexto.");
                return false;
            }

            // Get all units on board
            var allUnits = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
            
            // Filter by distance and relationship
            int foundCount = 0;
            foreach (var u in allUnits)
            {
                if (u == null || u == originUnit) continue;

                int dist = Mathf.Abs(originUnit.GridPosition.x - u.GridPosition.x) + Mathf.Abs(originUnit.GridPosition.y - u.GridPosition.y);
                if (dist > range) continue;

                // Relationship Filter
                // IMPORTANT: Allies of the originUnit or Allies of the CASTER?
                // For Sniper "Isolated Target", we want allies of the TARGET.
                bool isTargetAlly = u.GetType() == originUnit.GetType();
                
                bool matchesFilter = filter switch
                {
                    UnitFilter.Allies => isTargetAlly,
                    UnitFilter.Enemies => !isTargetAlly,
                    UnitFilter.Both => true,
                    _ => false
                };

                if (matchesFilter) 
                {
                    Debug.Log($"[RangeCondition] Encontrou unit {u.name} a distância {dist} do {originUnit.name} (Filtro: {filter})");
                    foundCount++;
                }
            }

            bool finalResult = comparison switch
            {
                Comparison.GreaterOrEqual => foundCount >= targetCount,
                Comparison.LessOrEqual => foundCount <= targetCount,
                Comparison.Exact => foundCount == targetCount,
                _ => false
            };

            Debug.Log($"[RangeCondition] Origin: {originUnit.name} | Range: {range} | Count: {foundCount} | Target: {targetCount} | Resultado: {finalResult}");
            return finalResult;
        }
    }
}
