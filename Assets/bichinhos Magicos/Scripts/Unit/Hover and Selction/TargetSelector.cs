using UnityEngine;
using System;
using System.Collections.Generic;

public class TargetSelector : MonoBehaviour
{
    public event Action<List<Unit>> OnTargetsConfirmed;
    public event Action OnCanceled;

    Unit source;
    int range;
    bool allowMultiple;
    bool includeSelf;

    HashSet<Unit> validTargets = new();
    List<Unit> selectedTargets = new();

    Camera cam;
    bool isActive;

    // =========================
    // UNITY
    // =========================

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

    // =========================
    // SETUP
    // =========================

    public void Begin(
        Unit sourceUnit,
        int selectionRange,
        bool multipleTargets = false,
        bool canTargetSelf = false
    )
    {
        source = sourceUnit;
        range = selectionRange;
        allowMultiple = multipleTargets;
        includeSelf = canTargetSelf;

        isActive = true;

        FindValidTargets();
        HighlightValidTargets();

        Debug.Log($"[TargetSelector] Iniciado | Range: {range} | Múltiplos: {allowMultiple}");
        Debug.Log($"[TargetSelector] Alvos válidos: {validTargets.Count}");
    }

    // =========================
    // CORE
    // =========================

    void FindValidTargets()
    {
        validTargets.Clear();

        foreach (var unit in FindObjectsOfType<Unit>())
        {
            if (!includeSelf && unit == source)
                continue;

            int dist = GridDistance(source.GridPosition, unit.GridPosition);
            if (dist <= range)
                validTargets.Add(unit);
        }
    }

    void HighlightValidTargets()
    {
        foreach (var unit in validTargets)
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            if (outline != null)
                outline.SetHover(true);
        }
    }

    void ClearAllHighlights()
    {
        foreach (var unit in validTargets)
        {
            var outline = unit.GetComponent<UnitOutlineController>();
            if (outline != null)
            {
                outline.SetHover(false);
                outline.SetSelected(false);
            }
        }
    }

    // =========================
    // INPUT
    // =========================

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

        if (!allowMultiple)
            ClearSelection();

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
            if (selectedTargets.Count == 0)
            {
                Debug.Log("[TargetSelector] Nenhum alvo selecionado");
                return;
            }

            Confirm();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    // =========================
    // FINALIZAÇÃO
    // =========================

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

    // =========================
    // UTIL
    // =========================

    int GridDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}
