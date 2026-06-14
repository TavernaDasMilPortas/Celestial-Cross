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

                    if (!gridMap.TileExists(next)) continue;
                    GridTile nextTile = gridMap.GetTile(next);
                    if (nextTile == null || nextTile.IsOccupied) continue;

                    visited.Add(next);
                    queue.Enqueue((next, cost + 1));
                }
            }

            return reachable;
        }

        public static Direction GetDirection(Vector2Int from, Vector2Int to)
        {
            Vector2 dir = new Vector2(to.x - from.x, to.y - from.y).normalized;
            if (dir == Vector2.zero) return Direction.N;

            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            if (angle >= 337.5f || angle < 22.5f) return Direction.E;
            if (angle >= 22.5f && angle < 67.5f) return Direction.NE;
            if (angle >= 67.5f && angle < 112.5f) return Direction.N;
            if (angle >= 112.5f && angle < 157.5f) return Direction.NW;
            if (angle >= 157.5f && angle < 202.5f) return Direction.W;
            if (angle >= 202.5f && angle < 247.5f) return Direction.SW;
            if (angle >= 247.5f && angle < 292.5f) return Direction.S;
            if (angle >= 292.5f && angle < 337.5f) return Direction.SE;

            return Direction.N;
        }

        public static (int validHits, int friendlyHits, HashSet<Vector2Int> hitPositions) EvaluateAoE(
            Vector2Int casterPos, Vector2Int targetPos, AreaPatternData pattern, 
            IEnumerable<Unit> validTargets, IEnumerable<Unit> friendlyUnits)
        {
            var dir = GetDirection(casterPos, targetPos);
            var cells = AreaResolver.ResolveCells(targetPos, pattern, dir);
            var hitPositions = new HashSet<Vector2Int>(cells);

            int validHits = 0;
            int friendlyHits = 0;

            foreach (var t in validTargets)
            {
                if (hitPositions.Contains(t.GridPosition)) validHits++;
            }

            foreach (var f in friendlyUnits)
            {
                if (hitPositions.Contains(f.GridPosition)) friendlyHits++;
            }

            return (validHits, friendlyHits, hitPositions);
        }
    }
}
