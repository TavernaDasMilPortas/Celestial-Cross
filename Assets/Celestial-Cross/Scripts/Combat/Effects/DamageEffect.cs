using UnityEngine;
using CelestialCross.Combat;

[System.Serializable]
public class DamageEffect : AbilityEffectBase
{
    public int amount = 5;

    public override void Execute(CombatContext context)
    {
        foreach (var t in EffectTargetSolver.GetTargets(context, targetType))
        {
            if (t != null && t.Health != null)
            {
                Debug.Log($"[{targetType}] DamageEffect: {amount} em {t.DisplayName}");
                t.Health.TakeDamage(amount);
            }
        }
    }
}
