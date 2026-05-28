namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTRepeater : BTDecorator
    {
        public int Count { get; set; } = 1;
        private int currentCount = 0;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Child == null) return BTResult.Failure;

            if (currentCount < Count)
            {
                var result = Child.Evaluate(blackboard);
                if (result != BTResult.Running)
                {
                    currentCount++;
                }
                return BTResult.Running; // Returns running until count is met
            }

            currentCount = 0; // Reset
            return BTResult.Success;
        }
    }
}
