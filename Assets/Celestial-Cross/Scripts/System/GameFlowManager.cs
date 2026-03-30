using System.Collections.Generic;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    public LevelData SelectedLevel { get; set; }
    public List<string> SelectedUnitIDs { get; set; } = new List<string>();
    public Dictionary<string, Vector2Int> UnitInitialPositions { get; set; } = new Dictionary<string, Vector2Int>();

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
