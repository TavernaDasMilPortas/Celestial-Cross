using System;
using UnityEngine;
using Celestial_Cross.Scripts.Abilities.Conditions;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Conditions
{
    [Serializable]
    public class TurnOrderCondition : AbilityConditionData
    {
        public enum OrderType
        {
            FirstInRound,
            LastInRound,
            SpecificIndex
        }

        public OrderType type = OrderType.FirstInRound;
        public int specificIndex = 0;

        protected override bool OnEvaluate(CombatContext context)
        {
            if (TurnManager.Instance == null) return false;

            if (type == OrderType.FirstInRound)
            {
                return context.source == TurnManager.Instance.RoundStartUnit;
            }

            // Other types would require a list from the queue, which the Queue<T> doesn't expose naturally.
            // For now, FirstInRound solves the Leidell requirement.
            
            return false; 
        }
    }
}
