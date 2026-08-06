using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Simulation
{
    public static class DamageCalculator
    {
        public static void Calculate(SimCombatContext context, bool applyDefense, List<BattleEvent> events)
        {
            var source = context.GetSource();
            var target = context.GetTarget();
            if (target == null) return;

            PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeDealDamage, context, events, source?.UnitId);
            PassiveProcessor.TriggerHook(SimCombatHook.OnBeforeTakeDamage, context, events, target.UnitId);

            int totalBase = context.Amount;
            if (context.Variables.TryGetValue("bonus_flat_damage", out float flatDmg))
                totalBase += (int)Math.Round(flatDmg);

            float multiplier = 1.0f;
            if (context.Variables.TryGetValue("damage_mult", out float md)) multiplier += md;

            int critChance = source != null ? source.Stats.CriticalChance : 0;
            if (context.Variables.TryGetValue("bonus_crit_chance", out float cb))
                critChance += (int)Math.Round(cb);

            context.IsCritical = context.Rng.Next(0, 100) < Math.Clamp(critChance, 0, 100);

            float dmgFloat = totalBase * multiplier;
            if (context.IsCritical)
            {
                float critMult = 1.0f + ((source != null ? source.Stats.CriticalDamage : 50f) / 100f);
                if (context.Variables.TryGetValue("crit_mult_bonus", out float cm)) critMult += cm;
                if (context.Variables.TryGetValue("bonus_crit_damage", out float bcd)) critMult += (bcd / 100f);
                dmgFloat *= critMult;
            }

            int defense = 0;
            if (applyDefense)
            {
                defense = target.Stats.Defense;
                if (context.Variables.TryGetValue("defense_reduction", out float dr))
                    defense = Math.Max(0, defense - (int)Math.Round(dr));
            }

            int finalDamage = Math.Max(1, (int)Math.Round(dmgFloat) - defense);

            target.CurrentHealth = Math.Max(0, target.CurrentHealth - finalDamage);

            events.Add(new BattleEvent
            {
                EventType = "DamageDealt",
                SourceUnitId = source?.UnitId,
                TargetUnitId = target.UnitId,
                Amount = finalDamage,
                IsCritical = context.IsCritical
            });

            PassiveProcessor.TriggerHook(SimCombatHook.OnAfterDealDamage, context, events, source?.UnitId);
            PassiveProcessor.TriggerHook(SimCombatHook.OnAfterTakeDamage, context, events, target.UnitId);

            if (target.CurrentHealth <= 0)
            {
                target.IsAlive = false;
                events.Add(new BattleEvent
                {
                    EventType = "UnitDied",
                    TargetUnitId = target.UnitId,
                    SourceUnitId = source?.UnitId
                });
                PassiveProcessor.TriggerHook(SimCombatHook.OnKill, context, events, source?.UnitId);
            }
        }
    }
}
