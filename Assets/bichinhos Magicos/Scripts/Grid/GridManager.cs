using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 5;
    public int height = 5;
    public float tileSize = 1f;

    public GameObject tilePrefab;

    private Dictionary<Vector2Int, GridTile> tiles = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gridPos = new(x, y);
                Vector3 worldPos = new Vector3(
                    x * tileSize,
                    0f,
                    y * tileSize
                );

                GameObject tileObj = Instantiate(
                    tilePrefab,
                    worldPos,
                    Quaternion.identity,
                    transform
                );

                GridTile tile = tileObj.GetComponent<GridTile>();
                tile.Init(gridPos);

                tiles.Add(gridPos, tile);
            }
        }
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * tileSize,
            0f,
            gridPos.y * tileSize
        );
    }

    public GridTile GetTile(Vector2Int pos)
    {
        tiles.TryGetValue(pos, out GridTile tile);
        return tile;
    }
}
