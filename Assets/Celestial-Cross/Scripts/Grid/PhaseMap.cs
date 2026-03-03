using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Grid/Phase Map")]
public class PhaseMap : ScriptableObject
{
    public int width;
    public int height;

    public List<Row> tiles = new();

    public List<UnitSpawnData> unitSpawns = new();

    [System.Serializable]
    public class Row
    {
        public List<int> columns = new();
    }

    [System.Serializable]
    public class UnitSpawnData
    {
        public GameObject unitPrefab;
        public Vector2Int gridPosition;
    }

    public int GetTileId(int x, int y)
    {
        if (y < 0 || y >= tiles.Count) return -1;
        if (x < 0 || x >= tiles[y].columns.Count) return -1;

        return tiles[y].columns[x];
    }
}
