using System.Collections.Generic;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTValueSwitch : BTNode
    {
        public BTValueSwitchData Data { get; set; }
        public Dictionary<string, BTNode> ChildNodes { get; set; } = new Dictionary<string, BTNode>();

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || Data.cases == null || Data.cases.Count == 0) return BTResult.Failure;

            float value = 0f;
            if (DataInputs.TryGetValue("Value", out var valNode) && valNode is Data.BTGetNumericDataNode numericNode)
            {
                numericNode.Evaluate(blackboard);
                value = numericNode.NumericResult;
            }
            else
            {
                return BTResult.Failure; // No data provider
            }

            foreach (var c in Data.cases)
            {
                bool isMatch = false;
                switch (c.operatorType)
                {
                    case BTComparisonOperator.Equal: isMatch = UnityEngine.Mathf.Approximately(value, c.threshold); break;
                    case BTComparisonOperator.LessThan: isMatch = value < c.threshold; break;
                    case BTComparisonOperator.LessOrEqual: isMatch = value <= c.threshold; break;
                    case BTComparisonOperator.Greater: isMatch = value > c.threshold; break;
                    case BTComparisonOperator.GreaterOrEqual: isMatch = value >= c.threshold; break;
                    case BTComparisonOperator.ModuloZero: isMatch = c.threshold != 0 && (value % c.threshold) == 0; break;
                }

                if (isMatch)
                {
                    if (ChildNodes.TryGetValue(c.portName, out var childNode))
                    {
                        return childNode.Evaluate(blackboard);
                    }
                    return BTResult.Success; // Matched case but no child logic, return success
                }
            }

            return BTResult.Failure;
        }
    }
}
