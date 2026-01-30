using UnityEngine;

[System.Serializable]
public class MoveActionData : UnitActionData
{
    public int range = 3;

    public override System.Type GetRuntimeActionType()
    {
        return typeof(MoveAction);
    }

    public override void Configure(IUnitAction action)
    {
        MoveAction move = action as MoveAction;
        if (move == null)
        {
            Debug.LogError("[MoveActionData] Action não é MoveAction");
            return;
        }

        move.Range = range;
        move.MarkConfigured();

        Debug.Log(
            $"[MoveActionData] Configure OK | Range={range}"
        );
    }
}
