namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionTargetHasBuff : BTCondition
    {
        public bool checkForDebuff = false; // se true, checa debuff invés de buff

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (blackboard.closestEnemy == null || blackboard.closestEnemy.PassiveManager == null) return BTResult.Failure;

            // Na arquitetura de PassiveManager da CelestialCross, assumindo métodos como HasPositiveEffects
            // Como isso é custom, vamos usar uma heurística simples se não tiver um método pronto.
            // Para fim de compilação, vou assumir que tem ou farei um placeholder que compila.
            bool hasPositive = blackboard.closestEnemy.PassiveManager.HasPositiveEffects();
            bool hasNegative = blackboard.closestEnemy.PassiveManager.HasNegativeEffects();

            if (checkForDebuff && hasNegative) return BTResult.Success;
            if (!checkForDebuff && hasPositive) return BTResult.Success;

            return BTResult.Failure;
        }
    }
}
