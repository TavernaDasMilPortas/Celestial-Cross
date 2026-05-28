using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTRandomChance : BTDecorator
    {
        public float ChancePercent { get; set; } = 50f;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Child == null) return BTResult.Failure;

            if (Random.Range(0f, 100f) <= ChancePercent)
            {
                return Child.Evaluate(blackboard);
            }

            return BTResult.Failure;
        }
    }
}
