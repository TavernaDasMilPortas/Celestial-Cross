using System.Collections.Generic;
using System.Linq;
using Celestial_Cross.Scripts.Units;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Data
{
    public class BTGetTargetNode : BTNode
    {
        public BTGetTargetData Data { get; set; }
        
        public Unit TargetResult { get; private set; }

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null) return BTResult.Failure;
            
            List<Unit> candidates = new List<Unit>();
            if (Data.faction == BTTargetFaction.Enemy)
                candidates = blackboard.enemies;
            else if (Data.faction == BTTargetFaction.Ally)
                candidates = blackboard.allies;

            if (Data.faction == BTTargetFaction.Self)
            {
                TargetResult = null;
                return BTResult.Success;
            }

            if (candidates == null || candidates.Count == 0)
            {
                TargetResult = null;
                return BTResult.Failure;
            }

            // Filter by tag if provided
            if (!string.IsNullOrEmpty(Data.requiredTag))
            {
                candidates = candidates.Where(u => u.gameObject.CompareTag(Data.requiredTag)).ToList();
            }

            if (candidates.Count == 0)
            {
                TargetResult = null;
                return BTResult.Failure;
            }

            switch (Data.strategy)
            {
                case BTTargetStrategy.Closest:
                    TargetResult = candidates.OrderBy(u => AIGridUtility.ChebyshevDistance(blackboard.myPosition, u.GridPosition)).FirstOrDefault();
                    break;
                case BTTargetStrategy.Farthest:
                    TargetResult = candidates.OrderByDescending(u => AIGridUtility.ChebyshevDistance(blackboard.myPosition, u.GridPosition)).FirstOrDefault();
                    break;
                case BTTargetStrategy.LowestHealth:
                    TargetResult = candidates.OrderBy(u => u.Health.CurrentHealth).FirstOrDefault();
                    break;
                case BTTargetStrategy.HighestHealth:
                    TargetResult = candidates.OrderByDescending(u => u.Health.CurrentHealth).FirstOrDefault();
                    break;
                case BTTargetStrategy.Random:
                    TargetResult = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                    break;
            }

            return TargetResult != null ? BTResult.Success : BTResult.Failure;
        }
    }
}
