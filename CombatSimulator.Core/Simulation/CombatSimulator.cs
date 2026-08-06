using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;
using CombatSimulator.Core.Interpreter;
using CombatSimulator.Core.Validation;

namespace CombatSimulator.Core.Simulation
{
    public static class CombatSimulator
    {
        public static TurnResult Simulate(BattleState state, TurnAction action)
        {
            var events = new List<BattleEvent>();
            var newState = state.DeepClone();
            var rng = new SeededRandom(newState.RngSeed);

            // Validation stub
            if (action.ActingUnitId != newState.CurrentTurnUnitId)
            {
                return new TurnResult { Success = false, ErrorMessage = "Not this unit's turn." };
            }

            var context = new SimCombatContext
            {
                State = newState,
                SourceUnitId = action.ActingUnitId,
                Variables = new Dictionary<string, float>(),
                LoopCounters = new Dictionary<string, int>(),
                Rng = rng,
                AbilityLevel = action.AbilityLevel,
                TargetPos = action.TargetPosition
            };

            switch (action.Type)
            {
                case TurnActionType.UseAbility:
                    if (newState.AbilityLibrary.TryGetValue(action.AbilityId, out var graph))
                    {
                        events.Add(new BattleEvent {
                            EventType = "AbilityUsed",
                            SourceUnitId = action.ActingUnitId,
                            AbilityId = action.AbilityId
                        });

                        if (!string.IsNullOrEmpty(action.TargetUnitId))
                            context.TargetUnitId = action.TargetUnitId;

                        HeadlessGraphInterpreter.ExecuteGraph(graph, context, SimCombatHook.OnManualCast, events);
                    }
                    break;
                case TurnActionType.Move:
                    // TODO: Move implementation
                    break;
                case TurnActionType.Wait:
                    break;
                case TurnActionType.Surrender:
                    newState.IsFinished = true;
                    newState.WinnerTeam = TeamType.Enemy;
                    events.Add(new BattleEvent { EventType = "CombatEnded", Extra = new Dictionary<string, string> { ["WinnerTeam"] = "Enemy" } });
                    break;
            }

            // Check combat end
            if (!newState.IsFinished && newState.Units.Count(u => u.Team == TeamType.Player && u.IsAlive) == 0)
            {
                newState.IsFinished = true;
                newState.WinnerTeam = TeamType.Enemy;
                events.Add(new BattleEvent { EventType = "CombatEnded", Extra = new Dictionary<string, string> { ["WinnerTeam"] = "Enemy" } });
            }
            else if (!newState.IsFinished && newState.Units.Count(u => u.Team == TeamType.Enemy && u.IsAlive) == 0)
            {
                newState.IsFinished = true;
                newState.WinnerTeam = TeamType.Player;
                events.Add(new BattleEvent { EventType = "CombatEnded", Extra = new Dictionary<string, string> { ["WinnerTeam"] = "Player" } });
            }

            if (!newState.IsFinished)
            {
                // Advance turn logic (stub)
                // TurnOrderResolver.AdvanceTurn(newState, events);
            }

            newState.RngSeed = rng.CurrentSeed;
            newState.ActionIndex++;
            newState.LastActionAtUtc = DateTime.UtcNow;

            return new TurnResult
            {
                Success = true,
                NewState = newState,
                Events = events
            };
        }
    }
}

namespace CombatSimulator.Core.Validation
{
    // Stub class to make CombatSimulator compile
    public static class TurnValidator
    {
        public static bool Validate(TurnAction action, BattleState state) => true;
    }
}
