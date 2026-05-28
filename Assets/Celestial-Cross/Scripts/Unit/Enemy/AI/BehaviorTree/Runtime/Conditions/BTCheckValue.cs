namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class BTCheckValue : BTNode
    {
        public BTCheckValueData Data { get; set; }

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null) return BTResult.Failure;

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

            bool isMatch = false;
            switch (Data.operatorType)
            {
                case BTComparisonOperator.Equal: isMatch = UnityEngine.Mathf.Approximately(value, Data.threshold); break;
                case BTComparisonOperator.LessThan: isMatch = value < Data.threshold; break;
                case BTComparisonOperator.LessOrEqual: isMatch = value <= Data.threshold; break;
                case BTComparisonOperator.Greater: isMatch = value > Data.threshold; break;
                case BTComparisonOperator.GreaterOrEqual: isMatch = value >= Data.threshold; break;
                case BTComparisonOperator.ModuloZero: isMatch = Data.threshold != 0 && (value % Data.threshold) == 0; break;
            }

            return isMatch ? BTResult.Success : BTResult.Failure;
        }
    }
}
