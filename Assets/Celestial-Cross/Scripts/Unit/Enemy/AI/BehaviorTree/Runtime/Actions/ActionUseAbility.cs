using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionUseAbility : BTAction
    {
        public AIAbilityHint.AbilityCategory category = AIAbilityHint.AbilityCategory.Damage;
        public float minimumScoreThreshold = 5f;
        public bool ignoreCategoryFilter = false;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.availableAbilities == null) return BTResult.Failure;

            var abilities = blackboard.availableAbilities;
            if (!ignoreCategoryFilter)
            {
                abilities = abilities.Where(a => a.hint != null && a.hint.category == category).ToList();
            }

            if (abilities.Count == 0)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({(ignoreCategoryFilter ? "Best" : category.ToString())}): Nenhuma habilidade disponível.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            Unit forcedTarget = null;
            if (DataInputs.TryGetValue("Target", out var targetNode) && targetNode is Data.BTGetTargetNode getTarget)
            {
                getTarget.Evaluate(blackboard);
                forcedTarget = getTarget.TargetResult;
            }

            AIBlackboard.AbilityInfo bestAbility = null;
            Unit bestTarget = null;
            float highestScore = -9999f;

            foreach (var ability in abilities)
            {
                var hint = ability.hint;
                if (hint == null) continue;

                var candidates = new List<Unit>();
                if (forcedTarget != null)
                {
                    candidates.Add(forcedTarget);
                }
                else
                {
                    candidates.AddRange(hint.targetsFriendlies ? blackboard.allies : blackboard.enemies);
                }

                // Filtro de Range Real
                candidates.RemoveAll(u => AIGridUtility.ChebyshevDistance(blackboard.myPosition, u.GridPosition) > ability.range);

                foreach (var target in candidates)
                {
                    float score = CalculateUtilityScore(ability, target, blackboard);
                    
                    if (score > highestScore)
                    {
                        highestScore = score;
                        bestAbility = ability;
                        bestTarget = target;
                    }
                }
            }

            if (bestAbility == null || bestTarget == null || highestScore < minimumScoreThreshold)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({(ignoreCategoryFilter ? "Best" : category.ToString())}): Nenhuma habilidade/alvo viável superou o threshold de {minimumScoreThreshold}.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            blackboard.bestPlan = new AIBlackboard.PlannedAction {
                actionToExecute = bestAbility.action,
                targetUnit = bestTarget,
                moveTarget = null
            };

            string abilityName = bestAbility.action != null ? bestAbility.action.ActionName : "Desconhecida";
            CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({(ignoreCategoryFilter ? "Best" : category.ToString())}): Planejou usar '{abilityName}' em <b>{bestTarget.DisplayName}</b> (Score: {highestScore:F1})", CelestialCross.Combat.LogCategory.AI);
            return BTResult.Success;
        }

        private float CalculateUtilityScore(AIBlackboard.AbilityInfo ability, Unit primaryTarget, AIBlackboard blackboard)
        {
            var hint = ability.hint;
            float score = hint.basePriority;

            var validTargets = hint.targetsFriendlies ? blackboard.allies : blackboard.enemies;
            var friendlyUnits = hint.targetsFriendlies ? blackboard.enemies : blackboard.allies; 

            int maxTargets = ability.maxTargets > 0 ? ability.maxTargets : 1;
            int totalValidHits = 0;
            int totalFriendlyHits = 0;

            var simValidTargets = new List<Unit>(validTargets);
            var simFriendlyTargets = new List<Unit>(friendlyUnits);

            for (int i = 0; i < maxTargets; i++)
            {
                Unit currentCenter = null;
                if (i == 0)
                {
                    currentCenter = primaryTarget;
                }
                else
                {
                    currentCenter = GetBestSecondaryTarget(blackboard.myPosition, ability.range, simValidTargets);
                }

                if (currentCenter == null) break;

                if (ability.areaPattern != null)
                {
                    var (vHits, fHits, hitPositions) = AIGridUtility.EvaluateAoE(
                        blackboard.myPosition, currentCenter.GridPosition,
                        ability.areaPattern, simValidTargets, simFriendlyTargets);

                    totalValidHits += vHits;
                    totalFriendlyHits += fHits;

                    if (!ability.allowSameTargetMultipleTimes)
                    {
                        simValidTargets.RemoveAll(u => hitPositions.Contains(u.GridPosition));
                        simFriendlyTargets.RemoveAll(u => hitPositions.Contains(u.GridPosition));
                    }
                    else
                    {
                        simValidTargets.Remove(currentCenter);
                    }
                }
                else
                {
                    totalValidHits += 1;
                    if (!ability.allowSameTargetMultipleTimes) simValidTargets.Remove(currentCenter);
                }
            }

            if (totalValidHits > 1)
            {
                score += (totalValidHits - 1) * (hint.estimatedValue > 0 ? hint.estimatedValue : 15f);
            }
            if (totalFriendlyHits > 0)
            {
                score -= totalFriendlyHits * (hint.estimatedValue > 0 ? hint.estimatedValue * 2f : 30f);
            }

            float targetHpPercent = (float)primaryTarget.Health.CurrentHealth / primaryTarget.Health.MaxHealth;
            if (targetHpPercent <= 0.3f)
                score += hint.lowHPTargetBonus;

            if (hint.category == AIAbilityHint.AbilityCategory.Buff || hint.category == AIAbilityHint.AbilityCategory.Debuff)
            {
                string stateKey = $"{ability.action.ActionName}_{primaryTarget.gameObject.GetInstanceID()}";
                if (string.IsNullOrEmpty(blackboard.GetState(stateKey)))
                {
                    score += hint.freshApplicationBonus;
                }
            }

            return score;
        }

        private Unit GetBestSecondaryTarget(Vector2Int casterPos, int range, List<Unit> candidates)
        {
            return candidates
                .Where(u => AIGridUtility.ChebyshevDistance(casterPos, u.GridPosition) <= range)
                .OrderBy(u => AIGridUtility.ChebyshevDistance(casterPos, u.GridPosition))
                .FirstOrDefault();
        }
    }
}
