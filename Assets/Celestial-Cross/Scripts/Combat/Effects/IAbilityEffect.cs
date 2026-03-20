using UnityEngine;
using CelestialCross.Combat;

public interface IAbilityEffect
{
    void Execute(CombatContext context);
}

[System.Serializable]
public abstract class AbilityEffectBase : IAbilityEffect
{
    public EffectTargetType targetType = EffectTargetType.Default;
    public abstract void Execute(CombatContext context);
}
