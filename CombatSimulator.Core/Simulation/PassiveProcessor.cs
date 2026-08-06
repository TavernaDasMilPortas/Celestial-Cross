using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;
using CombatSimulator.Core.Interpreter;

namespace CombatSimulator.Core.Simulation
{
    public static class PassiveProcessor
    {
        public static void TriggerHook(
            SimCombatHook hook,
            SimCombatContext context,
            List<BattleEvent> events,
            string unitId)
        {
            var unit = context.State.Units.FirstOrDefault(u => u.UnitId == unitId);
            if (unit == null || !unit.IsAlive) return;

            foreach (var condition in unit.ActiveConditions.ToList())
            {
                if (!context.State.AbilityLibrary.TryGetValue(condition.GraphId, out var graph)) continue;

                if (condition.IsExecuting) continue;
                condition.IsExecuting = true;

                var subContext = context.Clone();
                subContext.Variables["stacks"] = condition.Stacks;
                subContext.SourceUnitId = unitId;

                HeadlessGraphInterpreter.ExecuteGraph(graph, subContext, hook, events);

                condition.IsExecuting = false;
            }
        }

        public static void TickConditions(UnitState unit, List<BattleEvent> events)
        {
            for (int i = unit.ActiveConditions.Count - 1; i >= 0; i--)
            {
                var cond = unit.ActiveConditions[i];
                if (cond.IsPersistent) continue;

                cond.RemainingTurns--;
                if (cond.RemainingTurns <= 0)
                {
                    events.Add(new BattleEvent
                    {
                        EventType = "ConditionRemoved",
                        TargetUnitId = unit.UnitId,
                        StatusId = cond.GraphId
                    });
                    unit.ActiveConditions.RemoveAt(i);
                }
            }
        }
    }
}
