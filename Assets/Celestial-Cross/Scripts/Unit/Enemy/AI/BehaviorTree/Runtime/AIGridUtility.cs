using System.Collections.Generic;
using UnityEngine;

namespace Celestial_Cross.Scripts.Units.Enemy.AI.BehaviorTree.Runtime
{
    public static class AIGridUtility
    {
        public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
        {
            return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
        }

        public static HashSet<Vector2Int> GetReachableTiles(Vector2Int origin, int range, GridMap gridMap = null)
        {
            if (gridMap == null) gridMap = GridMap.Instance;
            
            HashSet<Vector2Int> reachable = new HashSet<Vector2Int>();
            Queue<(Vector2Int pos, int cost)> queue = new Queue<(Vector2Int pos, int cost)>();
            HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

            Vector2Int[] dirs = {
                Vector2Int.up, Vector2Int.down,
                Vector2Int.left, Vector2Int.right
            };

            queue.Enqueue((origin, 0));
            visited.Add(origin);

            while (queue.Count > 0)
            {
                var (pos, cost) = queue.Dequeue();
                if (cost > range) continue;

                if (pos != origin) reachable.Add(pos);

                foreach (var d in dirs)
                {
                    Vector2Int next = pos + d;
                    if (visited.Contains(next)) continue;

                    GridTile nextTile = gridMap.GetTile(next);
                    if (nextTile == null || nextTile.IsOccupied) continue;

                    visited.Add(next);
                    queue.Enqueue((next, cost + 1));
                }
            }

            return reachable;
        }
    }
}
