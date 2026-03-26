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
                // Usa a origem específica do padrão diagonal
                center = new Vector2Int(pattern.diagOriginX, pattern.diagOriginY);
                
                int stepsNeeded = GetDiagonalRotationSteps(pattern.referenceDiagonal, direction);
                return RotateOffsets(GetOffsetsFromPattern(patternToUse, center), stepsNeeded)
                    .Select(offset => origin + offset).ToList();
            }
        }

        var offsets = GetOffsetsFromPattern(patternToUse, center);
        int axialSteps = GetAxialRotationSteps(direction);
        return RotateOffsets(offsets, axialSteps)
            .Select(offset => origin + offset).ToList();
    }

    private static int GetAxialRotationSteps(Direction direction)
    {
        switch (direction)
        {
            case Direction.N: return 0;
            case Direction.E: return 1;
            case Direction.S: return 2;
            case Direction.W: return 3;
            default: return 0;
        }
    }

    private static int GetDiagonalRotationSteps(Direction reference, Direction target)
    {
        int refIdx = GetDiagonalIndex(reference);
        int targetIdx = GetDiagonalIndex(target);
        return (targetIdx - refIdx + 4) % 4;
    }

    private static int GetDiagonalIndex(Direction d)
    {
        switch (d)
        {
            case Direction.NE: return 0;
            case Direction.SE: return 1;
            case Direction.SW: return 2;
            case Direction.NW: return 3;
            default: return 0;
        }
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
                    // No editor, y cresce para baixo. No grid, y cresce para cima.
                    // Invertemos o sinal para que y=0 no editor seja Norte relativo ao centro.
                    offsets.Add(new Vector2Int(x - center.x, -(y - center.y))); 
                }
            }
        }
        return offsets;
    }

    private static List<Vector2Int> RotateOffsets(List<Vector2Int> offsets, int steps90Deg)
    {
        if (steps90Deg == 0) return offsets;
        return offsets.Select(p => Rotate90(p, steps90Deg)).ToList();
    }

    private static Vector2Int Rotate90(Vector2Int p, int steps)
    {
        Vector2Int res = p;
        for (int i = 0; i < steps; i++)
        {
            res = new Vector2Int(res.y, -res.x);
        }
        return res;
    }

    private static bool IsDiagonal(Direction direction)
    {
        return direction == Direction.NE || direction == Direction.SE || direction == Direction.SW || direction == Direction.NW;
    }
}
