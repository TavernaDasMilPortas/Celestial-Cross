using System;
using UnityEngine;
using CelestialCross.Combat;
using CelestialCross.Artifacts;
using Celestial_Cross.Scripts.Abilities.Modifiers;

namespace Celestial_Cross.Scripts.Abilities
{
    [Serializable]
    public class PassiveEffect_PercentStatBonus : ModifierData
    {
        [Header("Percent Modifiers")]
        [Tooltip("List of stat types and their percentage bonuses")]
        [SerializeReference]
        public System.Collections.Generic.List<PercentStatModifier> modifiers = new System.Collections.Generic.List<PercentStatModifier>();

        [System.Serializable]
        public class PercentStatModifier
        {
            public StatType statType;
            [Range(0, 500)] public float percentBonus = 10f; // 0-500% bonus
        }

        public override void ApplyModifier(CombatContext context)
        {
            if (modifiers.Count == 0) return;

            Debug.Log($"<color=#FFA500>[PercentStatBonus]</color> Aplicando bônus percentual para {context.target?.name ?? "Self"}");

            foreach (var mod in modifiers)
            {
                switch (mod.statType)
                {
                    case StatType.HealthPercent:
                        if (!context.Variables.ContainsKey("bonus_health_percent"))
                            context.Variables["bonus_health_percent"] = 0f;
                        context.Variables["bonus_health_percent"] = (float)context.Variables["bonus_health_percent"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Saúde injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.AttackPercent:
                        if (!context.Variables.ContainsKey("bonus_attack_percent"))
                            context.Variables["bonus_attack_percent"] = 0f;
                        context.Variables["bonus_attack_percent"] = (float)context.Variables["bonus_attack_percent"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Ataque injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.DefensePercent:
                        if (!context.Variables.ContainsKey("bonus_defense_percent"))
                            context.Variables["bonus_defense_percent"] = 0f;
                        context.Variables["bonus_defense_percent"] = (float)context.Variables["bonus_defense_percent"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Defesa injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.CriticalRate:
                        if (!context.Variables.ContainsKey("bonus_crit_rate"))
                            context.Variables["bonus_crit_rate"] = 0f;
                        context.Variables["bonus_crit_rate"] = (float)context.Variables["bonus_crit_rate"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Taxa de Crítico injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.CriticalDamage:
                        if (!context.Variables.ContainsKey("bonus_crit_damage"))
                            context.Variables["bonus_crit_damage"] = 0f;
                        context.Variables["bonus_crit_damage"] = (float)context.Variables["bonus_crit_damage"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Dano Crítico injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.EffectResistance:
                        if (!context.Variables.ContainsKey("bonus_effect_resistance"))
                            context.Variables["bonus_effect_resistance"] = 0f;
                        context.Variables["bonus_effect_resistance"] = (float)context.Variables["bonus_effect_resistance"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Resistência injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.EffectHitRate:
                        if (!context.Variables.ContainsKey("bonus_effect_hitrate"))
                            context.Variables["bonus_effect_hitrate"] = 0f;
                        context.Variables["bonus_effect_hitrate"] = (float)context.Variables["bonus_effect_hitrate"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Taxa de Acerto injetado via Passiva", LogCategory.Passive);
                        break;

                    case StatType.Speed:
                        if (!context.Variables.ContainsKey("bonus_speed_percent"))
                            context.Variables["bonus_speed_percent"] = 0f;
                        context.Variables["bonus_speed_percent"] = (float)context.Variables["bonus_speed_percent"] + mod.percentBonus;
                        CombatLogger.Log($"+{mod.percentBonus}% Velocidade injetado via Passiva", LogCategory.Passive);
                        break;
                }
            }
        }
    }
}
