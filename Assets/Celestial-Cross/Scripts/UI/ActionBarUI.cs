using UnityEngine;
using System.Collections.Generic;

public class ActionBarUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform container;

    private List<ActionButtonUI> spawnedButtons = new();
    private Unit currentUnit;

    public void GenerateButtons(Unit unit)
    {
        ClearButtons();

        if (unit == null || buttonPrefab == null || container == null)
            return;

        currentUnit = unit;
        currentUnit.OnActionChanged += HandleActionChanged;

        var actions = unit.Actions;
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            bool isClickable = true;

            if (action is Celestial_Cross.Scripts.Units.BlueprintActionWrapper wrapper)
            {
                isClickable = !wrapper.Blueprint.isPassive;
            }

            CreateButtonForAction(action, i, isClickable);
        }

        // Highlight initial action if it exists
        if (unit.CurrentAction != null)
            HandleActionChanged(unit.CurrentAction);
    }

    private void CreateButtonForAction(IUnitAction action, int index, bool isClickable)
    {
        GameObject btnObj = Instantiate(buttonPrefab, container);
        ActionButtonUI btnUI = btnObj.GetComponent<ActionButtonUI>();
        
        if (btnUI != null)
        {
            btnUI.Setup(action, index, isClickable);
            spawnedButtons.Add(btnUI);
        }
    }

    private void HandleActionChanged(IUnitAction action)
    {
        foreach (var btn in spawnedButtons)
        {
            btn.SetSelected(btn.Action == action);
        }
    }

    public void ClearButtons()
    {
        if (currentUnit != null)
        {
            currentUnit.OnActionChanged -= HandleActionChanged;
        }

        foreach (var btn in spawnedButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }
        spawnedButtons.Clear();
    }
}
