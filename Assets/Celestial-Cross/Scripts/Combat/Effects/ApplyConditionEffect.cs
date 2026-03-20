using UnityEngine;
using CelestialCross.Combat;

[System.Serializable]
public class ApplyConditionEffect : AbilityEffectBase
{
    public WeaverConditionData condition;

    public override void Execute(CombatContext context)
    {
        foreach (var t in EffectTargetSolver.GetTargets(context, targetType))
        {
            if (t != null && condition != null)
            {
                var passiveManager = t.PassiveManager; // Usando a propriedade public de Unit
                if (passiveManager != null)
                {
                    Debug.Log($"[{targetType}] ApplyConditionEffect: {condition.displayName} em {t.DisplayName}");
                    passiveManager.ApplyCondition(condition, context.source);
                }
            }
        }
    }
}
