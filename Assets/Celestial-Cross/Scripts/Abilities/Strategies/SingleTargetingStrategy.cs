using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Abilities.Strategies
{
    [System.Serializable]
    public class SingleTargetingStrategy : TargetingStrategyData
    {
        public SingleTargetingStrategy()
        {
            RequiresManualSelection = true;
            ManualRange = 5;
            ManualRule = new TargetingRuleData {
                minTargets = 1,
                maxTargets = 1,
                canTargetSelf = false,
                targetFaction = TargetFaction.Enemies
            };
        }

        public override List<Unit> GetTargets(CombatContext context)
        {
            // O AbilityExecutor cuida da seleção manual via TargetSelector.
            // Este método retorna apenas o que já estiver no contexto se necessário.
            return new List<Unit>();
        }
    }
}