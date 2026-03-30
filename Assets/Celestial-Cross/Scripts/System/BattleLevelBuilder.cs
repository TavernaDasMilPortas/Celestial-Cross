using System.Collections.Generic;
using UnityEngine;

public class BattleLevelBuilder : MonoBehaviour
{
    [Header("Catalog")]
    [SerializeField] private UnitCatalog unitCatalog;

    [Header("Spawn")]
    [SerializeField] private bool clearExistingUnits = true;

    [Header("Combat")]
    [Tooltip("Se true, chama CombatInitializer.StartCombat() após spawnar as units")]
    [SerializeField] private bool autoStartCombatAfterBuild = true;

    void Start()
    {
        Build();
    }

    public void Build()
    {
        var flow = GameFlowManager.Instance;
        if (flow == null || flow.SelectedLevel == null)
        {
            Debug.LogWarning("[BattleLevelBuilder] GameFlowManager/SelectedLevel não configurado.");
            return;
        }

        var grid = GridMap.Instance;
        if (grid == null)
        {
            Debug.LogError("[BattleLevelBuilder] GridMap.Instance não encontrado na cena.");
            return;
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

        // Spawns do player
        foreach (var unitId in flow.SelectedUnitIDs)
        {
            if (string.IsNullOrWhiteSpace(unitId))
                continue;

            var prefab = unitCatalog != null ? unitCatalog.GetPrefab(unitId) : null;
            if (prefab == null)
            {
                Debug.LogError($"[BattleLevelBuilder] Prefab não encontrado no UnitCatalog para UnitID='{unitId}'");
                continue;
            }

            Vector2Int gridPos = ResolvePlayerSpawnPos(flow, unitId);
            grid.SpawnUnitAt(prefab, gridPos, Team.Player);
        }

        // Spawns dos inimigos
        if (flow.SelectedLevel.Enemies != null)
        {
            foreach (var enemy in flow.SelectedLevel.Enemies)
            {
                if (enemy.UnitData == null)
                    continue;

                string enemyId = enemy.UnitData.UnitID;
                if (string.IsNullOrWhiteSpace(enemyId))
                {
                    Debug.LogWarning($"[BattleLevelBuilder] Enemy UnitData '{enemy.UnitData.name}' sem UnitID.");
                    continue;
                }

                var prefab = unitCatalog != null ? unitCatalog.GetPrefab(enemyId) : null;
                if (prefab == null)
                {
                    Debug.LogError($"[BattleLevelBuilder] Prefab não encontrado no UnitCatalog para enemy UnitID='{enemyId}'");
                    continue;
                }

                grid.SpawnUnitAt(prefab, enemy.GridPosition, Team.Enemy);
            }
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

    static Vector2Int ResolvePlayerSpawnPos(GameFlowManager flow, string unitId)
    {
        if (flow.UnitInitialPositions != null && flow.UnitInitialPositions.TryGetValue(unitId, out var pos))
            return pos;

        // fallback simples: usa (0,0) e vai preenchendo linha
        int index = flow.SelectedUnitIDs.IndexOf(unitId);
        int x = index % 3;
        int y = index / 3;
        return new Vector2Int(x, y);
    }

    // Spawn movido para GridMap.SpawnUnitAt
}
