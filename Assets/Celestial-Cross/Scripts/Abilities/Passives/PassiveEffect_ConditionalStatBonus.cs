using System;
using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;
using Celestial_Cross.Scripts.Abilities.Modifiers;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class PassiveEffect_ConditionalStatBonus : ModifierData
    {
        [Header("Bonus")]
        public CombatStats statBonus;

        public override void ApplyModifier(CombatContext context)
        {
            Debug.Log($"<color=#FFA500>[ConditionalStatBonus]</color> Analisando passiva para origem: {(context.source != null ? context.source.name : "NULO")} -> alvo: {(context.target != null ? context.target.name : "NULO")}. Hook engatilhado: {triggerHook}");

            CombatLogger.Log($"ATIVADA em {context.source?.name} -> {context.target?.name ?? "Self"}", LogCategory.Passive);

            context.amount += statBonus.attack;
            if (statBonus.attack > 0)
            {
                CombatLogger.Log($"+{statBonus.attack} Dano Base injetado via Passiva", LogCategory.Passive);
            }

            if (statBonus.criticalChance > 0)
            {
                float currentCrit = 0;
                context.Variables.TryGetValue("bonus_crit_chance", out currentCrit);
                context.Variables["bonus_crit_chance"] = currentCrit + statBonus.criticalChance;
            }
            if (statBonus.defense > 0)
            {
                float currentDef = 0;
                context.Variables.TryGetValue("bonus_defense", out currentDef);
                context.Variables["bonus_defense"] = currentDef + statBonus.defense;
            }
        }
    }
}
