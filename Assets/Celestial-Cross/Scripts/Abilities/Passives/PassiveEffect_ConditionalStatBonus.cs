using System;
using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class PassiveEffect_ConditionalStatBonus : PassiveEffect
    {
        [Header("Conditions")]
        [Tooltip("Conditions that must be met for this bonus to apply.")]
        [SerializeReference]
        public List<AbilityConditionData> conditions = new List<AbilityConditionData>();

        [Header("Bonus")]
        public CombatStats statBonus;

        [Tooltip("Optional: Variable to scale the bonus (e.g., multiplier * 2 if condition met multiple times). Default is 1.")]
        public string scalingVariable;

        public override void Execute(CombatContext context)
        {
            Debug.Log($"<color=#FFA500>[PassiveEffect_ConditionalStatBonus]</color> Analisando passiva para origem: {(context.source != null ? context.source.name : "NULO")} -> alvo: {(context.target != null ? context.target.name : "NULO")}. Hook engatilhado: {triggerHook}");

            // Evaluate conditions
            bool allMet = true;
            foreach (var condition in conditions)
            {
                if (condition != null)
                {
                    bool met = condition.Evaluate(context);
                    Debug.Log($"<color=#FFA500>[PassiveEffect_ConditionalStatBonus]</color> Verificando condição {condition.GetType().Name}: {(met ? "<color=green>Cumprida</color>" : "<color=red>Falhou</color>")}");
                    if (!met)
                    {
                        allMet = false;
                        break;
                    }
                }
            }

            if (allMet)
            {
                CombatLogger.Log($"ATIVADA em {context.source?.name} -> {context.target?.name ?? "Self"}", LogCategory.Passive);
                
                // Ensure we are modifying the context passed by the executor
                context.amount += statBonus.attack; 
                if (statBonus.attack > 0)
                {
                    CombatLogger.Log($"+{statBonus.attack} Dano Base injetado via Passiva", LogCategory.Passive);
                }
                
                // Add bonus to variables if any critical/multiplier logic is needed
                if (statBonus.criticalChance > 0)
                {
                    float currentCrit = 0;
                    context.Variables.TryGetValue("bonus_crit_chance", out currentCrit);
                    context.Variables["bonus_crit_chance"] = currentCrit + statBonus.criticalChance;
                    Debug.Log($"<color=#00FF00>[PassiveEffect_ConditionalStatBonus]</color> Chance de crítico bônus acumulada no contexto: {context.Variables["bonus_crit_chance"]}");
                }
                if (statBonus.defense > 0)
                {
                    float currentDef = 0;
                    context.Variables.TryGetValue("bonus_defense", out currentDef);
                    context.Variables["bonus_defense"] = currentDef + statBonus.defense;
                    Debug.Log($"<color=#00FF00>[PassiveEffect_ConditionalStatBonus]</color> Defesa bônus acumulada no contexto: {context.Variables["bonus_defense"]}");
                }
            }
            else
            {
                Debug.Log($"<color=#FFA500>[PassiveEffect_ConditionalStatBonus]</color> Condições não foram atendidas. Nenhum bônus aplicado.");
            }
        }
    }
}
