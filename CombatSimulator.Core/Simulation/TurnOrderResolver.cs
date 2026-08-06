using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Simulation
{
    public static class TurnOrderResolver
    {
        public static void AdvanceTurn(BattleState state, List<BattleEvent> events)
        {
            if (state.IsFinished) return;

            // Remove current unit from queue
            if (state.TurnQueue.Count > 0)
            {
                var prevUnitId = state.TurnQueue[0];
                state.TurnQueue.RemoveAt(0);

                var prevUnit = state.Units.Find(u => u.UnitId == prevUnitId);
                if (prevUnit != null && prevUnit.IsAlive)
                {
                    // Trigger OnTurnEnd
                    var context = new SimCombatContext { State = state, SourceUnitId = prevUnitId };
                    PassiveProcessor.TriggerHook(SimCombatHook.OnTurnEnd, context, events, prevUnitId);
                    
                    // Decrease remaining condition turns for this unit
                    PassiveProcessor.TickConditions(prevUnit, events);
                }
            }

            // If queue is empty, start a new round
            if (state.TurnQueue.Count == 0)
            {
                state.RoundNumber++;
                
                // Trigger OnRoundEnd for all alive units
                foreach (var unit in state.Units.Where(u => u.IsAlive))
                {
                    var context = new SimCombatContext { State = state, SourceUnitId = unit.UnitId };
                    PassiveProcessor.TriggerHook(SimCombatHook.OnRoundEnd, context, events, unit.UnitId);
                }

                // Refill queue sorted by Speed descending
                var aliveUnits = state.Units.Where(u => u.IsAlive).ToList();
                aliveUnits.Sort((a, b) => b.Stats.Speed.CompareTo(a.Stats.Speed)); // Descending speed

                state.TurnQueue = aliveUnits.Select(u => u.UnitId).ToList();

                // Trigger OnRoundStart for all alive units
                foreach (var unit in state.Units.Where(u => u.IsAlive))
                {
                    var context = new SimCombatContext { State = state, SourceUnitId = unit.UnitId };
                    PassiveProcessor.TriggerHook(SimCombatHook.OnRoundStart, context, events, unit.UnitId);
                }

                events.Add(new BattleEvent { EventType = "NewRoundStarted", Amount = state.RoundNumber });
            }

            // Clean up dead units from queue
            state.TurnQueue.RemoveAll(id => 
            {
                var u = state.Units.Find(un => un.UnitId == id);
                return u == null || !u.IsAlive;
            });

            if (state.TurnQueue.Count > 0)
            {
                state.CurrentTurnUnitId = state.TurnQueue[0];
                
                var nextUnit = state.Units.Find(u => u.UnitId == state.CurrentTurnUnitId);
                if (nextUnit != null)
                {
                    // Restore AP
                    nextUnit.CurrentAP = Math.Min(nextUnit.MaxAP, nextUnit.CurrentAP + 1);
                    
                    var context = new SimCombatContext { State = state, SourceUnitId = state.CurrentTurnUnitId };
                    PassiveProcessor.TriggerHook(SimCombatHook.OnTurnStart, context, events, state.CurrentTurnUnitId);
                    
                    events.Add(new BattleEvent { EventType = "TurnStarted", SourceUnitId = state.CurrentTurnUnitId });
                }
            }
            else
            {
                state.CurrentTurnUnitId = null;
            }
        }
    }
}
