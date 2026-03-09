using UnityEngine;
using System;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    public event Action<List<Unit>> OnTargetsConfirmed;
    public event Action OnCanceled;

    Unit source;
    int range;
    TargetingRuleData targetingRule;

    HashSet<Unit> validTargets = new();
    List<Unit> selectedTargets = new();

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

    public void Begin(Unit sourceUnit, int selectionRange, TargetingRuleData rule = null)
    {
        source = sourceUnit;
        range = selectionRange;
        targetingRule = rule ?? new TargetingRuleData();

        ClampRule();

        isActive = true;

        FindValidTargets();
        HighlightValidTargets();

        Debug.Log($"[TargetSelector] Iniciado | Range: {range} | Mode: {targetingRule.mode} | Max: {targetingRule.maxTargets}");
        Debug.Log($"[TargetSelector] Alvos válidos: {validTargets.Count}");
    }

    void ClampRule()
    {
        targetingRule.minTargets = Mathf.Max(1, targetingRule.minTargets);
        targetingRule.maxTargets = Mathf.Max(targetingRule.minTargets, targetingRule.maxTargets);

        if (!targetingRule.AllowMultiple)
        {
            targetingRule.minTargets = 1;
            targetingRule.maxTargets = 1;
        }
    }

    void FindValidTargets()
    {
        validTargets.Clear();

        foreach (var unit in FindObjectsOfType<Unit>())
        {
            if (!CanTargetUnit(unit))
                continue;

            int dist = GridDistance(source.GridPosition, unit.GridPosition);
            if (dist <= range)
                validTargets.Add(unit);
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
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            outline?.SetHover(true);
        }
    }

    void ClearAllHighlights()
    {
        foreach (var unit in validTargets)
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            if (outline == null)
                continue;

            outline.SetHover(false);
            outline.SetSelected(false);
        }
    }

    void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

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
            Debug.Log($"[TargetSelector] Removido: {unit.DisplayName}");
            return;
        }

        if (!targetingRule.AllowMultiple)
            ClearSelection();

        if (selectedTargets.Count >= targetingRule.maxTargets)
        {
            Debug.Log($"[TargetSelector] Limite de alvos atingido ({targetingRule.maxTargets})");
            return;
        }

        selectedTargets.Add(unit);
        outline?.SetSelected(true);

        Debug.Log($"[TargetSelector] Selecionado: {unit.DisplayName}");
    }

    void ClearSelection()
    {
        foreach (var unit in selectedTargets)
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            outline?.SetSelected(false);
        }

        selectedTargets.Clear();
    }

    void HandleConfirmCancel()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (selectedTargets.Count < targetingRule.minTargets)
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

        Debug.Log($"[TargetSelector] Confirmado ({selectedTargets.Count} alvo(s))");

        ClearAllHighlights();
        OnTargetsConfirmed?.Invoke(new List<Unit>(selectedTargets));

        Destroy(this);
    }

    void Cancel()
    {
        isActive = false;

        Debug.Log("[TargetSelector] Cancelado");

        ClearAllHighlights();
        selectedTargets.Clear();

        OnCanceled?.Invoke();
        Destroy(this);
    }

    int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
