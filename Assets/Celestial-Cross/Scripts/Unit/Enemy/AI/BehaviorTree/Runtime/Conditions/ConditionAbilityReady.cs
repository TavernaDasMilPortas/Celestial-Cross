using System.Linq;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionAbilityReady : BTCondition
    {
        public string abilityCategory = "Damage";

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.availableAbilities == null) return BTResult.Failure;

            var matchingAbilities = blackboard.availableAbilities.Where(a => 
                a.hint != null && 
                a.hint.category.ToString() == abilityCategory
            );

            bool isReady = false;
            foreach (var ability in matchingAbilities)
            {
                if (ability.action != null)
                {
                    isReady = true;
                    break;
                }
            }

            CelestialCross.Combat.CombatLogger.Log($"   Condição AbilityReady ({abilityCategory}): {(isReady ? "Disponível" : "Não disponível/Em cooldown")}", CelestialCross.Combat.LogCategory.AI);
            return isReady ? BTResult.Success : BTResult.Failure;
        }
    }
}
