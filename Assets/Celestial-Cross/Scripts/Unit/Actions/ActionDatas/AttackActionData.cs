using UnityEngine;

[System.Serializable]
public class AttackActionData : UnitActionData
{
    public int range = 1;
    public int damage = 5;

    [Header("Targeting")]
    public TargetingRuleData targeting = new TargetingRuleData();

    [Header("Area")]
    public AreaPatternData areaPattern;
    [Range(0, 3)] public int areaRotationSteps;

    public override System.Type GetRuntimeActionType()
    {
        return typeof(AttackAction);
    }

    public override void Configure(IUnitAction action)
    {
        AttackAction attack = action as AttackAction;
        if (attack == null)
        {
            Debug.LogError("[AttackActionData] Action não é AttackAction");
            return;
        }

        attack.Range = range;
        attack.Damage = damage;
        attack.TargetingRule = targeting != null ? targeting.Clone() : new TargetingRuleData();
        attack.AreaPattern = areaPattern;
        attack.AreaRotationSteps = areaRotationSteps;

        attack.MarkConfigured();

        Debug.Log($"[AttackActionData] Configure OK | Range={range} Damage={damage} Mode={targeting.mode}");
    }
}
