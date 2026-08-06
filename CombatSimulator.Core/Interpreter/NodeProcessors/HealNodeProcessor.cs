using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;
using CombatSimulator.Core.Simulation;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class HealNodeProcessor
    {
        public static string Process(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<HealNodeData>(node.JsonData);
            if (data == null) return "Out";

            var source = context.GetSource();
            if (source == null) return "Out";

            var targetIds = new List<string>(context.TargetUnitIds);
            foreach (var targetId in targetIds)
            {
                var target = context.State.Units.Find(u => u.UnitId == targetId);
                if (target == null || !target.IsAlive) continue;

                float baseVal = 0;

                if (data.scalings != null && data.scalings.Count > 0)
                {
                    foreach (var scaling in data.scalings)
                    {
                        var statSource = scaling.UseTargetStat ? target : source;
                        float statVal = DamageNodeProcessor.GetStatValue(statSource, scaling.StatTypeName);
                        baseVal += statVal * (scaling.Percentage / 100f);
                    }
                }
                else
                {
                    // Fallback to target max health (matches Unity behavior)
                    baseVal = target.MaxHealth;
                }

                float multiplier = 1.0f;
                if (!string.IsNullOrEmpty(data.variableReference) && context.Variables.TryGetValue(data.variableReference, out float varMult))
                    multiplier = varMult;

                int finalAmount = (int)Math.Floor(baseVal * multiplier);

                var stepContext = context.Clone();
                stepContext.SourceUnitId = source.UnitId;
                stepContext.TargetUnitId = targetId;
                stepContext.TargetUnitIds = new List<string> { targetId };
                stepContext.Amount = finalAmount;

                HealCalculator.Calculate(stepContext, data.canCrit, data.allowOverheal, events);
            }

            return "Out";
        }
    }
}
