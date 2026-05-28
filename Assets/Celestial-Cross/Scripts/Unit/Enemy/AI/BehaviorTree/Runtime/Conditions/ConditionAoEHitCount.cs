namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionAoEHitCount : BTCondition
    {
        public int minimumHitCount = 2;
        public int aoeRadius = 1;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.enemies == null) return BTResult.Failure;
            
            foreach (var target in blackboard.enemies)
            {
                if (target == null || target.Health.CurrentHealth <= 0) continue;

                int hits = 0;
                foreach (var unit in blackboard.enemies)
                {
                    if (unit == null || unit.Health.CurrentHealth <= 0) continue;
                    if (AIGridUtility.ChebyshevDistance(target.GridPosition, unit.GridPosition) <= aoeRadius)
                    {
                        hits++;
                    }
                }

                if (hits >= minimumHitCount) return BTResult.Success;
            }

            return BTResult.Failure;
        }
    }
}
