using System.Collections.Generic;
using UnityEngine;

public static class AreaResolver
{
    public static List<Vector2Int> ResolveCells(Vector2Int origin, AreaPatternData pattern, int rotationSteps = 0)
    {
        List<Vector2Int> cells = new();

        if (pattern == null)
            return cells;

        pattern.EnsureShape();

        int normalizedRotation = pattern.allowRotation ? NormalizeRotation(rotationSteps) : 0;

        for (int y = 0; y < pattern.height; y++)
        {
            for (int x = 0; x < pattern.width; x++)
            {
                if (!pattern.IsActive(x, y))
                    continue;

                Vector2Int local = new Vector2Int(x - pattern.originX, y - pattern.originY);
                Vector2Int rotated = Rotate(local, normalizedRotation);
                cells.Add(origin + rotated);
            }
        }

        return cells;
    }

    static int NormalizeRotation(int steps)
    {
        int value = steps % 4;
        if (value < 0)
            value += 4;

        return value;
    }

    static Vector2Int Rotate(Vector2Int point, int steps)
    {
        return steps switch
        {
            1 => new Vector2Int(-point.y, point.x),
            2 => new Vector2Int(-point.x, -point.y),
            3 => new Vector2Int(point.y, -point.x),
            _ => point,
        };
    }
}
