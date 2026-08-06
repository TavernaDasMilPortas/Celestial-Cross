using System;
using System.Collections.Generic;
using System.Linq;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class EffectProcessors
    {
        public static string ProcessModifyAP(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<ModifyAPNodeData>(node.JsonData);
            if (data == null) return "Out";

            foreach (var targetId in context.TargetUnitIds)
            {
                var target = context.State.Units.Find(u => u.UnitId == targetId);
                if (target == null) continue;

                if (data.modifyMax)
                    target.MaxAP += data.amount;
                else
                    target.CurrentAP = Math.Min(target.MaxAP, Math.Max(0, target.CurrentAP + data.amount));

                events.Add(new BattleEvent { EventType = "APModified", TargetUnitId = targetId, Amount = data.amount });
            }
            return "Out";
        }

        public static string ProcessCleanse(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<CleanseStatusNodeData>(node.JsonData);
            if (data == null) return "Out";

            foreach (var targetId in context.TargetUnitIds)
            {
                var target = context.State.Units.Find(u => u.UnitId == targetId);
                if (target == null) continue;

                for (int i = target.ActiveConditions.Count - 1; i >= 0; i--)
                {
                    var cond = target.ActiveConditions[i];
                    if ((data.allPositive && cond.IsBuff) || (data.allNegative && !cond.IsBuff))
                    {
                        events.Add(new BattleEvent { EventType = "ConditionRemoved", TargetUnitId = targetId, StatusId = cond.GraphId });
                        target.ActiveConditions.RemoveAt(i);
                    }
                }
                events.Add(new BattleEvent { EventType = "CleanseApplied", TargetUnitId = targetId });
            }
            return "Out";
        }

        public static string ProcessSacrifice(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<SacrificeHealthNodeData>(node.JsonData);
            if (data == null) return "Out";

            var source = context.GetSource();
            if (source == null) return "Out";

            int hpLoss = data.usePercentage
                ? (int)Math.Round(source.MaxHealth * (data.amount / 100f))
                : (int)Math.Round(data.amount);

            source.CurrentHealth = Math.Max(0, source.CurrentHealth - hpLoss);
            if (!string.IsNullOrEmpty(data.outputVariable))
                context.Variables[data.outputVariable] = hpLoss;

            events.Add(new BattleEvent { EventType = "HealthSacrificed", SourceUnitId = source.UnitId, Amount = hpLoss });

            if (source.CurrentHealth <= 0)
            {
                source.IsAlive = false;
                events.Add(new BattleEvent { EventType = "UnitDied", TargetUnitId = source.UnitId, SourceUnitId = source.UnitId });
            }
            return "Out";
        }

        public static string ProcessCost(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<CostNodeData>(node.JsonData);
            if (data == null) return "Out";
            // Cost deduction is mostly validation; server validates before execution
            return "Out";
        }

        public static string ProcessLimit(NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<LimitPerTurnNodeData>(node.JsonData);
            if (data == null) return "True";

            var source = context.GetSource();
            if (source == null) return "True";

            string key = $"Limit_{node.Guid}_{context.State.RoundNumber}";
            float uses = source.UnitVariables.TryGetValue(key, out float v) ? v : 0;

            if (uses < data.maxExecutionsPerTurn)
            {
                source.UnitVariables[key] = uses + 1;
                return "True";
            }
            return "False";
        }

        public static string ProcessSchedule(NodeExport node, AbilityGraphData graph, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<ScheduleExecutionNodeData>(node.JsonData);
            if (data == null) return "Out";

            var nextLink = graph.Links.Find(l => l.BaseNodeGuid == node.Guid && l.PortName == "Out");
            if (nextLink != null)
            {
                context.State.ScheduledActions[System.Guid.NewGuid().ToString()] = new ScheduledAction
                {
                    UnitId = context.SourceUnitId,
                    GraphId = graph.Name,
                    TurnsRemaining = data.delayTurns,
                    CapturedVariables = new Dictionary<string, float>(context.Variables)
                };
                return "Scheduled";
            }
            return "Out";
        }

        public static string ProcessMove(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<MoveEffectNodeData>(node.JsonData);
            if (data == null) return "Out";

            if (!context.TargetPos.HasValue) return "Out";

            string unitId = data.moveMode == MoveEffectNodeData.MoveMode.MoveCaster
                ? context.SourceUnitId
                : context.TargetUnitId;

            var unit = context.State.Units.Find(u => u.UnitId == unitId);
            if (unit == null) return "Out";

            unit.GridPosition = context.TargetPos.Value;
            events.Add(new BattleEvent { EventType = "UnitMoved", SourceUnitId = unitId, Position = context.TargetPos.Value });
            return "Out";
        }

        public static string ProcessStatModifier(NodeExport node, AbilityGraphData graph, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<StatModifierNodeData>(node.JsonData);
            if (data == null) return "Out";

            foreach (var targetId in context.TargetUnitIds)
            {
                var target = context.State.Units.Find(u => u.UnitId == targetId);
                if (target == null) continue;

                var mods = new List<StatModEntry>();
                foreach (var stat in data.stats)
                {
                    float val = stat.value;
                    if (stat.valueMode == ModifierValueMode.Variable && !string.IsNullOrEmpty(stat.valueVariable) && context.Variables.TryGetValue(stat.valueVariable, out float vv))
                        val = vv;

                    mods.Add(new StatModEntry { StatTypeName = stat.statTypeName, Value = val, IsPercent = stat.bonusType == ModifierBonusType.Percent });
                }

                events.Add(new BattleEvent
                {
                    EventType = "StatModApplied",
                    SourceUnitId = context.SourceUnitId,
                    TargetUnitId = targetId,
                    Extra = new Dictionary<string, string> { ["isBuff"] = data.isBuff.ToString() }
                });
            }
            return "Out";
        }

        public static string ProcessApplyModifier(NodeExport node, AbilityGraphData graph, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<ApplyModifierNodeData>(node.JsonData);
            if (data == null) return "Out";

            // Hit chance check
            if (data.hitChance < 100f)
            {
                int roll = context.Rng.Next(0, 100);
                if (roll >= (int)data.hitChance) return "Out";
            }

            // Find the condition graph in the ability library
            if (!string.IsNullOrEmpty(data.modifierId) && graph.Dependencies.TryGetValue(data.modifierId, out string depName))
            {
                if (context.State.AbilityLibrary.TryGetValue(depName, out var condGraph))
                {
                    foreach (var targetId in context.TargetUnitIds)
                    {
                        var target = context.State.Units.Find(u => u.UnitId == targetId);
                        if (target == null) continue;

                        // Check stacking
                        var existing = target.ActiveConditions.Find(c => c.GraphId == depName);
                        if (existing != null && condGraph.CanStack && existing.Stacks < condGraph.MaxStacks)
                        {
                            existing.Stacks += data.stacks;
                            existing.Stacks = Math.Min(existing.Stacks, condGraph.MaxStacks);
                        }
                        else if (existing == null)
                        {
                            target.ActiveConditions.Add(new ActiveCondition
                            {
                                GraphId = depName,
                                RemainingTurns = condGraph.Duration,
                                Stacks = data.stacks,
                                MaxStacks = condGraph.MaxStacks,
                                IsPersistent = condGraph.IsPersistent,
                                IsBuff = condGraph.IsBuff,
                            });
                        }

                        events.Add(new BattleEvent
                        {
                            EventType = "ConditionApplied",
                            SourceUnitId = context.SourceUnitId,
                            TargetUnitId = targetId,
                            StatusId = depName,
                            Stacks = data.stacks,
                            Duration = condGraph.Duration
                        });
                    }
                }
            }
            return "Out";
        }
    }
}
