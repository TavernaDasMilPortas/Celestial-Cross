namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTRoot : BTNode
    {
        public BTNode Child { get; set; }

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Child == null) return BTResult.Failure;
            return Child.Evaluate(blackboard);
        }
    }
}
