namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTCooldownDecorator : BTDecorator
    {
        public int CooldownTurns { get; set; } = 1;

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Child == null) return BTResult.Failure;

            // Check blackboard for cooldown using Guid
            if (blackboard.abilityCooldowns.TryGetValue(Guid, out int turnsLeft))
            {
                if (turnsLeft > 0)
                {
                    return BTResult.Failure;
                }
            }

            var result = Child.Evaluate(blackboard);

            if (result == BTResult.Success)
            {
                // Set cooldown
                blackboard.abilityCooldowns[Guid] = CooldownTurns;
            }

            return result;
        }
    }
}
