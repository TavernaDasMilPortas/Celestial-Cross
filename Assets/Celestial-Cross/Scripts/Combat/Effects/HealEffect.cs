using UnityEngine;
using CelestialCross.Combat;

[System.Serializable]
public class HealEffect : AbilityEffectBase
{
    public int amount = 5;

    public override void Execute(CombatContext context)
    {
        foreach (var t in EffectTargetSolver.GetTargets(context, targetType))
        {
            if (t != null && t.Health != null)
            {
                Debug.Log($"[{targetType}] HealEffect: {amount} em {t.DisplayName}");
                t.Health.Heal(amount);
            }
        }
    }
}
