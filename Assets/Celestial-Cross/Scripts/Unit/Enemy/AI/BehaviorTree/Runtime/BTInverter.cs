namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTInverter : BTDecorator
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Child == null) return BTResult.Failure;

            var result = Child.Evaluate(blackboard);
            if (result == BTResult.Success) return BTResult.Failure;
            if (result == BTResult.Failure) return BTResult.Success;
            return BTResult.Running;
        }
    }
}
