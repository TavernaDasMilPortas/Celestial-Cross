using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Data; 

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionMove : BTAction
    {
        public ActionMoveData Data { get; set; } = new ActionMoveData();

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.reachableTiles == null || blackboard.reachableTiles.Count == 0)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionMove ({Data.intent}): Nenhum tile alcançável.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            if (Data.intent == BTMoveIntent.Wander)
            {
                var tilesList = blackboard.reachableTiles.ToList();
                var randomTile = tilesList[UnityEngine.Random.Range(0, tilesList.Count)];
                
                var wMoveAction = blackboard.availableAbilities.FirstOrDefault(a => a.action != null && a.action.GetType().Name == "MoveAction");
                if (wMoveAction == null || wMoveAction.action == null) return BTResult.Failure;

                blackboard.bestPlan = new AIBlackboard.PlannedAction {
                    actionToExecute = wMoveAction.action,
                    targetUnit = null,
                    moveTarget = randomTile
                };
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionMove (Wander): Escolheu andar aleatoriamente para {randomTile}", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Success;
            }

            Unit targetUnit = null;
            if (DataInputs.TryGetValue("Target", out var targetNode) && targetNode is BTGetTargetNode getTarget)
            {
                getTarget.Evaluate(blackboard);
                targetUnit = getTarget.TargetResult;
            }

            if (targetUnit == null)
            {
                if (Data.intent == BTMoveIntent.Approach) targetUnit = blackboard.closestEnemy;
                else if (Data.intent == BTMoveIntent.Flee) targetUnit = blackboard.closestEnemy;
            }

            Vector2Int bestTile = blackboard.myPosition;
            int bestScore = -9999;

            foreach (var tile in blackboard.reachableTiles)
            {
                int score = 0;
                
                if (targetUnit != null)
                {
                    int dist = AIGridUtility.ChebyshevDistance(tile, targetUnit.GridPosition);
                    if (Data.intent == BTMoveIntent.Approach)
                        score = -dist; // Closer is better
                    else if (Data.intent == BTMoveIntent.Flee)
                        score = dist; // Farther is better
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }

            if (bestTile == blackboard.myPosition)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionMove ({Data.intent}): Já está na melhor posição.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            var moveAction = blackboard.availableAbilities.FirstOrDefault(a => a.action != null && a.action.GetType().Name == "MoveAction");
            if (moveAction == null || moveAction.action == null) return BTResult.Failure;

            blackboard.bestPlan = new AIBlackboard.PlannedAction {
                actionToExecute = moveAction.action,
                targetUnit = null,
                moveTarget = bestTile
            };

            CelestialCross.Combat.CombatLogger.Log($"   Ação ActionMove ({Data.intent}): Planejou mover-se para {bestTile} em relação a {targetUnit?.DisplayName}", CelestialCross.Combat.LogCategory.AI);
            return BTResult.Success;
        }
    }
}
