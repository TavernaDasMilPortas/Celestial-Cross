using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject que define o perfil de comportamento de um inimigo.
/// Montável no Inspector com lista de regras priorizadas e fator de aleatoriedade.
/// Create > AI > Behavior Profile
/// </summary>
[CreateAssetMenu(menuName = "AI/Behavior Profile")]
public class AIBehaviorProfile : ScriptableObject
{
    public string profileName = "New Profile";

    [Header("Behavior Rules (avaliadas por priority decrescente)")]
    [Tooltip("A primeira regra cuja condição for verdadeira será a regra ativa do turno.")]
    public List<AIBehaviorRule> rules = new();

    [Header("Randomness")]
    [Tooltip("0 = determinístico, 1 = totalmente aleatório. Recomendado: 0.05-0.15")]
    [Range(0f, 1f)] public float randomnessFactor = 0.1f;

    [Header("Fallback")]
    [Tooltip("Comportamento usado se nenhuma regra tiver condição satisfeita.")]
    public BehaviorType fallbackBehavior = BehaviorType.Balanced;

    [Tooltip("Preferência de alvo do fallback.")]
    public AITargetPreference fallbackTargetPreference = AITargetPreference.Closest;

    // =============================
    // API
    // =============================

    /// <summary>
    /// Retorna a regra ativa de maior prioridade cujas condições são satisfeitas.
    /// Se nenhuma regra qualifica, retorna null (usar fallback externamente).
    /// </summary>
    public AIBehaviorRule GetActiveRule(float hpPercent, int aliveAllies, bool isLastEnemy)
    {
        AIBehaviorRule best = null;

        foreach (var rule in rules)
        {
            if (rule == null)
                continue;

            if (!rule.EvaluateConditions(hpPercent, aliveAllies, isLastEnemy))
                continue;

            if (best == null || rule.priority > best.priority)
                best = rule;
        }

        return best;
    }
}
