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

        // Aplica o PhaseMap definido no LevelData e gera o grid
        var phaseMap = flow.SelectedLevel.PhaseMap;
        if (phaseMap != null)
        {
            grid.phaseMap = phaseMap;
            grid.Generate();
        }
        else
        {
            Debug.LogError($"[BattleLevelBuilder] LevelData '{flow.SelectedLevel.name}' sem PhaseMap. A geração do grid falhará.");
            yield break;
        }

        // Aguarda um frame para garantir que o grid foi totalmente construído
        yield return null;

        if (clearExistingUnits)
        {
            // A lógica de limpar unidades precisa ser revista, pois o grid é recriado.
            // Por enquanto, vamos assumir que a recriação do grid já limpa o cenário.
        }

        // Spawns dos inimigos
        SpawnEnemies(flow, grid);

        // Inicia a fase de posicionamento do jogador
        if (placementManager != null)
        {
            placementManager.OnPlacementEnded += HandlePlacementEnded;
            placementManager.StartPlacementPhase();
        }
        else
        {
            Debug.LogError("[BattleLevelBuilder] PlacementManager não está configurado!");
            // Se não há placement, podemos considerar iniciar o combate aqui se for o caso
            if (autoStartCombatAfterBuild)
            {
                StartCombat();
            }
        }
    }

    private void HandlePlacementEnded()
    {
        // Remove o listener para não ser chamado múltiplas vezes
        if (placementManager != null)
        {
            placementManager.OnPlacementEnded -= HandlePlacementEnded;
        }

        Debug.Log("[BattleLevelBuilder] Fase de posicionamento concluída.");

        if (autoStartCombatAfterBuild)
        {
            StartCombat();
        }
    }

    private void StartCombat()
    {
        var initializer = FindFirstObjectByType<CombatInitializer>();
        if (initializer != null)
        {
            Debug.Log("[BattleLevelBuilder] Iniciando o combate.");
            initializer.StartCombat();
        }
        else
        {
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
