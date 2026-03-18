using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class TurnTimelineUI : MonoBehaviour
{
    [SerializeField] private GameObject portraitPrefab;
    [SerializeField] private Transform container;

    private List<GameObject> activePortraits = new();

    public void UpdateTimeline(IEnumerable<Unit> turnQueue)
    {
        ClearTimeline();

        if (turnQueue == null || portraitPrefab == null || container == null)
            return;

        foreach (var unit in turnQueue)
        {
            GameObject portraitObj = Instantiate(portraitPrefab, container);
            // Assuming the portrait is a simple image or a small script
            // Image img = portraitObj.GetComponentInChildren<Image>();
            // if (img != null && unit.EquippedPet != null) img.sprite = unit.EquippedPet.icon;
            
            activePortraits.Add(portraitObj);
        }
    }

    private void ClearTimeline()
    {
        foreach (var p in activePortraits)
            if (p != null) Destroy(p);
        activePortraits.Clear();
    }
}
