using System.Linq;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Actions
{
    public class ActionUseAbility : BTAction
    {
        public AIAbilityHint.AbilityCategory category = AIAbilityHint.AbilityCategory.Damage;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.availableAbilities == null) return BTResult.Failure;

            // Na arquitetura real, o IsOnCooldown precisa ser tratado pela interface IUnitAction ou por reflection
            // Para fim de compilação sem saber a interface exata, não chamaremos IsOnCooldown diretamente se ela não existir.
            // Pelo log anterior, a AI mantinha um AICooldownTracker para monitorar os turnos.
            // Para simplificar, vou ignorar o cooldown aqui e deixar o BTCooldownDecorator lidar com isso.
            
            var abilities = blackboard.availableAbilities.Where(a => a.hint != null && a.hint.category == category).ToList();
            if (abilities.Count == 0) return BTResult.Failure;

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

            if (target == null) return BTResult.Failure;

            blackboard.bestPlan = new AIBlackboard.PlannedAction {
                actionToExecute = bestAbility.action,
                targetUnit = target,
                moveTarget = null
            };

            return BTResult.Success;
        }
    }
}
