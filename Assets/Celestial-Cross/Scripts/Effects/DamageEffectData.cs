using UnityEngine;
using CelestialCross.Combat;
using Celestial_Cross.Scripts.Abilities;
using System;

[Serializable]
[CreateAssetMenu(menuName = "Effects/Damage")]
public class DamageEffectData : EffectData
{
    public int amount;
    public bool useDynamicVariable;
    public string dynamicVariableName;

    public override void Execute(CombatContext context)
    {
        if (context.target == null || context.target.Health == null) return;
        
        int finalAmount = amount;

        context.target.Health.TakeDamage(finalAmount);
        Debug.Log($"[Damage] {context.source.DisplayName} causou {finalAmount} de dano em {context.target.DisplayName}");
    }
}