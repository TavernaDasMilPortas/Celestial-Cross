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
            SpawnUnit(prefab, grid, gridPos, Team.Player);
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

                SpawnUnit(prefab, grid, enemy.GridPosition, Team.Enemy);
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

    static void SpawnUnit(GameObject prefab, GridMap grid, Vector2Int gridPos, Team team)
    {
        var tile = grid.GetTile(gridPos);
        if (tile == null)
        {
            Debug.LogError($"[BattleLevelBuilder] Tile inexistente em {gridPos}");
            return;
        }

        if (tile.IsOccupied)
        {
            Debug.LogWarning($"[BattleLevelBuilder] Tile {gridPos} já ocupado. Sobrescrevendo.");
            tile.IsOccupied = false;
            tile.OccupyingUnit = null;
        }

        Vector3 worldPos = new Vector3(gridPos.x * grid.tileSize, 0f, gridPos.y * grid.tileSize);
        var unitObj = Object.Instantiate(prefab, worldPos, Quaternion.identity, grid.transform);

        var unit = unitObj.GetComponent<Unit>();
        if (unit != null)
        {
            unit.Team = team;
            unit.GridPosition = gridPos;
            tile.IsOccupied = true;
            tile.OccupyingUnit = unit;
        }
        else
        {
            Debug.LogWarning($"[BattleLevelBuilder] Prefab '{prefab.name}' não possui componente Unit.");
        }
    }
}
