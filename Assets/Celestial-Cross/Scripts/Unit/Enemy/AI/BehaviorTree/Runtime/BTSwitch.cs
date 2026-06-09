using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTSwitch : BTNode
    {
        public BTSwitchData Data { get; set; }
        public Dictionary<string, BTNode> Cases { get; set; } = new Dictionary<string, BTNode>();

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || string.IsNullOrEmpty(Data.blackboardKey))
                return BTResult.Failure;

            // In a real scenario, you'd fetch the blackboard value safely.
            // For now, let's assume the blackboard has a dictionary of strings or state.
            // We will add a simple state tracking to the blackboard.
            
            string currentState = blackboard.GetState(Data.blackboardKey);

            if (Cases.TryGetValue(currentState, out var node))
            {
                return node.Evaluate(blackboard);
            }

            return BTResult.Failure;
        }
    }
}
