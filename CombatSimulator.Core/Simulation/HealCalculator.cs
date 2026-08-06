using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Simulation
{
    public static class HealCalculator
    {
        public static void Calculate(SimCombatContext context, bool canCrit, bool allowOverheal, List<BattleEvent> events)
        {
            var source = context.GetSource();
            var target = context.GetTarget();
            if (target == null) return;

            PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeDealHeal, context, events, source?.UnitId);
            PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeTakeHeal, context, events, target.UnitId);

            int totalBase = context.Amount;
            float multiplier = 1.0f;

            context.IsCritical = false;
            if (canCrit && source != null)
            {
                int critChance = source.Stats.CriticalChance;
                context.IsCritical = context.Rng.Next(0, 100) < critChance;
            }

            float healFloat = totalBase * multiplier;
            if (context.IsCritical && source != null)
            {
                float critMult = 1.0f + (source.Stats.CriticalDamage / 100f);
                if (context.Variables.TryGetValue("crit_mult_bonus", out float cm)) critMult += cm;
                if (context.Variables.TryGetValue("bonus_crit_damage", out float bcd)) critMult += (bcd / 100f);
                healFloat *= critMult;
            }

            int finalHeal = Math.Max(0, (int)Math.Round(healFloat));

            if (allowOverheal)
                target.CurrentHealth += finalHeal;
            else
                target.CurrentHealth = Math.Min(target.MaxHealth, target.CurrentHealth + finalHeal);

            events.Add(new BattleEvent
            {
                EventType = "HealApplied",
                SourceUnitId = source?.UnitId,
                TargetUnitId = target.UnitId,
                Amount = finalHeal,
                IsCritical = context.IsCritical
            });

            PassiveProcessor.TriggerHook(SimCombatHook.OnAfterDealHeal, context, events, source?.UnitId);
            PassiveProcessor.TriggerHook(SimCombatHook.OnAfterTakeHeal, context, events, target.UnitId);
        }
    }
}
