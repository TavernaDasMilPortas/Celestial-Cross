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
                
                var wMoveAction = blackboard.availableAbilities.FirstOrDefault(a => a.action != null && (a.action is Celestial_Cross.Scripts.Units.GraphActionWrapper gw && gw.Subtype == AbilitySubtype.Movement)) 
                               ?? blackboard.availableAbilities.FirstOrDefault(a => a.action != null && a.action.GetType().Name == "MoveAction");

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

            if (targetUnit != null)
            {
                int myDist = AIGridUtility.ChebyshevDistance(blackboard.myPosition, targetUnit.GridPosition);
                if (Data.intent == BTMoveIntent.Approach)
                {
                    int idealRange = 1;
                    var offensiveAbilities = blackboard.availableAbilities.Where(a => a.subtype == AbilitySubtype.Attack || a.subtype == AbilitySubtype.Debuff).ToList();
                    if (offensiveAbilities.Count > 0)
                        idealRange = offensiveAbilities.Max(a => a.range);

                    if (myDist > idealRange)
                        bestScore = -myDist;
                    else
                        bestScore = 1000 - ((idealRange - myDist) * 10);
                }
                else if (Data.intent == BTMoveIntent.Flee)
                {
                    bestScore = myDist;
                }
            }
            else
            {
                bestScore = 0; // Sem alvo, a melhor opção é ficar parado (score 0)
            }

            foreach (var tile in blackboard.reachableTiles)
            {
                int score = 0;
                
                if (targetUnit != null)
                {
                    int dist = AIGridUtility.ChebyshevDistance(tile, targetUnit.GridPosition);
                    if (Data.intent == BTMoveIntent.Approach)
                    {
                        int idealRange = 1;
                        var offensiveAbilities = blackboard.availableAbilities.Where(a => a.subtype == AbilitySubtype.Attack || a.subtype == AbilitySubtype.Debuff).ToList();
                        if (offensiveAbilities.Count > 0)
                            idealRange = offensiveAbilities.Max(a => a.range);

                        if (dist > idealRange)
                        {
                            score = -dist; // Longe do ideal: quanto mais perto, melhor
                        }
                        else
                        {
                            // Dentro do alcance de ataque! 
                            // Ranged preferem ficar no limite do range para não tomar dano melee.
                            score = 1000 - ((idealRange - dist) * 10);
                        }
                    }
                    else if (Data.intent == BTMoveIntent.Flee)
                        score = dist; // Farther is better
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTile = tile;
                }
            }

            Debug.Log($"[ActionMove] Caster: {blackboard.MyUnit.DisplayName} at {blackboard.myPosition}. Reachable tiles count: {blackboard.reachableTiles.Count}. Best tile selected: {bestTile} with score {bestScore}.");

            if (bestTile == blackboard.myPosition)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionMove ({Data.intent}): Já está na melhor posição.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            var moveAction = blackboard.availableAbilities.FirstOrDefault(a => a.action != null && (a.action is Celestial_Cross.Scripts.Units.GraphActionWrapper gw && gw.Subtype == AbilitySubtype.Movement)) 
                          ?? blackboard.availableAbilities.FirstOrDefault(a => a.action != null && a.action.GetType().Name == "MoveAction");

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
