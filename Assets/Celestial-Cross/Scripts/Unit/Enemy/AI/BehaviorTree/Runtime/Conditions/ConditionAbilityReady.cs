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

            foreach (var ability in matchingAbilities)
            {
                if (ability.action != null)
                {
                    return BTResult.Success;
                }
            }

            return BTResult.Failure;
        }
    }
}
