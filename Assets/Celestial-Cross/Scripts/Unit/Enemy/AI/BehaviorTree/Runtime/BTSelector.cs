namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTSelector : BTComposite
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || Data.ports == null) return BTResult.Failure;
            
            foreach (var portName in Data.ports)
            {
                if (ChildNodes.TryGetValue(portName, out var child) && child != null)
                {
                    var result = child.Evaluate(blackboard);
                    if (result != BTResult.Failure)
                    {
                        return result; // Success or Running
                    }
                }
            }
            return BTResult.Failure;
        }
    }
}
