using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class GridMap : MonoBehaviour
{
    public static GridMap Instance { get; private set; }

    public float tileSize = 1f;
    public PhaseMap phaseMap;
    public List<TileDefinition> tileDefinitions;

    private Dictionary<Vector2Int, GridTile> tiles = new();
    private List<GameObject> spawnedUnits = new();

    // =============================
    // LIFECYCLE
    // =============================

    void OnEnable()
    {
        Instance = this;

        // Garante que o grid lógico existe tanto no Editor quanto no Play
        if (phaseMap != null)
            RegenerateRuntimeGrid();
    }

    // =============================
    // PUBLIC API
    // =============================

    public void Generate()
    {
        RegenerateRuntimeGrid();
    }

    public GridTile GetTile(Vector2Int pos)
    {
        bool found = tiles.TryGetValue(pos, out var tile);

        if (!found)
            Debug.LogWarning($"[GridMap] GetTile FALHOU em {pos}");

        return tile;
    }

    public void ResetAllTileVisuals()
    {
        foreach (var tile in tiles.Values)
        {
            if (tile != null)
                tile.HardClearAllStates();
        }
    }

    // =============================
    // INTERNAL
    // =============================

    void RegenerateRuntimeGrid()
    {
        Clear();

        GenerateTiles();
        GenerateUnits();

        Debug.Log($"[GridMap] Grid lógico reconstruído. Tiles: {tiles.Count}");
    }

    void GenerateTiles()
    {
        for (int y = 0; y < phaseMap.tiles.Count; y++)
        {
            for (int x = 0; x < phaseMap.tiles[y].columns.Count; x++)
            {
                int id = phaseMap.tiles[y].columns[x];
                TileDefinition def = tileDefinitions.Find(t => t.id == id);

                if (def == null)
                    continue;

                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = new Vector3(x * tileSize, 0f, y * tileSize);

                GameObject obj = Instantiate(def.prefab, worldPos, Quaternion.identity, transform);

                GridTile tile = obj.GetComponent<GridTile>();
                tile.Init(gridPos);
                tile.IsOccupied = false;

                tiles.Add(gridPos, tile);
            }
        }
    }

    void GenerateUnits()
    {
        foreach (var spawn in phaseMap.unitSpawns)
        {
            if (spawn.unitPrefab == null)
                continue;

            if (!tiles.ContainsKey(spawn.gridPosition))
            {
                Debug.LogError($"[GridMap] Unit spawn fora do mapa em {spawn.gridPosition}");
                continue;
            }

            Vector3 worldPos = new Vector3(
                spawn.gridPosition.x * tileSize,
                0f,
                spawn.gridPosition.y * tileSize
            );

            GameObject unit = Instantiate(
                spawn.unitPrefab,
                worldPos,
                Quaternion.identity,
                transform
            );

            spawnedUnits.Add(unit);

            Unit u = unit.GetComponent<Unit>();
            if (u != null)
            {
                u.GridPosition = spawn.gridPosition;
                tiles[spawn.gridPosition].IsOccupied = true;
            }
        }
    }

    public void Clear()
    {
        tiles.Clear();
        spawnedUnits.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);
    }
}
