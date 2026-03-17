using UnityEngine;
using System;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    public event Action<List<Unit>> OnTargetsConfirmed;
    public event Action OnCanceled;

    public IReadOnlyList<Vector2Int> SelectedPoints => selectedPoints;

    Unit source;
    int range;
    TargetingRuleData targetingRule;
    AreaPatternData areaPattern;
    int areaRotationSteps;

    HashSet<Unit> validTargets = new();
    List<Unit> selectedTargets = new();

    HashSet<GridTile> validTiles = new();
    List<GridTile> selectedTiles = new();
    List<GridTile> areaPreviewTiles = new();
    List<Vector2Int> selectedPoints = new();

    Camera cam;
    bool isActive;

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (!isActive)
            return;

        HandleMouseInput();
        HandleConfirmCancel();
    }

    public void Begin(
        Unit sourceUnit,
        int selectionRange,
        TargetingRuleData rule = null,
        AreaPatternData selectedAreaPattern = null,
        int selectedAreaRotationSteps = 0
    )
    {
        source = sourceUnit;
        range = selectionRange;
        targetingRule = rule != null ? rule.Clone() : new TargetingRuleData();
        areaPattern = selectedAreaPattern;
        areaRotationSteps = selectedAreaRotationSteps;

        selectedTargets.Clear();
        validTargets.Clear();
        selectedTiles.Clear();
        validTiles.Clear();
        areaPreviewTiles.Clear();
        selectedPoints.Clear();

        ClampRule();

        isActive = true;

        if (targetingRule.origin == TargetOrigin.Point)
            PrepareTileSelection();
        else
            PrepareUnitSelection();

        Debug.Log($"[TargetSelector] Iniciado | Range: {range} | Type: {targetingRule.mode} | Origin: {targetingRule.origin}");
    }

    void ClampRule()
    {
        targetingRule.minTargets = Mathf.Max(1, targetingRule.minTargets);
        targetingRule.maxTargets = Mathf.Max(targetingRule.minTargets, targetingRule.maxTargets);

        if (!targetingRule.allowMultiple)
        {
            targetingRule.minTargets = 1;
            targetingRule.maxTargets = 1;
        }
    }

    void PrepareUnitSelection()
    {
        FindValidTargets();
        HighlightValidTargets();
        Debug.Log($"[TargetSelector] Alvos válidos: {validTargets.Count}");
    }

    void PrepareTileSelection()
    {
        FindValidTiles();
        HighlightValidTiles();
        Debug.Log($"[TargetSelector] Tiles válidos: {validTiles.Count}");
    }

    void FindValidTargets()
    {
        foreach (var unit in FindObjectsOfType<Unit>())
        {
            if (!CanTargetUnit(unit))
                continue;

            int dist = GridDistance(source.GridPosition, unit.GridPosition);
            if (dist <= range)
                validTargets.Add(unit);
        }
    }

    void FindValidTiles()
    {
        if (GridMap.Instance == null)
            return;

        foreach (var tile in FindObjectsOfType<GridTile>())
        {
            if (GridDistance(source.GridPosition, tile.GridPosition) <= range)
                validTiles.Add(tile);
        }
    }

    bool CanTargetUnit(Unit unit)
    {
        if (unit == null)
            return false;

        if (!targetingRule.canTargetSelf && unit == source)
            return false;

        return targetingRule.targetFaction switch
        {
            TargetFaction.Allies => unit.GetType() == source.GetType(),
            TargetFaction.Enemies => unit.GetType() != source.GetType(),
            _ => true,
        };
    }

    void HighlightValidTargets()
    {
        foreach (var unit in validTargets)
            unit.GetComponent<UnitOutlineController>()?.SetHover(true);
    }

    void HighlightValidTiles()
    {
        foreach (var tile in validTiles)
            tile.Highlight();
    }

    void ClearAllHighlights()
    {
        ClearAreaPreview();

        foreach (var unit in validTargets)
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            if (outline == null)
                continue;

            outline.SetHover(false);
            outline.SetSelected(false);
        }

        foreach (var tile in validTiles)
            tile.HardClearAllStates();
    }

    void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (targetingRule.origin == TargetOrigin.Point)
        {
            GridTile tile = hit.collider.GetComponent<GridTile>();

            // Fallback: se clicou em uma Unit, tentar pegar o Tile embaixo dela
            if (tile == null)
            {
                Unit unitHit = hit.collider.GetComponent<Unit>();
                if (unitHit != null && GridMap.Instance != null)
                {
                    tile = GridMap.Instance.GetTile(unitHit.GridPosition);
                }
            }

            if (tile == null || !validTiles.Contains(tile))
                return;

            ToggleTileSelection(tile);
            return;
        }

        Unit unit = hit.collider.GetComponent<Unit>();
        if (unit == null)
            return;

        if (!validTargets.Contains(unit))
        {
            Debug.Log("[TargetSelector] Clique em alvo inválido");
            return;
        }

        ToggleSelection(unit);
    }

    void ToggleSelection(Unit unit)
    {
        var outline = unit.GetComponent<UnitOutlineController>();

        if (selectedTargets.Contains(unit))
        {
            selectedTargets.Remove(unit);
            outline?.SetSelected(false);
            return;
        }

        if (!targetingRule.allowMultiple)
            ClearSelection();

        if (targetingRule.maxTargets > 0 && selectedTargets.Count >= targetingRule.maxTargets)
            return;

        selectedTargets.Add(unit);
        outline?.SetSelected(true);
    }

    void ToggleTileSelection(GridTile tile)
    {
        if (selectedTiles.Contains(tile))
        {
            selectedTiles.Remove(tile);
            selectedPoints.Remove(tile.GridPosition);
            tile.ClearSelect();
            RefreshAreaPreview();
            return;
        }

        // Fix: Se não permite múltiplos alvos, limpar seleção anterior antes de adicionar nova
        if (!targetingRule.allowMultiple)
        {
            ClearTileSelection();
        }

        if (targetingRule.maxTargets > 0 && selectedTiles.Count >= targetingRule.maxTargets)
            return;

        selectedTiles.Add(tile);
        selectedPoints.Add(tile.GridPosition);
        tile.Select();

        RefreshAreaPreview();
    }

    IEnumerable<Vector2Int> GetPreviewOrigins()
    {
        if (targetingRule.origin == TargetOrigin.Point)
        {
            foreach (var point in selectedPoints)
                yield return point;

            yield break;
        }

        foreach (var target in selectedTargets)
            yield return target.GridPosition;
    }

    void ClearAreaPreview()
    {
        foreach (var tile in areaPreviewTiles)
        {
            tile.ClearAreaPreview();
            tile.SetAreaCenter(false);
        }

        areaPreviewTiles.Clear();
    }

    void ClearSelection()
    {
        foreach (var unit in selectedTargets)
            unit.GetComponent<UnitOutlineController>()?.SetSelected(false);

        selectedTargets.Clear();
    }

    void RefreshAreaPreview()
    {
        ClearAreaPreview();

        if (targetingRule.mode != TargetingMode.Area || areaPattern == null)
            return;

        if (GridMap.Instance == null)
            return;

        foreach (var origin in GetPreviewOrigins())
        {
            foreach (var cell in AreaResolver.ResolveCells(origin, areaPattern, areaRotationSteps))
            {
                var previewTile = GridMap.Instance.GetTile(cell);
                if (previewTile == null || areaPreviewTiles.Contains(previewTile))
                    continue;

                if (cell == origin)
                {
                    previewTile.SetAreaCenter(true);
                }
                else
                {
                    previewTile.PreviewArea();
                }
                
                areaPreviewTiles.Add(previewTile);
            }
        }

        foreach (var selectedTile in selectedTiles)
            selectedTile.Select();
    }

    void ClearTileSelection()
    {
        foreach (var tile in selectedTiles)
            tile.ClearSelect();

        selectedTiles.Clear();
        selectedPoints.Clear();
        RefreshAreaPreview();
    }


    void HandleConfirmCancel()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            int count = targetingRule.origin == TargetOrigin.Point
                ? selectedTiles.Count
                : selectedTargets.Count;

            if (count < targetingRule.minTargets)
            {
                Debug.Log($"[TargetSelector] Selecione pelo menos {targetingRule.minTargets} alvo(s)");
                return;
            }

            Confirm();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    void Confirm()
    {
        isActive = false;

        ClearAllHighlights();
        OnTargetsConfirmed?.Invoke(new List<Unit>(selectedTargets));

        Destroy(this);
    }

    void Cancel()
    {
        isActive = false;

        ClearAllHighlights();
        selectedTargets.Clear();
        selectedTiles.Clear();
        selectedPoints.Clear();

        OnCanceled?.Invoke();
        Destroy(this);
    }

    int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
