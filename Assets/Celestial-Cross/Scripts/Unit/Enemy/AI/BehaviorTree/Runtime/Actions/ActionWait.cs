using System.Linq;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionWait : BTAction
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            blackboard.bestPlan = new AIBlackboard.PlannedAction {
                actionToExecute = null,
                moveTarget = null,
                targetUnit = null
            };
            return BTResult.Success;
        }
    }
}
