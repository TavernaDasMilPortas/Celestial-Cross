using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Simulation
{
    public static class AIResolver
    {
        public static TurnAction ResolveNextAction(BattleState state)
        {
            var unitId = state.CurrentTurnUnitId;
            if (string.IsNullOrEmpty(unitId)) return new TurnAction { Type = TurnActionType.Wait, ActingUnitId = "" };

            var unit = state.Units.Find(u => u.UnitId == unitId);
            if (unit == null || !unit.IsAlive) return new TurnAction { Type = TurnActionType.Wait, ActingUnitId = unitId };

            // Very basic stub AI: Use first ability on a random enemy
            var enemies = state.Units.Where(u => u.IsAlive && u.Team != unit.Team).ToList();
            if (enemies.Count > 0 && unit.AbilityIds.Count > 0)
            {
                var target = enemies[new Random((int)state.RngSeed).Next(enemies.Count)];
                return new TurnAction
                {
                    Type = TurnActionType.UseAbility,
                    ActingUnitId = unit.UnitId,
                    AbilityId = unit.AbilityIds[0],
                    TargetUnitId = target.UnitId
                };
            }

            return new TurnAction { Type = TurnActionType.Wait, ActingUnitId = unit.UnitId };
        }
    }
}
