using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class TargetSelector : MonoBehaviour
{
    public event Action<List<Unit>> OnTargetsConfirmed;
    public event Action<List<Unit>> OnSelectedTargetsChanged;
    public event Action<Unit> OnHoverChanged;
    public event Action OnCanceled;

    public IReadOnlyList<Vector2Int> SelectedPoints => selectedPoints;

    Unit sourceUnit;
    int selectionRange;
    TargetingRuleData targetingRule;
    AreaPatternData areaPattern;
    Direction currentRotation = Direction.N;
    IEnumerable<GridTile> tileWhitelist;
    bool autoRotateArea;

    public Direction CurrentRotation => currentRotation;

    HashSet<Unit> validTargets = new();
    List<Unit> selectedTargets = new();

    HashSet<GridTile> validTiles = new();
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
        Direction startingDirection = Direction.N,
        IEnumerable<GridTile> tileWhitelist = null,
        bool autoRotate = false
    )
    {
        ClearAreaPreview();
        ClearSelection();
        ClearPointSelection();
        ClearAllHighlights();

        this.sourceUnit = sourceUnit;
        this.selectionRange = selectionRange;
        targetingRule = rule != null ? rule.Clone() : new TargetingRuleData();
        areaPattern = selectedAreaPattern;
        this.currentRotation = startingDirection;
        this.tileWhitelist = tileWhitelist;
        this.autoRotateArea = autoRotate;

        selectedTargets.Clear();
        validTargets.Clear();
        validTiles.Clear();
        areaPreviewTiles.Clear();
        selectedPoints.Clear();

        ClampRule();

        isActive = true;

        if (targetingRule.origin == TargetOrigin.Point)
            PrepareTileSelection();
        else
            PrepareUnitSelection();

        if (Celestial_Cross.Scripts.UI.TargetMultiplierUIManager.Instance != null)
        {
            Celestial_Cross.Scripts.UI.TargetMultiplierUIManager.Instance.RegisterTargetSelector(this);
            Celestial_Cross.Scripts.UI.TargetMultiplierUIManager.Instance.BeginSelection(targetingRule);
        }

        Debug.Log($"[TargetSelector] Iniciado | Range: {selectionRange} | Type: {targetingRule.mode} | Origin: {targetingRule.origin}");
    }

    public void UpdateAreaConfig(AreaPatternData pattern, Direction rotation, bool autoRotate = false)
    {
        this.areaPattern = pattern;
        this.currentRotation = rotation;
        this.autoRotateArea = autoRotate;
        if (isActive) RefreshAreaPreview();
    }

    public void UpdateWhitelist(IEnumerable<GridTile> whitelist)
    {
        this.tileWhitelist = whitelist;
        if (isActive)
        {
            FindValidTiles();
            HighlightValidTiles();
        }
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
        
        // Também destacar Units válidas presas na área para melhor percepção visual no modo point
        FindValidTargets();
        HighlightValidTargets();
        
        Debug.Log($"[TargetSelector] Tiles válidos: {validTiles.Count}");
    }

    void FindValidTargets()
    {
        validTargets.Clear();
        foreach (var unit in UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None))
        {
            if (unit == sourceUnit && !targetingRule.canTargetSelf)
                continue;

            if (!CanTargetUnit(unit))
                continue;

            int dist = GridDistance(sourceUnit.GridPosition, unit.GridPosition);
            if (dist <= selectionRange)
                validTargets.Add(unit);
        }
    }

    void FindValidTiles()
    {
        if (GridMap.Instance == null)
            return;

        validTiles.Clear();
        foreach (var tile in GridMap.Instance.GetAllTiles())
        {
            if (tile == null) continue;

            // Se houver whitelist, apenas tiles nela podem ser válidos
            if (tileWhitelist != null && !tileWhitelist.Contains(tile))
                continue;

            // Se a origem for Unit, o tile PRECISA estar ocupado para ser um alvo válido de clique
            if (targetingRule.origin == TargetOrigin.Unit && !tile.IsOccupied)
                continue;

            // Previne que tiles 'Not Walkable' se tornem alvo de movimentos ou ações em áreas
            if (!tile.IsWalkable && targetingRule.origin == TargetOrigin.Point)
                continue;

            if (GridDistance(sourceUnit.GridPosition, tile.GridPosition) <= selectionRange)
                validTiles.Add(tile);
        }
    }

    bool CanTargetUnit(Unit unit)
    {
        if (unit == null)
            return false;

        if (!targetingRule.canTargetSelf && unit == sourceUnit)
            return false;

        return targetingRule.targetFaction switch
        {
            TargetFaction.Allies => unit.GetType() == sourceUnit.GetType(),
            TargetFaction.Enemies => unit.GetType() != sourceUnit.GetType(),
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

    public void ClearAllHighlights()
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

    public GridTile CurrentHoveredTile => currentHoveredTile;
    public Unit CurrentHoveredUnit => currentHoveredUnit;
    public HashSet<GridTile> ValidTiles => validTiles;
    public HashSet<Unit> ValidTargets => validTargets;

    GridTile currentHoveredTile;
    Unit currentHoveredUnit;

    void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray;
        if (RenderTextureInputManager.Instance != null)
        {
            if (!RenderTextureInputManager.Instance.TryGetRay(Input.mousePosition, out ray))
                return;
        }
        else
            ray = cam.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        if (targetingRule.origin == TargetOrigin.Point)
        {
            GridTile clickedTile = hit.collider.GetComponentInParent<GridTile>();

            // Fallback: se clicou em uma Unit, tentar pegar o Tile embaixo dela
            if (clickedTile == null)
            {
                Unit unitHit = hit.collider.GetComponentInParent<Unit>();
                if (unitHit != null && GridMap.Instance != null)
                {
                    clickedTile = GridMap.Instance.GetTile(unitHit.GridPosition);
                }
            }

            if (clickedTile == null || !validTiles.Contains(clickedTile))
                return;

            currentHoveredTile = clickedTile; // Define como hover para cálculo de rotação
            ToggleTileSelection(clickedTile);
            return;
        }

        Unit clickedUnit = hit.collider.GetComponentInParent<Unit>();
        if (clickedUnit == null)
            return;

        if (!validTargets.Contains(clickedUnit))
        {
            Debug.Log("[TargetSelector] Clique em alvo inválido");
            return;
        }

        currentHoveredUnit = clickedUnit; // Define como hover para cálculo de rotação
        ToggleSelection(clickedUnit);
    }

    void ToggleSelection(Unit unit)
    {
        var outline = unit.GetComponent<UnitOutlineController>();
        GridTile tileUnderUnit = GridMap.Instance?.GetTile(unit.GridPosition);

        if (selectedTargets.Count >= targetingRule.maxTargets)
        {
            if (selectedTargets.Contains(unit))
            {
                Confirm();
                return;
            }
            else
            {
                Unit previousUnit = selectedTargets[0];
                selectedTargets.RemoveAt(0);
                if (!selectedTargets.Contains(previousUnit))
                {
                    previousUnit.GetComponent<UnitOutlineController>()?.SetSelected(false);
                    GridMap.Instance?.GetTile(previousUnit.GridPosition)?.ClearSelect();
                }
            }
        }
        else
        {
            if (selectedTargets.Contains(unit) && !targetingRule.allowSameTargetMultipleTimes)
            {
                selectedTargets.Remove(unit);
                if (!selectedTargets.Contains(unit))
                {
                    outline?.SetSelected(false);
                    tileUnderUnit?.ClearSelect();
                }
                RefreshAreaPreview();
                OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
                return;
            }
        }

        selectedTargets.Add(unit);
        outline?.SetSelected(true);
        tileUnderUnit?.Select();

        RefreshAreaPreview();
        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
    }

    void ToggleTileSelection(GridTile tile)
    {
        if (selectedPoints.Count >= targetingRule.maxTargets)
        {
            if (selectedPoints.Contains(tile.GridPosition))
            {
                Confirm();
                return;
            }
            else
            {
                Vector2Int previousPos = selectedPoints[0];
                selectedPoints.RemoveAt(0);
                if (!selectedPoints.Contains(previousPos))
                {
                    GridMap.Instance?.GetTile(previousPos)?.ClearSelect();
                }
            }
        }
        else
        {
            if (selectedPoints.Contains(tile.GridPosition) && !targetingRule.allowSameTargetMultipleTimes)
            {
                selectedPoints.Remove(tile.GridPosition);
                if (!selectedPoints.Contains(tile.GridPosition))
                {
                    tile.ClearSelect();
                }
                RefreshAreaPreview();
                OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
                return;
            }
        }

        selectedPoints.Add(tile.GridPosition);
        tile.Select();

        RefreshAreaPreview(); 
        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
    }

    List<Unit> GetResolvedTargets(IEnumerable<Unit> baseTargets, IEnumerable<Vector2Int> basePoints)
    {
        if (targetingRule.allowSameTargetMultipleTimes)
        {
            List<Unit> finalResult = new List<Unit>();
            var allUnits = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None)
                .Where(u => u != null && (targetingRule.canTargetSelf || u != sourceUnit))
                .ToList();

            if (targetingRule.origin == TargetOrigin.Point)
            {
                foreach (var point in basePoints)
                {
                    if (targetingRule.mode == TargetingMode.Area && areaPattern != null)
                    {
                        var cells = new HashSet<Vector2Int>(AreaResolver.ResolveCells(point, areaPattern, currentRotation));
                        foreach (var u in allUnits)
                        {
                            if (cells.Contains(u.GridPosition))
                            {
                                finalResult.Add(u);
                            }
                        }
                    }
                    else
                    {
                        foreach (var u in allUnits)
                        {
                            if (u.GridPosition == point)
                            {
                                finalResult.Add(u);
                            }
                        }
                    }
                }
            }
            else
            {
                foreach (var t in baseTargets)
                {
                    if (t == null || (!targetingRule.canTargetSelf && t == sourceUnit))
                        continue;

                    if (targetingRule.mode == TargetingMode.Area && areaPattern != null)
                    {
                        var cells = new HashSet<Vector2Int>(AreaResolver.ResolveCells(t.GridPosition, areaPattern, currentRotation));
                        foreach (var u in allUnits)
                        {
                            if (cells.Contains(u.GridPosition))
                            {
                                finalResult.Add(u);
                            }
                        }
                    }
                    else
                    {
                        finalResult.Add(t);
                    }
                }
            }

            return finalResult;
        }
        else
        {
            HashSet<Vector2Int> affectedCells = new();

            if (targetingRule.mode == TargetingMode.Area && areaPattern != null)
            {
                if (targetingRule.origin == TargetOrigin.Point)
                {
                    foreach (var point in basePoints)
                    {
                        foreach (var cell in AreaResolver.ResolveCells(point, areaPattern, currentRotation))
                            affectedCells.Add(cell);
                    }
                }
                else
                {
                    foreach (var target in baseTargets)
                    {
                        if (target == null) continue;
                        foreach (var cell in AreaResolver.ResolveCells(target.GridPosition, areaPattern, currentRotation))
                            affectedCells.Add(cell);
                    }
                }
            }
            else
            {
                if (targetingRule.origin == TargetOrigin.Point)
                {
                    foreach (var point in basePoints)
                        affectedCells.Add(point);
                }
            }

            var result = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None)
                .Where(u => u != null && affectedCells.Contains(u.GridPosition))
                .Where(u => targetingRule.canTargetSelf || u != sourceUnit)
                .ToList();

            foreach (var t in baseTargets)
            {
                if (t != null && (targetingRule.canTargetSelf || t != sourceUnit) && !result.Contains(t))
                {
                    result.Add(t);
                }
            }

            return result.Distinct().ToList();
        }
    }

    public HashSet<Vector2Int> GetFinalTargetArea()
    {
        HashSet<Vector2Int> finalArea = new HashSet<Vector2Int>();
        
        if (targetingRule.mode == TargetingMode.Area && areaPattern != null)
        {
            if (targetingRule.origin == TargetOrigin.Point)
            {
                foreach (var point in selectedPoints)
                {
                    foreach (var cell in AreaResolver.ResolveCells(point, areaPattern, currentRotation))
                        finalArea.Add(cell);
                }
            }
            else
            {
                foreach (var target in selectedTargets)
                {
                    if (target == null) continue;
                    foreach (var cell in AreaResolver.ResolveCells(target.GridPosition, areaPattern, currentRotation))
                        finalArea.Add(cell);
                }
            }
        }
        else
        {
            if (targetingRule.origin == TargetOrigin.Point)
            {
                foreach (var point in selectedPoints)
                    finalArea.Add(point);
            }
            else
            {
                foreach (var target in selectedTargets)
                {
                    if (target == null) continue;
                    finalArea.Add(target.GridPosition);
                }
            }
        }

        return finalArea;
    }

    void Confirm()
    {
        sourceUnit.ConfirmAction();
        OnTargetsConfirmed?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
        isActive = false;
        ClearAllHighlights();
    }

    void Cancel()
    {
        sourceUnit.CancelAction();
        isActive = false;
        ClearAllHighlights();
        ClearSelection();
        ClearPointSelection();
        OnCanceled?.Invoke();
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

    void OnDestroy()
    {
        ClearAllHighlights();
        
        if (currentHoveredUnit != null)
            UnitHoverDetector.ForceHoverEnd(currentHoveredUnit);
            
        currentHoveredTile = null;
        currentHoveredUnit = null;
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

        if (targetingRule == null || targetingRule.mode != TargetingMode.Area || areaPattern == null)
            return;

        if (GridMap.Instance == null)
            return;

        if (autoRotateArea && areaPattern.canRotate && sourceUnit != null)
        {
            Vector2Int targetPos = sourceUnit.GridPosition;
            
            if (targetingRule.origin == TargetOrigin.Point)
            {
                if (selectedPoints.Count > 0)
                    targetPos = selectedPoints.Last();
                else if (currentHoveredTile != null)
                    targetPos = currentHoveredTile.GridPosition;
            }
            else if (targetingRule.origin == TargetOrigin.Unit)
            {
                if (selectedTargets.Count > 0)
                    targetPos = selectedTargets.Last().GridPosition;
                else if (currentHoveredUnit != null)
                    targetPos = currentHoveredUnit.GridPosition;
            }
            
            currentRotation = CalculateDirectionTowards(sourceUnit.GridPosition, targetPos);
        }

        foreach (var origin in GetPreviewOrigins())
        {
            foreach (var cell in AreaResolver.ResolveCells(origin, areaPattern, currentRotation))
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

        foreach (var point in selectedPoints)
        {
            var tile = GridMap.Instance.GetTile(point);
            if (tile != null) tile.Select();
        }
    }

    void ClearPointSelection()
    {
        foreach (var point in selectedPoints)
        {
            var tile = GridMap.Instance?.GetTile(point);
            if (tile != null) tile.ClearSelect();
        }

        selectedPoints.Clear();
        RefreshAreaPreview();
    }


    void HandleConfirmCancel()
    {
        // Enter removed or ignored because we auto-confirm now.

        if (Input.GetKeyDown(KeyCode.Escape))
            Cancel();
    }

    int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    Direction CalculateDirectionTowards(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;
        
        if (dx == 0 && dy == 0) return currentRotation; 
        
        if (Mathf.Abs(dx) > Mathf.Abs(dy)) {
            return dx > 0 ? Direction.E : Direction.W;
        } else if (Mathf.Abs(dy) > Mathf.Abs(dx)) {
            return dy > 0 ? Direction.N : Direction.S;
        } else {
            if (dx > 0 && dy > 0) return Direction.NE;
            if (dx > 0 && dy < 0) return Direction.SE;
            if (dx < 0 && dy < 0) return Direction.SW;
            if (dx < 0 && dy > 0) return Direction.NW;
        }
        return Direction.N;
    }
}
