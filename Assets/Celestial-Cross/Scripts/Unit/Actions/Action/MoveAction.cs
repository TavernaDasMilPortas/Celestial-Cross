using UnityEngine;
using System.Collections;
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
        ActionCategory = UnitActionCategory.Movement;
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
        PerformFinalExecution();
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
                if (nextTile == null || nextTile.IsOccupied || !nextTile.IsWalkable)
                    continue;

                visited.Add(next);
                queue.Enqueue((next, cost + 1));
            }
        }

        validTiles.Remove(gridMap.GetTile(unit.GridPosition));
    }

    // Nota: O TargetSelector cuida do highlight original, mas o MoveAction 
    // pode ter lógica específica aqui se necessário.

    public void MoveUnit(Vector2Int destination)
    {
        // Se já houver um movimento em curso no mesmo objeto, paramos para evitar conflitos
        StopAllCoroutines(); 
        StartCoroutine(MoveRoutine(destination));
    }

    public IEnumerator MoveRoutine(Vector2Int destination)
    {
        if (gridMap == null) gridMap = GridMap.Instance;
        
        GridTile current = gridMap.GetTile(unit.GridPosition);
        if (current != null)
        {
            current.IsOccupied = false;
            current.OccupyingUnit = null;
        }

        Vector2Int start = unit.GridPosition;
        Vector3 startWorldPos = unit.transform.position;
        
        // Coordenadas de destino reais do mundo
        Vector3 finalWorldPos = gridMap.GridToWorld(destination);
        finalWorldPos.y = startWorldPos.y;

        // Determina os pontos do "L" no mundo
        Vector3 cornerWorldPos = gridMap.GridToWorld(new Vector2Int(destination.x, start.y));
        cornerWorldPos.y = startWorldPos.y;
        
        float speed = 4f;

        Debug.Log($"[MoveAction] {unit.name} animando de {startWorldPos} para {finalWorldPos} via {cornerWorldPos}");

        // Move para o corner (X)
        if (Vector3.Distance(unit.transform.position, cornerWorldPos) > 0.01f)
        {
            yield return StartCoroutine(AnimateToWorld(cornerWorldPos, speed));
        }

        // Move para o destino (Y)
        if (Vector3.Distance(unit.transform.position, finalWorldPos) > 0.01f)
        {
            yield return StartCoroutine(AnimateToWorld(finalWorldPos, speed));
        }

        unit.GridPosition = destination;
        unit.transform.position = finalWorldPos;

        GridTile destTile = gridMap.GetTile(destination);
        if (destTile != null)
        {
            destTile.IsOccupied = true;
            destTile.OccupyingUnit = unit;
        }

        unit.hasMovedThisTurn = true;
    }

    IEnumerator AnimateToWorld(Vector3 targetPos, float speed)
    {
        Vector3 startPos = unit.transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        if (distance < 0.01f) yield break;

        float elapsed = 0;
        float duration = distance / speed;

        while (elapsed < duration)
        {
            unit.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        unit.transform.position = targetPos;
    }
}
// force world-based L move
