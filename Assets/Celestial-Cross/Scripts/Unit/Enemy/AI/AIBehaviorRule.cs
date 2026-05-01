using System;
using UnityEngine;

/// <summary>
/// Uma regra de comportamento dentro de um AIBehaviorProfile.
/// Configurável no Inspector com condições de ativação e pesos por tipo de ação.
/// </summary>
[Serializable]
public class AIBehaviorRule
{
    public string ruleName = "New Rule";
    [Range(0, 100)] public int priority = 50;

    public BehaviorType behavior = BehaviorType.Balanced;
    public AITargetPreference targetPreference = AITargetPreference.Closest;

    [Header("Role & Class Targeting")]
    [Tooltip("Role a priorizar quando targetPreference é PrioritizeRole.")]
    public UnitRole preferredRole = UnitRole.Support;
    [Tooltip("Class a priorizar quando targetPreference é PrioritizeClass.")]
    public UnitClass preferredClass = UnitClass.Healer;

    // =============================
    // CONDIÇÕES DE ATIVAÇÃO
    // =============================

    [Header("Conditions")]
    [Tooltip("Ativa quando HP da unit está abaixo deste percentual (0-1). 1 = sempre.")]
    [Range(0f, 1f)] public float activateWhenHpBelow = 1f;

    [Tooltip("Ativa quando a unit é o último inimigo vivo.")]
    public bool activateWhenAlone;

    [Tooltip("Ativa quando aliados vivos são <= a este número. 0 = desativado.")]
    [Min(0)] public int activateWhenAlliesBelow = 0;

    // =============================
    // PESOS DE SCORING
    // =============================

    [Header("Scoring Weights")]
    [Range(0f, 2f)] public float attackWeight = 1f;
    [Range(0f, 2f)] public float moveWeight = 1f;
    [Range(0f, 2f)] public float abilityWeight = 1f;

    [Header("Ability Category Weights")]
    [Range(0f, 3f)] public float damageWeight = 1f;
    [Range(0f, 3f)] public float healWeight = 1f;
    [Range(0f, 3f)] public float buffWeight = 1f;
    [Range(0f, 3f)] public float debuffWeight = 1f;

    // =============================
    // AVALIAÇÃO DE CONDIÇÕES
    // =============================

    /// <summary>
    /// Verifica se as condições dessa regra são satisfeitas pelo contexto atual.
    /// </summary>
    public bool EvaluateConditions(float hpPercent, int aliveAllies, bool isLastEnemy)
    {
        // HP abaixo do limiar
        if (hpPercent > activateWhenHpBelow)
            return false;

        // Precisa ser o último e não é
        if (activateWhenAlone && !isLastEnemy)
            return false;

        // Aliados abaixo do limiar
        if (activateWhenAlliesBelow > 0 && aliveAllies > activateWhenAlliesBelow)
            return false;

        return true;
    }
}
