namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionTargetInRange : BTCondition
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (DataInputs.TryGetValue("Target", out var targetNode) && targetNode is Data.BTGetTargetNode getTarget)
            {
                getTarget.Evaluate(blackboard);
                var target = getTarget.TargetResult;
                if (target == null)
                {
                    CelestialCross.Combat.CombatLogger.Log($"   Condição TargetInRange: Sem alvo definido.", CelestialCross.Combat.LogCategory.AI);
                    return BTResult.Failure;
                }

                int dist = AIGridUtility.ChebyshevDistance(blackboard.myPosition, target.GridPosition);
                bool inRange = dist <= blackboard.myBaseRange;
                CelestialCross.Combat.CombatLogger.Log($"   Condição TargetInRange: Alvo {target.DisplayName} à distância {dist} (Alcance: {blackboard.myBaseRange}) -> {(inRange ? "Dentro" : "Fora")}", CelestialCross.Combat.LogCategory.AI);
                return inRange ? BTResult.Success : BTResult.Failure;
            }
            return BTResult.Failure;
        }
    }
}
