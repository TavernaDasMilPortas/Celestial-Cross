using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[ExecuteAlways]
public class GridMap : MonoBehaviour
{
    public static GridMap Instance { get; private set; }

    public float tileSize = 1f;
    public PhaseMap phaseMap;
    public List<TileDefinition> tileDefinitions;

    [Header("Visual Configs")]
    public CelestialCross.Grid.HighlightConfig defaultHighlightConfig;
    [Tooltip("Se true, spawna PhaseMap.unitSpawns ao gerar o grid. Em geral, prefira spawnar via scripts (ex: BattleLevelBuilder / fases).")]
    [SerializeField] private bool spawnUnitsFromPhaseMap = false;

    private Dictionary<Vector2Int, GridTile> tiles = new();
    private List<GameObject> spawnedUnits = new();

    // =============================
    // DIRTY STATE
    // =============================
    private bool highlightsDirty = false;

    public void MarkHighlightsDirty()
    {
        highlightsDirty = true;
    }

    void LateUpdate()
    {
        if (highlightsDirty)
        {
            RefreshDynamicHighlights();
            highlightsDirty = false;
        }
    }

    // =============================
    // LIFECYCLE
    // =============================

    void OnEnable()
    {
        Instance = this;

        if (phaseMap != null)
            RegenerateRuntimeGrid();
    }

    // =============================
    // PUBLIC API
    // =============================

    [Button(ButtonSizes.Medium), GUIColor(0.4f, 1f, 0.4f)]
    public void Generate()
    {
        RegenerateRuntimeGrid();
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.8f, 0.4f)]
    [InfoBox("Crie uma área de 3x3 no centro para testar as bordas dinâmicas.")]
    public void TestHighlightArea()
    {
        if (phaseMap == null) return;
        
        List<Vector2Int> testArea = new List<Vector2Int>();
        int centerX = phaseMap.width / 2;
        int centerY = phaseMap.height / 2;

        for (int y = centerY - 1; y <= centerY + 1; y++)
        {
            for (int x = centerX - 1; x <= centerX + 1; x++)
            {
                if (x >= 0 && x < phaseMap.width && y >= 0 && y < phaseMap.height)
                {
                    testArea.Add(new Vector2Int(x, y));
                }
            }
        }

        ResetAllTileVisuals();
        foreach(var pos in testArea)
        {
            var tile = GetTile(pos);
            if (tile != null) tile.Highlight();
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSize, 0f, gridPos.y * tileSize);
    }

    public Unit SpawnUnitAt(GameObject prefab, Vector2Int gridPos, Team team, UnitData unitData, CelestialCross.Data.Pets.RuntimePetData runtimePetData = null, CelestialCross.Data.Pets.PetSpeciesSO petSpeciesData = null, bool overwriteIfOccupied = true)
    {
        if (prefab == null)
        {
            Debug.LogError("[GridMap] SpawnUnitAt recebeu prefab null.");
            return null;
        }

        var tile = GetTile(gridPos);
        if (tile == null)
        {
            Debug.LogError($"[GridMap] SpawnUnitAt falhou: tile inexistente em {gridPos}");
            return null;
        }

        if (tile.IsOccupied)
        {
            if (!overwriteIfOccupied)
            {
                Debug.LogWarning($"[GridMap] SpawnUnitAt abortado: tile {gridPos} já ocupado.");
                return null;
            }

            tile.IsOccupied = false;
            tile.OccupyingUnit = null;
        }

        Vector3 worldPos = GridToWorld(gridPos);
        var unitObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        spawnedUnits.Add(unitObj);

        var configurator = unitObj.GetComponent<UnitRuntimeConfigurator>();
        if (configurator == null)
        {
            Debug.LogError($"[GridMap] Prefab '{prefab.name}' não possui componente UnitRuntimeConfigurator.");
            Destroy(unitObj);
            return null;
        }

        configurator.Initialize(unitData, runtimePetData, petSpeciesData);

        var unit = unitObj.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogWarning($"[GridMap] Prefab '{prefab.name}' não possui componente Unit.");
            return null;
        }

        unit.Team = team;
        unit.GridPosition = gridPos;
        tile.IsOccupied = true;
        tile.OccupyingUnit = unit;
        
        var visual = unitObj.GetComponentInChildren<UnitVisualController>();
        if (visual != null && phaseMap != null)
        {
            visual.ForceFlip(gridPos.x > phaseMap.width / 2);
        }

        return unit;
    }

    public GridTile GetTile(Vector2Int pos)
    {
        bool found = tiles.TryGetValue(pos, out var tile);
        if (!found)
            Debug.LogWarning($"[GridMap] GetTile FALHOU em {pos}");
        return tile;
    }

    public IEnumerable<GridTile> GetAllTiles() => tiles.Values;

    public bool TileExists(Vector2Int pos) => tiles.ContainsKey(pos);

    public void ResetAllTileVisuals()
    {
        foreach (var tile in tiles.Values)
            if (tile != null)
                tile.HardClearAllStates();
    }

    public Vector2Int GetMouseGridPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GridTile tile = hit.collider.GetComponent<GridTile>();
            if (tile == null)
            {
                Unit unit = hit.collider.GetComponent<Unit>();
                if (unit != null) return unit.GridPosition;
            }
            if (tile != null) return tile.GridPosition;
        }
        return new Vector2Int(-1, -1);
    }

    /// <summary>
    /// Utility method to highlight an area directly (used by PlacementManager, etc).
    /// </summary>
    public void HighlightArea(List<Vector2Int> area, CelestialCross.Grid.HighlightType type = CelestialCross.Grid.HighlightType.Movement)
    {
        ResetAllTileVisuals();
        if (area == null || area.Count == 0) return;

        foreach (var pos in area)
        {
            var tile = GetTile(pos);
            if (tile != null) tile.Highlight();
        }
    }

    /// <summary>
    /// Lê a prioridade de cada tile no grid baseando-se nas variáveis internas originais (IsAreaPreview, IsSelected, IsHighlight),
    /// agrupa espacialmente para gerar o delineado perfeito (bitmask), e solicita ao Pool para desenhar apenas o topo da "pilha".
    /// Isso permite que as sobreposições desfaçam/reapareçam naturalmente sem destroçar as bordas umas das outras.
    /// </summary>
    public void RefreshDynamicHighlights()
    {
        if (defaultHighlightConfig == null || CelestialCross.Grid.HighlightOverlayPool.Instance == null) return;

        CelestialCross.Grid.HighlightOverlayPool.Instance.Clear();

        // 1. Coleta os grupos puros para que os cálculos de borda sejam exatos (mesmo que partes deles estejam escondidas sob outras camadas)
        HashSet<Vector2Int> rangeSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> selectSet = new HashSet<Vector2Int>();
        HashSet<Vector2Int> previewSet = new HashSet<Vector2Int>();

        foreach (var tile in tiles.Values)
        {
            if (tile == null) continue;
            if (tile.IsHighlighted) rangeSet.Add(tile.GridPosition);
            if (tile.IsSelected) selectSet.Add(tile.GridPosition);
            if (tile.IsAreaPreview || tile.IsAreaCenter) previewSet.Add(tile.GridPosition);
        }

        // 2. Renderiza as camadas em andares diferentes! Assim elas não "furam" umas às outras.
        foreach (var tile in tiles.Values)
        {
            if (tile == null) continue;

            // Camada 1: Fundo (Range)
            if (tile.IsHighlighted)
            {
                int mask = GetHighlightBitmask(tile.GridPosition, rangeSet);
                Sprite s = defaultHighlightConfig.GetSprite(mask);
                Color c = defaultHighlightConfig.GetColor(CelestialCross.Grid.HighlightType.Movement);
                if (s != null)
                {
                    Vector3 worldPos = GridToWorld(tile.GridPosition) + new Vector3(0, 0.00f, 0);
                    CelestialCross.Grid.HighlightOverlayPool.Instance.Get(worldPos, s, c);
                }
            }

            // Camada 2: Alvo Principal (Seleção Fixada)
            if (tile.IsSelected)
            {
                int mask = GetHighlightBitmask(tile.GridPosition, selectSet);
                Sprite s = defaultHighlightConfig.GetSprite(mask);
                // "Amarelo" ao clicar (Preview)
                Color c = defaultHighlightConfig.GetColor(CelestialCross.Grid.HighlightType.Preview);
                if (s != null)
                {
                    Vector3 worldPos = GridToWorld(tile.GridPosition) + new Vector3(0, 0.01f, 0);
                    CelestialCross.Grid.HighlightOverlayPool.Instance.Get(worldPos, s, c);
                }
            }

            // Camada 3: Topo (Padrão de Área do Mouse em movimento)
            if (tile.IsAreaPreview || tile.IsAreaCenter)
            {
                int mask = GetHighlightBitmask(tile.GridPosition, previewSet);
                Sprite s = defaultHighlightConfig.GetSprite(mask);
                Color c = defaultHighlightConfig.GetColor(CelestialCross.Grid.HighlightType.Preview);
                if (s != null)
                {
                    Vector3 worldPos = GridToWorld(tile.GridPosition) + new Vector3(0, 0.02f, 0);
                    CelestialCross.Grid.HighlightOverlayPool.Instance.Get(worldPos, s, c);
                }
            }
        }
    }

    private int GetHighlightBitmask(Vector2Int pos, HashSet<Vector2Int> area)
    {
        int mask = 0;
        // North (0, 1), East (1, 0), South (0, -1), West (-1, 0)
        // Bit positions: N=1, E=2, S=4, W=8
        if (area.Contains(pos + new Vector2Int(0, 1))) mask += 1;
        if (area.Contains(pos + new Vector2Int(1, 0))) mask += 2;
        if (area.Contains(pos + new Vector2Int(0, -1))) mask += 4;
        if (area.Contains(pos + new Vector2Int(-1, 0))) mask += 8;
        return mask;
    }

    // =============================
    // INTERNAL
    // =============================

    void RegenerateRuntimeGrid()
    {
        Clear();
        GenerateTiles();
        if (spawnUnitsFromPhaseMap) GenerateUnits();
        SyncCameraBounds();

        Debug.Log($"[GridMap] Grid lógico reconstruído. Tiles: {tiles.Count}");
    }

    void SyncCameraBounds()
    {
        CameraBounds bounds = Object.FindFirstObjectByType<CameraBounds>();
        if (bounds == null || tiles.Count == 0) return;

        var first = tiles.Keys.First();
        int minX = first.x, minY = first.y, maxX = first.x, maxY = first.y;

        foreach (var pos in tiles.Keys)
        {
            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x);
            maxY = Mathf.Max(maxY, pos.y);
        }

        if (bounds.bottomLeft != null)
            bounds.bottomLeft.position = new Vector3(minX * tileSize, 0, minY * tileSize);
        if (bounds.topRight != null)
            bounds.topRight.position = new Vector3(maxX * tileSize, 0, maxY * tileSize);
    }

    void GenerateTiles()
    {
        if (phaseMap == null) return;

        for (int y = 0; y < phaseMap.tiles.Count; y++)
        {
            for (int x = 0; x < phaseMap.tiles[y].columns.Count; x++)
            {
                int id = phaseMap.tiles[y].columns[x];
                TileDefinition def = tileDefinitions.Find(t => t.id == id);

                if (def == null) continue;

                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = new Vector3(x * tileSize, 0f, y * tileSize);

                GameObject obj = Instantiate(def.prefab, worldPos, Quaternion.identity, transform);

                GridTile tile = obj.GetComponent<GridTile>();
                tile.Init(gridPos);
                tile.IsOccupied = false;

                // ── Camada 1: tipo de tile ──
                tile.ApplyDefinition(def);

                // ── Camada 2: walkable override ──
                bool walkable = phaseMap.GetWalkable(x, y);
                tile.ApplyWalkableOverride(walkable);

                // ── Camada 3: sprite override (null = usa TileDefinition.defaultSprite) ──
                Sprite spriteOverride = phaseMap.GetSpriteOverride(x, y);
                tile.ApplySprite(spriteOverride != null ? spriteOverride : def.defaultSprite);

                tiles.Add(gridPos, tile);
            }
        }
    }

    void GenerateUnits()
    {
        foreach (var spawn in phaseMap.unitSpawns)
        {
            if (spawn.unitPrefab == null) continue;

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

            GameObject unit = Instantiate(spawn.unitPrefab, worldPos, Quaternion.identity, transform);
            spawnedUnits.Add(unit);

            Unit u = unit.GetComponent<Unit>();
            if (u != null)
            {
                u.GridPosition = spawn.gridPosition;
                tiles[spawn.gridPosition].IsOccupied = true;
                tiles[spawn.gridPosition].OccupyingUnit = u;
            }
        }
    }

    public void Clear()
    {
        tiles.Clear();
        spawnedUnits.Clear();

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            DestroyImmediate(child);
        }
    }
}
