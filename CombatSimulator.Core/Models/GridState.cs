using System.Collections.Generic;

namespace CombatSimulator.Core.Models
{
    public class GridState
    {
        public int Width { get; set; }
        public int Height { get; set; }
        // Simple map of occupied cells for validation
        public List<SimpleVector2Int> Obstacles { get; set; } = new List<SimpleVector2Int>();
        
        public bool IsValidPosition(SimpleVector2Int pos)
        {
            return pos.X >= 0 && pos.X < Width && pos.Y >= 0 && pos.Y < Height && !Obstacles.Contains(pos);
        }
    }
}
