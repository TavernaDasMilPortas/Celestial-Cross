namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public class BTSequence : BTComposite
    {
        public override BTResult Evaluate(AIBlackboard blackboard)
        {
            if (Data == null || Data.ports == null) return BTResult.Failure;

            string shortId = !string.IsNullOrEmpty(Guid) && Guid.Length >= 4 ? Guid.Substring(0, 4) : "Node";
            CelestialCross.Combat.CombatLogger.Log($"-> <b>Sequência ({shortId})</b>: Iniciando avaliação", CelestialCross.Combat.LogCategory.AI);

            foreach (var portName in Data.ports)
            {
                if (ChildNodes.TryGetValue(portName, out var child) && child != null)
                {
                    CelestialCross.Combat.CombatLogger.Log($"   Avaliando passo '{portName}' ({child.GetType().Name})", CelestialCross.Combat.LogCategory.AI);
                    var result = child.Evaluate(blackboard);
                    CelestialCross.Combat.CombatLogger.Log($"   Passo '{portName}' ({child.GetType().Name}) retornou: <b>{result}</b>", CelestialCross.Combat.LogCategory.AI);
                    if (result != BTResult.Success)
                    {
                        return result; // Failure or Running
                    }
                }
                else 
                {
                    CelestialCross.Combat.CombatLogger.Log($"   Passo '{portName}' sem conexão de filho!", CelestialCross.Combat.LogCategory.AI);
                    return BTResult.Failure; // Missing child link
                }
            }
            return BTResult.Success;
        }
    }
}
