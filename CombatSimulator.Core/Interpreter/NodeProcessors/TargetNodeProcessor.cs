using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class TargetNodeProcessor
    {
        public static string Process(AbilityGraphData graph, NodeExport node, SimCombatContext context)
        {
            var data = HeadlessGraphInterpreter.Deserialize<TargetNodeData>(node.JsonData);
            if (data == null) return "Out";

            // If we are reusing the previous target, we don't need to do anything.
            if (data.reusePrevious)
            {
                return "Out";
            }

            // In headless, if it's manual, the target should have been set by the TurnAction
            if (data.sourceType == GraphTargetSourceType.Manual)
            {
                if (data.mode == GraphTargetMode.Single)
                {
                    // Manual single target should be in context.TargetUnitId from TurnAction
                    if (string.IsNullOrEmpty(context.TargetUnitId))
                    {
                        // Fallback: self
                        context.TargetUnitId = context.SourceUnitId;
                    }
                    context.TargetUnitIds.Clear();
                    context.TargetUnitIds.Add(context.TargetUnitId);
                }
                else if (data.mode == GraphTargetMode.Area)
                {
                    // Area targets from a manual position
                    context.TargetUnitIds.Clear();
                    if (context.TargetPos.HasValue)
                    {
                        // Need GridValidator to find units in area pattern
                        // For now, this is a placeholder
                    }
                }
            }
            else
            {
                // Auto strategy (e.g. passive triggers, self buffs)
                context.TargetUnitIds.Clear();
                
                if (data.targetsSelf || data.strategy == GraphAutoStrategyType.Self)
                {
                    context.TargetUnitId = context.SourceUnitId;
                    context.TargetUnitIds.Add(context.SourceUnitId);
                }
                else if (data.strategy == GraphAutoStrategyType.MainTarget)
                {
                    // Keeps existing TargetUnitId
                    if (!string.IsNullOrEmpty(context.TargetUnitId))
                    {
                        context.TargetUnitIds.Add(context.TargetUnitId);
                    }
                }
                // TODO: Implement other auto strategies like ClosestUnit, RandomTarget etc.
            }

            return "Out";
        }
    }
}
