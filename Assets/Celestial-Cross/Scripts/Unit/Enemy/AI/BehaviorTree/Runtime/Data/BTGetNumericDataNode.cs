using System.Linq;
using Celestial_Cross.Scripts.Units;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Data
{
    public class BTGetNumericDataNode : BTNode
    {
        public BTGetNumericData Data { get; set; }
        public float NumericResult { get; private set; }

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null) return BTResult.Failure;

            switch (Data.dataType)
            {
                case BTNumericDataType.SelfHPPercent:
                    NumericResult = blackboard.myHpPercent;
                    break;
                case BTNumericDataType.TargetHPPercent:
                    Unit target = null;
                    if (DataInputs.TryGetValue("Target", out var targetNode) && targetNode is BTGetTargetNode getTarget)
                    {
                        getTarget.Evaluate(blackboard);
                        target = getTarget.TargetResult;
                    }
                    else
                    {
                        target = blackboard.enemies?.OrderBy(u => AIGridUtility.ChebyshevDistance(blackboard.myPosition, u.GridPosition)).FirstOrDefault();
                    }

                    if (target != null && target.Health != null)
                        NumericResult = ((float)target.Health.CurrentHealth / target.Health.MaxHealth) * 100f;
                    else
                        NumericResult = 100f;
                    break;
                case BTNumericDataType.LowestAllyHPPercent:
                    if (blackboard.allies != null && blackboard.allies.Count > 0)
                    {
                        var lowestAlly = blackboard.allies.OrderBy(u => (float)u.Health.CurrentHealth / u.Health.MaxHealth).First();
                        NumericResult = ((float)lowestAlly.Health.CurrentHealth / lowestAlly.Health.MaxHealth) * 100f;
                    }
                    else NumericResult = 100f;
                    break;
                case BTNumericDataType.DistanceToTarget:
                    Unit distTarget = null;
                    if (DataInputs.TryGetValue("Target", out var distTargetNode) && distTargetNode is BTGetTargetNode getDistTarget)
                    {
                        getDistTarget.Evaluate(blackboard);
                        distTarget = getDistTarget.TargetResult;
                    }
                    else
                    {
                        distTarget = blackboard.enemies?.OrderBy(u => AIGridUtility.ChebyshevDistance(blackboard.myPosition, u.GridPosition)).FirstOrDefault();
                    }

                    if (distTarget != null)
                        NumericResult = AIGridUtility.ChebyshevDistance(blackboard.myPosition, distTarget.GridPosition);
                    else
                        NumericResult = 999f;
                    break;
                case BTNumericDataType.TurnNumber:
                    NumericResult = blackboard.currentTurnNumber;
                    break;
                case BTNumericDataType.AliveAllyCount:
                    NumericResult = blackboard.aliveAllyCount;
                    break;
            }

            return BTResult.Success;
        }
    }
}
