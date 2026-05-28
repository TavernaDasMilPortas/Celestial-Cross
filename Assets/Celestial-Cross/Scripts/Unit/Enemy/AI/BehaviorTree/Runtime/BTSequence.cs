namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTSequence : BTComposite
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || Data.ports == null) return BTResult.Failure;

            foreach (var portName in Data.ports)
            {
                if (ChildNodes.TryGetValue(portName, out var child) && child != null)
                {
                    var result = child.Evaluate(blackboard);
                    if (result != BTResult.Success)
                    {
                        return result; // Failure or Running
                    }
                }
                else return BTResult.Failure; // Missing child link
            }
            return BTResult.Success;
        }
    }
}
