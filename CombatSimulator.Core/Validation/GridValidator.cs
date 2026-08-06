using System;
using System.Collections.Generic;
using CombatSimulator.Core.Models;

namespace CombatSimulator.Core.Validation
{
    public static class GridValidator
    {
        public static bool IsValidMove(SimpleVector2Int start, SimpleVector2Int end, GridState grid, int maxRange)
        {
            if (!grid.IsValidPosition(end)) return false;

            int dist = Math.Abs(start.X - end.X) + Math.Abs(start.Y - end.Y);
            return dist <= maxRange;
        }

        public static List<SimpleVector2Int> GetCellsInPattern(SimpleVector2Int center, AreaPatternData pattern, Direction facing, GridState grid)
        {
            var cells = new List<SimpleVector2Int>();
            if (pattern == null || pattern.PatternCells == null) return cells;

            foreach (var offset in pattern.PatternCells)
            {
                var rotated = RotateOffset(offset, facing);
                var pos = new SimpleVector2Int { X = center.X + rotated.X, Y = center.Y + rotated.Y };
                if (grid.IsValidPosition(pos))
                {
                    cells.Add(pos);
                }
            }
            return cells;
        }

        private static SimpleVector2Int RotateOffset(SimpleVector2Int offset, Direction facing)
        {
            // Simple rotation logic stub
            // In a real grid (like Unity's), this depends on the specific coordinate system and facing direction
            return offset; 
        }
    }
}
