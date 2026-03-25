using UnityEngine;
using CelestialCross.Combat;

namespace Celestial_Cross.Scripts.Combat.Execution
{
    public static class DamageProcessor
    {
        public static void ProcessAndApplyDamage(CombatContext context, bool applyDefense)
        {
            if (context.target == null || context.target.Health == null) return;

            // 1. Hooks Pre-Dano (Passivas injetam amount flat e preenchem Variables)
            context.source?.TriggerPassives(CombatHook.OnBeforeDealDamage, context);
            context.target?.TriggerPassives(CombatHook.OnBeforeTakeDamage, context);

            // 2. Puxa o ataque base pós-modificações flat
            int totalBase = context.amount;

            // 3. Multiplicadores
            float multiplier = 1.0f;
            if (context.Variables.TryGetValue("damage_mult", out float md)) multiplier += md;

            // 4. Critico Dinâmico
            int critChance = context.source != null ? context.source.Stats.criticalChance : 0;
            if (context.Variables.TryGetValue("bonus_crit_chance", out float critBonus))
                critChance += Mathf.RoundToInt(critBonus);

            context.isCritical = Random.Range(0, 100) < Mathf.Clamp(critChance, 0, 100);

            // 5. Calcula Dano Bruto Multiplicado
            float dmgFloat = totalBase * multiplier;
            if (context.isCritical)
            {
                float critMult = 2.0f;
                if (context.Variables.TryGetValue("crit_mult_bonus", out float cMult)) critMult += cMult;
                dmgFloat *= critMult;
            }

            // 6. Defesa
            int defense = 0;
            if (applyDefense && context.target != null)
            {
                defense = context.target.Stats.defense;
                if (context.Variables.TryGetValue("defense_reduction", out float defRed))
                    defense = Mathf.Max(0, defense - Mathf.RoundToInt(defRed));
            }

            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(dmgFloat) - defense);

            string critText = context.isCritical ? " <color=yellow>(CRÍTICO!)</color>" : "";
            CombatLogger.Log($"{context.source?.name} atacou {context.target?.name} | DanoBase: {totalBase} | Mult: {multiplier}x | Defesa: {-defense} | Final: <b>{finalDamage}</b>{critText}", LogCategory.Damage);

            // 7. Aplica na Vida Real
            context.target.Health.TakeDamage(finalDamage, context.isCritical, context.source);

            // 8. Hooks Pos-Dano
            context.source?.TriggerPassives(CombatHook.OnAfterDealDamage, context);
            context.target?.TriggerPassives(CombatHook.OnAfterTakeDamage, context);
        }
    }
}
