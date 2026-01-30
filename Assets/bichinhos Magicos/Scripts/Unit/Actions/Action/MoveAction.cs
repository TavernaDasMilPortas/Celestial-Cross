using UnityEngine;
using System.Collections.Generic;

public class MoveAction : UnitActionBase
{
    public int Range { get; set; }

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

    protected override void OnEnter()
    {
        Debug.Log($"[MoveAction] {unit.DisplayName} | Range {Range}");

        CalculateReachableTiles();
        HighlightTiles();

        selectedTile = null;
        unit.LogCanConfirm(false);
    }

    protected override void OnUpdate()
    {
        HandleTileSelection();
    }

    protected override void Resolve()
    {
        if (selectedTile == null)
            return;

        MoveUnit();
        ClearHighlight();
    }

    protected override void OnCancel()
    {
        ClearHighlight();
    }

    // =========================
    // CORE
    // =========================

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

    void HighlightTiles()
    {
        foreach (var t in validTiles)
            t.Highlight();
    }

    void ClearHighlight()
    {
        foreach (var t in validTiles)
            t.Clear();

        validTiles.Clear();
        selectedTile = null;
    }

    void HandleTileSelection()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        GridTile tile = hit.collider.GetComponent<GridTile>();
        if (tile == null || !validTiles.Contains(tile))
            return;

        selectedTile?.Highlight();
        selectedTile = tile;
        selectedTile.Select();

        state = ActionState.ReadyToConfirm;
        unit.LogCanConfirm(true);
    }

    void MoveUnit()
    {
        GridTile current = gridMap.GetTile(unit.GridPosition);
        if (current != null)
            current.IsOccupied = false;

        unit.GridPosition = selectedTile.GridPosition;
        unit.transform.position = new Vector3(
            unit.GridPosition.x,
            0f,
            unit.GridPosition.y
        );

        selectedTile.IsOccupied = true;
    }
}
