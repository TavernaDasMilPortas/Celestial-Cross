using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleLevelBuilder : MonoBehaviour
{
    [Header("Molds")]
    [SerializeField] private GameObject playerUnitMold;
    [SerializeField] private GameObject enemyUnitMold;

    [Header("Dependencies")]
    [SerializeField] private PlacementManager placementManager;

    [Header("Spawn")]
    [SerializeField] private bool clearExistingUnits = true;

    [Header("Combat")]
    [Tooltip("Se true, chama CombatInitializer.StartCombat() após spawnar as units")]
    [SerializeField] private bool autoStartCombatAfterBuild = true;

    void Start()
    {
        StartCoroutine(BuildRoutine());
    }

    public IEnumerator BuildRoutine()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null || flow.SelectedLevel == null)
        {
            Debug.LogWarning("[BattleLevelBuilder] GameFlowManager/SelectedLevel não configurado.");
            yield break;
        }

        var grid = GridMap.Instance;
        if (grid == null)
        {
            Debug.LogError("[BattleLevelBuilder] GridMap.Instance não encontrado na cena.");
            yield break;
        }

        // Aplica o PhaseMap definido no LevelData
        if (flow.SelectedLevel.PhaseMap != null)
        {
            grid.phaseMap = flow.SelectedLevel.PhaseMap;
            grid.Generate();
        }
        else
        {
            Debug.LogWarning($"[BattleLevelBuilder] LevelData '{flow.SelectedLevel.name}' sem PhaseMap. Usando o PhaseMap já configurado no GridMap.");
        }

        if (clearExistingUnits)
        {
            ClearUnits(grid);
            ClearOccupancy(grid);
        }

        // Spawns dos inimigos
        SpawnEnemies(flow, grid);

        // Inicia a fase de posicionamento do jogador
        if (placementManager != null)
        {
            bool placementComplete = false;
            placementManager.OnPlacementEnded += () => placementComplete = true;
            placementManager.StartPlacementPhase();

            // Espera a fase de posicionamento terminar
            yield return new WaitUntil(() => placementComplete);
        }
        else
        {
            Debug.LogError("[BattleLevelBuilder] PlacementManager não está configurado!");
        }


        Debug.Log("[BattleLevelBuilder] Build concluído.");

        if (autoStartCombatAfterBuild)
        {
            var initializer = Object.FindFirstObjectByType<CombatInitializer>();
            if (initializer != null)
                initializer.StartCombat();
            else
                Debug.LogWarning("[BattleLevelBuilder] CombatInitializer não encontrado para autoStart.");
        }
    }

    private void SpawnEnemies(GameFlowManager flow, GridMap grid)
    {
        var level = flow.SelectedLevel;
        List<EnemySpawnInfo> enemySpawns = null;

        if (level.Waves != null && level.Waves.Count > 0 && level.Waves[0] != null && level.Waves[0].Enemies != null && level.Waves[0].Enemies.Count > 0)
            enemySpawns = level.Waves[0].Enemies;
        else
            enemySpawns = level.Enemies;

        if (enemySpawns != null)
        {
            foreach (var enemy in enemySpawns)
            {
                if (enemy.UnitData == null)
                    continue;

                grid.SpawnUnitAt(enemyUnitMold, enemy.GridPosition, Team.Enemy, enemy.UnitData);
            }
        }
    }

    static void ClearUnits(GridMap grid)
    {
        var units = Object.FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var u in units)
        {
            if (u == null) continue;
            Destroy(u.gameObject);
        }
    }

    static void ClearOccupancy(GridMap grid)
    {
        foreach (var tile in grid.GetAllTiles())
        {
            if (tile == null) continue;
            tile.IsOccupied = false;
            tile.OccupyingUnit = null;
        }
    }

    // Spawn movido para GridMap.SpawnUnitAt
}
