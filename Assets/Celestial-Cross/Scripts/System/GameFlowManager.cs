using System.Collections.Generic;
using UnityEngine;
using CelestialCross.Data.Dungeon;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public LevelData SelectedLevel { get; set; }
    
    // Novas propriedades (Dungeon Context)
    public DungeonBaseSO SelectedDungeon { get; set; }
    public DungeonLevelNode SelectedDungeonNode { get; set; }

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
