using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public LevelData SelectedLevel { get; set; }
    // Filled in PreparationScene (unit selection), consumed by PlacementManager.
    public List<string> SelectedUnitIDs { get; set; } = new List<string>();

    // Filled at runtime during placement (battle scene).
    public List<Unit> PlayerFormation { get; set; } = new List<Unit>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
