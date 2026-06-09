using System.Linq;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionUseAbility : BTAction
    {
        public AIAbilityHint.AbilityCategory category = AIAbilityHint.AbilityCategory.Damage;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.availableAbilities == null) return BTResult.Failure;

            var abilities = blackboard.availableAbilities.Where(a => a.hint != null && a.hint.category == category).ToList();
            if (abilities.Count == 0)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({category}): Nenhuma habilidade disponível nesta categoria.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            var bestAbility = abilities.OrderByDescending(a => a.hint.basePriority).First();

            Unit target = null;

            // Resolve target from Data Port if connected
            if (DataInputs.TryGetValue("Target", out var targetNode) && targetNode is Data.BTGetTargetNode getTarget)
            {
                getTarget.Evaluate(blackboard); // Evaluate the data node
                target = getTarget.TargetResult;
            }

            // Fallback behavior if no Data Node is connected
            if (target == null)
            {
                if (bestAbility.hint.targetsFriendlies)
                {
                    target = blackboard.allies.OrderBy(a => a.Health.CurrentHealth).FirstOrDefault();
                }
                else
                {
                    target = blackboard.enemies.OrderBy(a => a.Health.CurrentHealth).FirstOrDefault();
                }
            }

            if (target == null)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({category}): Alvo nulo.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            blackboard.bestPlan = new AIBlackboard.PlannedAction {
                actionToExecute = bestAbility.action,
                targetUnit = target,
                moveTarget = null
            };

            string abilityName = bestAbility.action != null ? bestAbility.action.ActionName : "Desconhecida";
            CelestialCross.Combat.CombatLogger.Log($"   Ação ActionUseAbility ({category}): Planejou usar '{abilityName}' em <b>{target.DisplayName}</b>", CelestialCross.Combat.LogCategory.AI);
            return BTResult.Success;
        }
    }
}
