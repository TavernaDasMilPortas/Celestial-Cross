using System;
using UnityEngine;
using System.Collections.Generic;
using Celestial_Cross.Scripts.Abilities;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities.Conditions;

namespace CelestialCross.Abilities
{
    [Serializable]
    public class PassiveEffect_ScalingDistanceBonus : PassiveEffect
    {
        [Header("Scaling")]
        [Tooltip("Bonus applied per unit of distance between source and target.")]
        public CombatStats bonusPerUnit;

        public override void Execute(CombatContext context)
        {
            if (context.source == null || context.target == null) 
            {
                Debug.LogWarning($"<color=#FFA500>[PassiveEffect_ScalingDistanceBonus]</color> Origem ou Alvo nulos! Abortando execução.");
                return;
            }

            // Usando posições da grade para determinar distância no mapa
            int distance = Mathf.Abs(Mathf.RoundToInt(context.source.transform.position.x) - Mathf.RoundToInt(context.target.transform.position.x)) + 
                           Mathf.Abs(Mathf.RoundToInt(context.source.transform.position.z) - Mathf.RoundToInt(context.target.transform.position.z));

            Debug.Log($"<color=#FFA500>[PassiveEffect_ScalingDistanceBonus]</color> Calculando bônus por distância para atacante {context.source.name} atacando alvo {context.target.name}. Distância calculada: {distance} espaços (Manhattan). Bônus configurado por unidade - Ataque: {bonusPerUnit.attack} | Chance Crítica: {bonusPerUnit.criticalChance}%");

            if (distance <= 0)
            {
                Debug.Log($"<color=#FFA500>[PassiveEffect_ScalingDistanceBonus]</color> Distância é 0 (ou menor). Nenhum bônus aplicado.");
                return;
            }

            if (bonusPerUnit.criticalChance > 0)
            {
                float totalBonusCrit = distance * bonusPerUnit.criticalChance;
                
                if (!context.Variables.ContainsKey("bonus_crit_chance")) context.Variables["bonus_crit_chance"] = 0f;
                // Accumulate without typecasting issues
                context.Variables["bonus_crit_chance"] += totalBonusCrit;

                Debug.Log($"<color=#00FF00>[PassiveEffect_ScalingDistanceBonus]</color> Chance de Crítico Bônus: +{totalBonusCrit}% (Distância: {distance} * Bônus Base: {bonusPerUnit.criticalChance}%). Total em context: {context.Variables["bonus_crit_chance"]}");
            }
            
            // Handle other stats as needed
            if (bonusPerUnit.attack > 0)
            {
                int totalAtk = Mathf.RoundToInt(distance * bonusPerUnit.attack);
                context.amount += totalAtk;
                Debug.Log($"<color=#00FF00>[PassiveEffect_ScalingDistanceBonus]</color> Ataque Bônus: +{totalAtk} (Distância: {distance} * Ataque Base: {bonusPerUnit.attack}). Novo dano inserido no alvo: {context.amount}");
            }
        }
    }
}
