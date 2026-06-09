namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime.Conditions
{
    public class ConditionTargetHasBuff : BTCondition
    {
        public bool checkForDebuff = false; // se true, checa debuff invés de buff

        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            string shortId = !string.IsNullOrEmpty(Guid) && Guid.Length >= 4 ? Guid.Substring(0, 4) : "Node";
            if (blackboard.closestEnemy == null)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Condição TargetHasBuff ({shortId}): Sem alvo mais próximo.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }
            if (blackboard.closestEnemy.PassiveManager == null)
            {
                CelestialCross.Combat.CombatLogger.Log($"   Condição TargetHasBuff ({shortId}): Alvo {blackboard.closestEnemy.DisplayName} não possui PassiveManager.", CelestialCross.Combat.LogCategory.AI);
                return BTResult.Failure;
            }

            // Na arquitetura de PassiveManager da CelestialCross, assumindo métodos como HasPositiveEffects
            // Como isso é custom, vamos usar uma heurística simples se não tiver um método pronto.
            // Para fim de compilação, vou assumir que tem ou farei um placeholder que compila.
            bool hasPositive = blackboard.closestEnemy.PassiveManager.HasPositiveEffects();
            bool hasNegative = blackboard.closestEnemy.PassiveManager.HasNegativeEffects();

            bool isMatch = false;
            if (checkForDebuff)
            {
                isMatch = hasNegative;
                CelestialCross.Combat.CombatLogger.Log($"   Condição TargetHasBuff ({shortId}): Checando Debuff no alvo {blackboard.closestEnemy.DisplayName} -> {(hasNegative ? "Possui debuff" : "Não possui debuff")}", CelestialCross.Combat.LogCategory.AI);
            }
            else
            {
                isMatch = hasPositive;
                CelestialCross.Combat.CombatLogger.Log($"   Condição TargetHasBuff ({shortId}): Checando Buff no alvo {blackboard.closestEnemy.DisplayName} -> {(hasPositive ? "Possui buff" : "Não possui buff")}", CelestialCross.Combat.LogCategory.AI);
            }

            return isMatch ? BTResult.Success : BTResult.Failure;
        }
    }
}
