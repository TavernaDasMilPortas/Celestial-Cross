using UnityEngine;

[System.Serializable]
public class AttackActionData : UnitActionData
{
    public int range = 1;
    public int damage = 5;

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

        // 🔴 LINHA CRÍTICA QUE ESTAVA FALTANDO
        attack.MarkConfigured();

        Debug.Log(
            $"[AttackActionData] Configure OK | Range={range} Damage={damage}"
        );
    }
}
