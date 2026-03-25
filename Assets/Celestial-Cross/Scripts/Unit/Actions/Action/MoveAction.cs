using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MoveAction : UnitActionBase
{
    public override int Range { get; set; }
    public override string GetDetailStats() => $"Alcance: {Range}";

    GridMap gridMap;

    HashSet<GridTile> validTiles = new();
    GridTile selectedTile;

    readonly Vector2Int[] dirs =
    {
        Vector2Int.up,
        Vector2Int.down,
        Vector2Int.left,
        Vector2Int.right
    };

    protected override void Awake()
    {
        base.Awake();
        gridMap = GridMap.Instance;
    }

    // =========================
    // BASE OVERRIDES
    // =========================

    protected override ActionContext CreateContext()
    {
        return new ActionContext(unit);
    }

    public TargetingRuleData TargetingRule { get; set; } = new TargetingRuleData
    {
        mode = TargetingMode.Area,
        origin = TargetOrigin.Point,
        minTargets = 1,
        maxTargets = 1,
        allowMultiple = false
    };


    protected override void OnEnter()
    {
        Debug.Log($"[MoveAction] {unit.DisplayName} | Range {Range}");

        CalculateReachableTiles();
        StartTargetSelection(Range, TargetingRule);
        
        // Passa a whitelist para o seletor da base
        targetSelector.UpdateWhitelist(validTiles);

        unit.LogCanConfirm(false);
    }

    protected override void OnUpdate()
    {
        // Input tratado pelo TargetSelector e UnitActionBase.HandleFinalClickConfirmation
    }

    protected override void Resolve()
    {
        if (context.targetPoints == null || context.targetPoints.Count == 0)
            return;

        MoveUnit(context.targetPoints[0]);
    }

    protected override void OnCancel()
    {
    }

    protected override void OnTargetsConfirmed(List<Unit> targets)
    {
        context.targetPoints = targetSelector.SelectedPoints.ToList();
        state = ActionState.ReadyToConfirm;
        unit.LogCanConfirm(true);
    }


    void CalculateReachableTiles()
    {
        validTiles.Clear();

        Queue<(Vector2Int, int)> queue = new();
        HashSet<Vector2Int> visited = new();

        queue.Enqueue((unit.GridPosition, 0));
        visited.Add(unit.GridPosition);

        while (queue.Count > 0)
        {
            var (pos, cost) = queue.Dequeue();
            if (cost > Range)
                continue;

            GridTile tile = gridMap.GetTile(pos);
            if (tile != null)
                validTiles.Add(tile);

            foreach (var d in dirs)
            {
                Vector2Int next = pos + d;
                if (visited.Contains(next))
                    continue;

                GridTile nextTile = gridMap.GetTile(next);
                if (nextTile == null || nextTile.IsOccupied)
                    continue;

                visited.Add(next);
                queue.Enqueue((next, cost + 1));
            }
        }

        validTiles.Remove(gridMap.GetTile(unit.GridPosition));
    }

    // Nota: O TargetSelector cuida do highlight original, mas o MoveAction 
    // pode ter lógica específica aqui se necessário.

    void MoveUnit(Vector2Int destination)
    {
        GridTile current = gridMap.GetTile(unit.GridPosition);
        if (current != null)
            current.IsOccupied = false;

        unit.GridPosition = destination;
        unit.transform.position = new Vector3(
            unit.GridPosition.x,
            0f,
            unit.GridPosition.y
        );

        GridTile destTile = gridMap.GetTile(destination);
        if (destTile != null)
            destTile.IsOccupied = true;
    }
}
