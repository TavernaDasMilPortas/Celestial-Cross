namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTSelector : BTComposite
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || Data.ports == null) return BTResult.Failure;
            
            string shortId = !string.IsNullOrEmpty(Guid) && Guid.Length >= 4 ? Guid.Substring(0, 4) : "Node";
            CelestialCross.Combat.CombatLogger.Log($"-> <b>Seletor ({shortId})</b>: Iniciando avaliação", CelestialCross.Combat.LogCategory.AI);

            foreach (var portName in Data.ports)
            {
                if (ChildNodes.TryGetValue(portName, out var child) && child != null)
                {
                    CelestialCross.Combat.CombatLogger.Log($"   Avaliando passo '{portName}' ({child.GetType().Name})", CelestialCross.Combat.LogCategory.AI);
                    var result = child.Evaluate(blackboard);
                    CelestialCross.Combat.CombatLogger.Log($"   Passo '{portName}' ({child.GetType().Name}) retornou: <b>{result}</b>", CelestialCross.Combat.LogCategory.AI);
                    if (result != BTResult.Failure)
                    {
                        return result; // Success or Running
                    }
                }
            }
            return BTResult.Failure;
        }
    }
}
