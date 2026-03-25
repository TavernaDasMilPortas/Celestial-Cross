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
    int areaRotationSteps;
    IEnumerable<GridTile> tileWhitelist;

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
        int selectedAreaRotationSteps = 0,
        IEnumerable<GridTile> tileWhitelist = null
    )
    {
        this.sourceUnit = sourceUnit;
        this.selectionRange = selectionRange;
        targetingRule = rule != null ? rule.Clone() : new TargetingRuleData();
        areaPattern = selectedAreaPattern;
        this.areaRotationSteps = selectedAreaRotationSteps;
        this.tileWhitelist = tileWhitelist;

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

        Debug.Log($"[TargetSelector] Iniciado | Range: {selectionRange} | Type: {targetingRule.mode} | Origin: {targetingRule.origin}");
    }

    public void UpdateAreaConfig(AreaPatternData pattern, int rotationSteps)
    {
        this.areaPattern = pattern;
        this.areaRotationSteps = rotationSteps;
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

    GridTile currentHoveredTile;
    Unit currentHoveredUnit;

    void HandleMouseInput()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (targetingRule.origin == TargetOrigin.Point)
            {
                GridTile tile = hit.collider.GetComponent<GridTile>();
                if (tile == null)
                {
                    Unit unitHit = hit.collider.GetComponent<Unit>();
                    if (unitHit != null && GridMap.Instance != null)
                        tile = GridMap.Instance.GetTile(unitHit.GridPosition);
                }

                if (tile != currentHoveredTile)
                {
                    currentHoveredTile = tile;
                    RefreshAreaPreview(); // Garante atualização contínua do preview no hover
                    
                    if (tile != null && validTiles.Contains(tile))
                    {
                        var previewPoints = new List<Vector2Int>(selectedPoints) { tile.GridPosition };
                        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, previewPoints));
                    }
                    else
                    {
                        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
                    }
                }
            }
            // ... restante removido para aplicar o patch localmente
            else
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != currentHoveredUnit)
                {
                    if (currentHoveredUnit != null)
                        UnitHoverDetector.ForceHoverEnd(currentHoveredUnit);

                    currentHoveredUnit = unit;
                    OnHoverChanged?.Invoke(unit);

                    if (unit != null)
                    {
                        UnitHoverDetector.ForceHover(unit);
                        if (validTargets.Contains(unit))
                        {
                            var previewTargets = new List<Unit>(selectedTargets);
                            if (!previewTargets.Contains(unit))
                                previewTargets.Add(unit);

                            OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(previewTargets, selectedPoints));
                        }
                    }
                    else
                    {
                        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
                    }
                }
            }
        }

        if (!Input.GetMouseButtonDown(0))
            return;

        if (!Physics.Raycast(ray, out hit))
            return;

        if (targetingRule.origin == TargetOrigin.Point)
        {
            GridTile clickedTile = hit.collider.GetComponent<GridTile>();

            // Fallback: se clicou em uma Unit, tentar pegar o Tile embaixo dela
            if (clickedTile == null)
            {
                Unit unitHit = hit.collider.GetComponent<Unit>();
                if (unitHit != null && GridMap.Instance != null)
                {
                    clickedTile = GridMap.Instance.GetTile(unitHit.GridPosition);
                }
            }

            if (clickedTile == null || !validTiles.Contains(clickedTile))
                return;

            ToggleTileSelection(clickedTile);
            return;
        }

        Unit clickedUnit = hit.collider.GetComponent<Unit>();
        if (clickedUnit == null)
            return;

        if (!validTargets.Contains(clickedUnit))
        {
            Debug.Log("[TargetSelector] Clique em alvo inválido");
            return;
        }

        ToggleSelection(clickedUnit);
    }

    void ToggleSelection(Unit unit)
    {
        var outline = unit.GetComponent<UnitOutlineController>();
        GridTile tileUnderUnit = GridMap.Instance?.GetTile(unit.GridPosition);

        if (selectedTargets.Contains(unit))
        {
            // Se já está selecionado e clicamos de novo, confirmamos a execução
            Confirm(); // Corrigido para chamar Confirm() de forma consistente
            return;
        }

        // Se for seleção única (maxTargets == 1) e clicarmos em OUTRO alvo válido
        if (targetingRule.maxTargets == 1 && selectedTargets.Count > 0)
        {
            // Limpa o anterior antes de selecionar o novo
            Unit previous = selectedTargets[0];
            previous.GetComponent<UnitOutlineController>()?.SetSelected(false);
            GridMap.Instance?.GetTile(previous.GridPosition)?.ClearSelect();
            selectedTargets.Clear();
        }

        // FIFO Swap para seleções múltiplas
        if (targetingRule.maxTargets > 1 && selectedTargets.Count >= targetingRule.maxTargets)
        {
            Unit first = selectedTargets[0];
            selectedTargets.RemoveAt(0);
            
            first.GetComponent<UnitOutlineController>()?.SetSelected(false);
            GridMap.Instance?.GetTile(first.GridPosition)?.ClearSelect();
        }

        selectedTargets.Add(unit);
        outline?.SetSelected(true);
        tileUnderUnit?.Select();

        RefreshAreaPreview(); // Adicionado para atualizar preview de área centrada em Unit
        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
    }

    void ToggleTileSelection(GridTile tile)
    {
        if (selectedPoints.Contains(tile.GridPosition))
        {
            // Se já está selecionado e clicamos de novo, confirmamos a execução
            Confirm(); // Usar o método Confirm() que já limpa o estado
            return;
        }

        // Seleção única de Tile
        if (targetingRule.maxTargets == 1 && selectedPoints.Count > 0)
        {
            Vector2Int previousPos = selectedPoints[0];
            GridMap.Instance?.GetTile(previousPos)?.ClearSelect();
            selectedPoints.Clear();
        }

        // FIFO para múltiplos Tiles
        if (targetingRule.maxTargets > 1 && selectedPoints.Count >= targetingRule.maxTargets)
        {
            Vector2Int firstPos = selectedPoints[0];
            selectedPoints.RemoveAt(0);
            GridMap.Instance?.GetTile(firstPos)?.ClearSelect();
        }

        selectedPoints.Add(tile.GridPosition);
        tile.Select();

        RefreshAreaPreview(); // Adicionado para atualizar preview de área centrada em Point
        OnSelectedTargetsChanged?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
    }

    List<Unit> GetResolvedTargets(IEnumerable<Unit> baseTargets, IEnumerable<Vector2Int> basePoints)
    {
        HashSet<Vector2Int> affectedCells = new();

        if (targetingRule.mode == TargetingMode.Area && areaPattern != null)
        {
            if (targetingRule.origin == TargetOrigin.Point)
            {
                foreach (var point in basePoints)
                    foreach (var cell in AreaResolver.ResolveCells(point, areaPattern, areaRotationSteps))
                        affectedCells.Add(cell);
            }
            else
            {
                foreach (var target in baseTargets)
                {
                    if (target == null) continue;
                    foreach (var cell in AreaResolver.ResolveCells(target.GridPosition, areaPattern, areaRotationSteps))
                        affectedCells.Add(cell);
                }
            }
        }
        else
        {
            foreach (var point in basePoints)
                affectedCells.Add(point);
        }

        var result = UnityEngine.Object.FindObjectsByType<Unit>(FindObjectsSortMode.None)
            .Where(u => affectedCells.Contains(u.GridPosition))
            .Where(u => targetingRule.canTargetSelf || u != sourceUnit)
            .ToList();

        foreach (var t in baseTargets)
            if (t != null && (!targetingRule.canTargetSelf && t == sourceUnit ? false : true) && !result.Contains(t)) result.Add(t);

        return result.Distinct().ToList();
    }

    void Confirm()
    {
        OnTargetsConfirmed?.Invoke(GetResolvedTargets(selectedTargets, selectedPoints));
        isActive = false;
        ClearAllHighlights();
    }

    void Cancel()
    {
        isActive = false;
        ClearAllHighlights();
        selectedTargets.Clear();
        selectedTiles.Clear();
        selectedPoints.Clear();
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

    int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
