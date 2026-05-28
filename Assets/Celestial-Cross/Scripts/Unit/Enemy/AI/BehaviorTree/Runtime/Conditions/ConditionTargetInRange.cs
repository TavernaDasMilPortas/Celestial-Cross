namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionTargetInRange : BTCondition
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            // Simple check: is closest enemy within my base range?
            if (blackboard.closestEnemy == null) return BTResult.Failure;
            
            int dist = AIGridUtility.ChebyshevDistance(blackboard.myPosition, blackboard.closestEnemy.GridPosition);
            if (dist <= blackboard.myBaseRange)
            {
                return BTResult.Success;
            }
            return BTResult.Failure;
        }
    }
}
