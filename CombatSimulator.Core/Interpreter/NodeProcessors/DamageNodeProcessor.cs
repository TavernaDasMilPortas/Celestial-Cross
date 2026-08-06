using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;
using CombatSimulator.Core.Simulation;

namespace CombatSimulator.Core.Interpreter.NodeProcessors
{
    public static class DamageNodeProcessor
    {
        public static string Process(NodeExport node, SimCombatContext context, List<BattleEvent> events)
        {
            var data = HeadlessGraphInterpreter.Deserialize<DamageNodeData>(node.JsonData);
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
                        float statVal = GetStatValue(statSource, scaling.StatTypeName);
                        baseVal += statVal * (scaling.Percentage / 100f);
                    }
                }
                else
                {
                    // Fallback to source attack (matches Unity behavior)
                    baseVal = source.Stats.Attack;
                }

                float multiplier = 1.0f;
                if (!string.IsNullOrEmpty(data.variableReference) && context.Variables.TryGetValue(data.variableReference, out float varMult))
                    multiplier = varMult;

                int finalAmount = (int)Math.Floor(baseVal * multiplier);

                // Create isolated sub-context per target (matches Unity stepContext pattern)
                var stepContext = context.Clone();
                stepContext.SourceUnitId = source.UnitId;
                stepContext.TargetUnitId = targetId;
                stepContext.TargetUnitIds = new List<string> { targetId };
                stepContext.Amount = finalAmount;

                DamageCalculator.Calculate(stepContext, true, events);
            }

            return "Out";
        }

        public static float GetStatValue(UnitState unit, string statTypeName)
        {
            if (unit == null) return 0;
            switch (statTypeName)
            {
                case "AttackFlat": return unit.Stats.Attack;
                case "HealthFlat": return unit.Stats.Health;
                case "DefenseFlat": return unit.Stats.Defense;
                case "SpeedFlat": return unit.Stats.Speed;
                case "CritChanceFlat": return unit.Stats.CriticalChance;
                case "CritDamageFlat": return unit.Stats.CriticalDamage;
                case "EffectAccuracyFlat": return unit.Stats.EffectAccuracy;
                case "EffectResistanceFlat": return unit.Stats.EffectResistance;
                default: return 0;
            }
        }
    }
}
