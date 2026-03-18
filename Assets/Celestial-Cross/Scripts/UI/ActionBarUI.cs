using UnityEngine;
using System.Collections.Generic;

public class ActionBarUI : MonoBehaviour
{
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform container;

    private List<GameObject> spawnedButtons = new();

    public void GenerateButtons(Unit unit)
    {
        ClearButtons();

        if (unit == null || buttonPrefab == null || container == null)
            return;

        var actions = unit.Actions;
        for (int i = 0; i < actions.Count; i++)
        {
            GameObject btnObj = Instantiate(buttonPrefab, container);
            ActionButtonUI btnUI = btnObj.GetComponent<ActionButtonUI>();
            
            if (btnUI != null)
            {
                btnUI.Setup(actions[i], i);
            }
            
            spawnedButtons.Add(btnObj);
        }
    }

    public void ClearButtons()
    {
        foreach (var btn in spawnedButtons)
        {
            if (btn != null) Destroy(btn);
        }
        spawnedButtons.Clear();
    }
}
