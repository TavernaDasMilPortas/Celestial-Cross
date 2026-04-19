using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[CreateAssetMenu(menuName = "Celestial Cross/Grid/Phase Map")]
public class PhaseMap : ScriptableObject
{
    [BoxGroup("Dimensions")]
    [HorizontalGroup("Dimensions/Size")]
    [LabelWidth(50)]
    public int width;

    [HorizontalGroup("Dimensions/Size")]
    [LabelWidth(50)]
    public int height;

    // ─── Camada 1: Tile Type ─────────────────────────────────────────────────
    [Title("Layer 1 — Tile Types")]
    [HideInInspector]
    public List<Row> tiles = new();

    // ─── Camada 2: Walkable Override ─────────────────────────────────────────
    [Title("Layer 2 — Walkable Overrides")]
    [HideInInspector]
    public List<WalkableRow> walkableOverrides = new();

    // ─── Camada 3+: Sprite Overrides (Múltiplas Camadas) ─────────────────────
    [Title("Layer 3+ — Sprite Overrides")]
    [HideInInspector]
    public List<SpriteLayer> spriteLayers = new List<SpriteLayer>();

    // ─── Unit Spawns ──────────────────────────────────────────────────────────
    [Title("Unit Spawns")]
    public List<UnitSpawnData> unitSpawns = new();

    // ─── Serializable Row Types ───────────────────────────────────────────────

    [System.Serializable]
    public class Row
    {
        public List<int> columns = new();
    }

    [System.Serializable]
    public class WalkableRow
    {
        /// <summary>
        /// true = walkable, false = blocked.
        /// Initialised to true — override must be explicitly set to false.
        /// </summary>
        public List<bool> columns = new();
    }

    [System.Serializable]
    public class SpriteRow
    {
        /// <summary>Null = use TileDefinition.defaultSprite.</summary>
        public List<Sprite> columns = new();
    }

    [System.Serializable]
    public class SpriteLayer
    {
        public string name = "Nova Camada";
        public bool isVisible = true;
        public List<SpriteRow> rows = new();
    }

    [System.Serializable]
    public class UnitSpawnData
    {
        public GameObject unitPrefab;
        public Vector2Int gridPosition;
    }

    // ─── Utilities ────────────────────────────────────────────────────────────

    public int GetTileId(int x, int y)
    {
        if (y < 0 || y >= tiles.Count) return -1;
        if (x < 0 || x >= tiles[y].columns.Count) return -1;
        return tiles[y].columns[x];
    }

    public bool GetWalkable(int x, int y)
    {
        if (y < 0 || y >= walkableOverrides.Count) return true;
        if (x < 0 || x >= walkableOverrides[y].columns.Count) return true;
        return walkableOverrides[y].columns[x];
    }

    public Sprite GetSpriteOverride(int layerIndex, int x, int y)
    {
        if (layerIndex < 0 || layerIndex >= spriteLayers.Count) return null;
        if (y < 0 || y >= spriteLayers[layerIndex].rows.Count) return null;
        if (x < 0 || x >= spriteLayers[layerIndex].rows[y].columns.Count) return null;
        return spriteLayers[layerIndex].rows[y].columns[x];
    }

    /// <summary>
    /// Resizes all three layer grids to newW × newH,
    /// preserving existing data and filling new cells with defaults.
    /// </summary>
    public void Resize(int newW, int newH)
    {
        width = newW;
        height = newH;

        // ── Camada 1 (Tile IDs)
        while (tiles.Count < newH) tiles.Add(new Row());
        while (tiles.Count > newH) tiles.RemoveAt(tiles.Count - 1);
        foreach (var row in tiles)
        {
            while (row.columns.Count < newW) row.columns.Add(-1);
            while (row.columns.Count > newW) row.columns.RemoveAt(row.columns.Count - 1);
        }

        // ── Camada 2 (Walkable)
        while (walkableOverrides.Count < newH) walkableOverrides.Add(new WalkableRow());
        while (walkableOverrides.Count > newH) walkableOverrides.RemoveAt(walkableOverrides.Count - 1);
        foreach (var row in walkableOverrides)
        {
            while (row.columns.Count < newW) row.columns.Add(true);
            while (row.columns.Count > newW) row.columns.RemoveAt(row.columns.Count - 1);
        }

        // ── Camadas de Sprites
        foreach (var layer in spriteLayers)
        {
            while (layer.rows.Count < newH) layer.rows.Add(new SpriteRow());
            while (layer.rows.Count > newH) layer.rows.RemoveAt(layer.rows.Count - 1);
            foreach (var row in layer.rows)
            {
                while (row.columns.Count < newW) row.columns.Add(null);
                while (row.columns.Count > newW) row.columns.RemoveAt(row.columns.Count - 1);
            }
        }
    }
}
