using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction { N, NE, E, SE, S, SW, W, NW }

public static class AreaResolver
{
    public static List<Vector2Int> ResolveCells(Vector2Int origin, AreaPatternData pattern, Direction direction = Direction.N)
    {
        if (pattern == null) return new List<Vector2Int>();

        var patternToUse = pattern.Rows.Select(r => r.cells).ToList();
        var center = new Vector2Int(pattern.originX, pattern.originY);

        if (pattern.canRotate && pattern.rotationType == RotationType.EightDirections)
        {
            if (IsDiagonal(direction))
            {
                patternToUse = pattern.diagonalPattern.Select(r => r.cells).ToList();
                center = pattern.diagonalCenter;
            }
        }

        var offsets = GetOffsetsFromPattern(patternToUse, center);
        var rotatedOffsets = RotateOffsets(offsets, direction);

        return rotatedOffsets.Select(offset => origin + offset).ToList();
    }

    private static List<Vector2Int> GetOffsetsFromPattern(List<List<bool>> patternMatrix, Vector2Int center)
    {
        var offsets = new List<Vector2Int>();
        for (int y = 0; y < patternMatrix.Count; y++)
        {
            for (int x = 0; x < patternMatrix[y].Count; x++)
            {
                if (patternMatrix[y][x])
                {
                    offsets.Add(new Vector2Int(x - center.x, -(y - center.y)));
                }
            }
        }
        return offsets;
    }

    private static List<Vector2Int> RotateOffsets(List<Vector2Int> offsets, Direction direction)
    {
        return offsets.Select(p => Rotate(p, direction)).ToList();
    }

    private static Vector2Int Rotate(Vector2Int point, Direction direction)
    {
        switch (direction)
        {
            case Direction.N:  return new Vector2Int(point.x, point.y);
            case Direction.NE: return new Vector2Int(point.x - point.y, point.x + point.y); // Simplificado, pode precisar de ajuste
            case Direction.E:  return new Vector2Int(-point.y, point.x);
            case Direction.SE: return new Vector2Int(-point.x - point.y, point.x - point.y); // Simplificado
            case Direction.S:  return new Vector2Int(-point.x, -point.y);
            case Direction.SW: return new Vector2Int(-point.x + point.y, -point.x - point.y); // Simplificado
            case Direction.W:  return new Vector2Int(point.y, -point.x);
            case Direction.NW: return new Vector2Int(point.x + point.y, -point.x + point.y); // Simplificado
            default:           return point;
        }
    }

    private static bool IsDiagonal(Direction direction)
    {
        return direction == Direction.NE || direction == Direction.SE || direction == Direction.SW || direction == Direction.NW;
    }
}
