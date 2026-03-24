using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using System;

[Serializable]
[CreateAssetMenu(menuName = "Effects/Heal")]
public class HealEffectData : EffectData
{
    public int healAmount;

    public override void Execute(CombatContext context)
    {
        if (context.target == null || context.target.Health == null) return;
        
        context.target.Health.Heal(healAmount);
        Debug.Log($"[Heal] {context.source.DisplayName} curou {healAmount} de vida em {context.target.DisplayName}");
    }
}