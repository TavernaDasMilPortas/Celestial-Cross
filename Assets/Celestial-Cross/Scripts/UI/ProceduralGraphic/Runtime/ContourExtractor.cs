using System.Collections.Generic;
using UnityEngine;

namespace CelestialCross.UI.ProceduralGraphic
{
    public static class ContourExtractor
    {
        // Direction vectors for Moore neighborhood (clockwise)
        private static readonly int[] dx = { 0, 1, 1, 1, 0, -1, -1, -1 };
        private static readonly int[] dy = { -1, -1, 0, 1, 1, 1, 0, -1 };

        public static List<Vector2> ExtractContour(Texture2D tex, float alphaThreshold, int closeGapsRadius = 0)
        {
            int width = tex.width;
            int height = tex.height;
            Color[] pixels = tex.GetPixels();

            bool[,] solidGrid = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    solidGrid[x, y] = pixels[y * width + x].a >= alphaThreshold;
                }
            }

            if (closeGapsRadius > 0)
            {
                solidGrid = ApplyMorphologicalClosing(solidGrid, width, height, closeGapsRadius);
            }

            bool IsSolid(int x, int y)
            {
                if (x < 0 || x >= width || y < 0 || y >= height) return false;
                return solidGrid[x, y];
            }

            // Find a starting point (first solid pixel from bottom-left)
            int startX = -1, startY = -1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsSolid(x, y))
                    {
                        startX = x;
                        startY = y;
                        break;
                    }
                }
                if (startX != -1) break;
            }

            if (startX == -1) return new List<Vector2>(); // Completely transparent image

            List<Vector2> contour = new List<Vector2>();
            
            // Moore Neighborhood tracing
            int cx = startX;
            int cy = startY;
            int prevDir = 4; // Start looking upwards

            int failSafe = width * height; // Prevent infinite loop

            do
            {
                contour.Add(new Vector2(cx, cy));
                int dir = (prevDir + 2) % 8; // Look to the "left" of previous direction
                bool foundNext = false;

                for (int i = 0; i < 8; i++)
                {
                    int nx = cx + dx[dir];
                    int ny = cy + dy[dir];

                    if (IsSolid(nx, ny))
                    {
                        cx = nx;
                        cy = ny;
                        prevDir = (dir + 4) % 8; // Opposite direction
                        foundNext = true;
                        break;
                    }
                    dir = (dir + 1) % 8;
                }

                if (!foundNext) break; // Isolated pixel
                failSafe--;

            } while ((cx != startX || cy != startY) && failSafe > 0);

            return contour;
        }

        private static bool[,] ApplyMorphologicalClosing(bool[,] grid, int width, int height, int radius)
        {
            // 1. Dilation (Expande os pixels sólidos)
            bool[,] dilated = new bool[width, height];
            int r2 = radius * radius;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (grid[x, y])
                    {
                        // Se for sólido, pinta um circulo ao redor no novo grid
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                if (dx * dx + dy * dy <= r2)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                    {
                                        dilated[nx, ny] = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // 2. Erosion (Encolhe os pixels sólidos de volta)
            bool[,] eroded = new bool[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (dilated[x, y])
                    {
                        bool keep = true;
                        // Para manter um pixel, TODOS os vizinhos no raio precisam ser sólidos no dilated
                        for (int dy = -radius; dy <= radius; dy++)
                        {
                            for (int dx = -radius; dx <= radius; dx++)
                            {
                                if (dx * dx + dy * dy <= r2)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;
                                    if (nx < 0 || nx >= width || ny < 0 || ny >= height || !dilated[nx, ny])
                                    {
                                        keep = false;
                                        break;
                                    }
                                }
                            }
                            if (!keep) break;
                        }
                        eroded[x, y] = keep;
                    }
                }
            }

            return eroded;
        }

        public static List<Vector2> SimplifyContour(List<Vector2> points, int targetCount)
        {
            if (points.Count <= targetCount) return new List<Vector2>(points);

            float minTolerance = 0f;
            float maxTolerance = 100f; // Max pixel distance
            List<Vector2> bestResult = new List<Vector2>(points);

            // Binary search for tolerance
            for (int i = 0; i < 20; i++)
            {
                float midTolerance = (minTolerance + maxTolerance) / 2f;
                List<Vector2> currentResult = DouglasPeucker(points, midTolerance);

                if (Mathf.Abs(currentResult.Count - targetCount) < Mathf.Abs(bestResult.Count - targetCount))
                {
                    bestResult = currentResult;
                }

                if (currentResult.Count > targetCount)
                {
                    minTolerance = midTolerance;
                }
                else if (currentResult.Count < targetCount)
                {
                    maxTolerance = midTolerance;
                }
                else
                {
                    break; // Exact match found
                }
            }

            return bestResult;
        }

        private static List<Vector2> DouglasPeucker(List<Vector2> points, float tolerance)
        {
            if (points == null || points.Count < 3)
                return points;

            int firstPoint = 0;
            int lastPoint = points.Count - 1;
            List<int> pointIndexsToKeep = new List<int> { firstPoint, lastPoint };

            while (points[firstPoint] == points[lastPoint])
            {
                lastPoint--;
            }

            DouglasPeuckerReduction(points, firstPoint, lastPoint, tolerance, ref pointIndexsToKeep);

            pointIndexsToKeep.Sort();
            List<Vector2> returnPoints = new List<Vector2>();

            foreach (int index in pointIndexsToKeep)
            {
                returnPoints.Add(points[index]);
            }

            return returnPoints;
        }

        private static void DouglasPeuckerReduction(List<Vector2> points, int firstPoint, int lastPoint, float tolerance, ref List<int> pointIndexsToKeep)
        {
            float maxDistance = 0;
            int indexFarthest = 0;

            for (int index = firstPoint; index < lastPoint; index++)
            {
                float distance = PerpendicularDistance(points[firstPoint], points[lastPoint], points[index]);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    indexFarthest = index;
                }
            }

            if (maxDistance > tolerance && indexFarthest != 0)
            {
                pointIndexsToKeep.Add(indexFarthest);
                DouglasPeuckerReduction(points, firstPoint, indexFarthest, tolerance, ref pointIndexsToKeep);
                DouglasPeuckerReduction(points, indexFarthest, lastPoint, tolerance, ref pointIndexsToKeep);
            }
        }

        private static float PerpendicularDistance(Vector2 Point1, Vector2 Point2, Vector2 Point)
        {
            float area = Mathf.Abs(.5f * (Point1.x * Point2.y + Point2.x * Point.y + Point.x * Point1.y - Point2.x * Point1.y - Point.x * Point2.y - Point1.x * Point.y));
            float bottom = Mathf.Sqrt(Mathf.Pow(Point1.x - Point2.x, 2) + Mathf.Pow(Point1.y - Point2.y, 2));
            if (bottom == 0) return 0f;
            return area / bottom * 2f;
        }

        public static List<Vector2> NormalizeContour(List<Vector2> points, int texWidth, int texHeight)
        {
            if (points.Count == 0) return new List<Vector2>();

            List<Vector2> normalized = new List<Vector2>();
            foreach (var p in points)
            {
                float nx = p.x / (float)texWidth;
                float ny = p.y / (float)texHeight;
                normalized.Add(new Vector2(nx, ny));
            }

            return normalized;
        }

        public static List<Vector2> ExpandContour(List<Vector2> points, float expansion)
        {
            if (points.Count < 3 || expansion == 0) return new List<Vector2>(points);

            List<Vector2> expanded = new List<Vector2>(points.Count);
            int count = points.Count;

            for (int i = 0; i < count; i++)
            {
                Vector2 prev = points[(i - 1 + count) % count];
                Vector2 curr = points[i];
                Vector2 next = points[(i + 1) % count];

                Vector2 dir1 = (curr - prev).normalized;
                Vector2 dir2 = (next - curr).normalized;

                Vector2 normal1 = new Vector2(dir1.y, -dir1.x); // Right normal (Outward for CCW tracing)
                Vector2 normal2 = new Vector2(dir2.y, -dir2.x);

                Vector2 averageNormal = (normal1 + normal2).normalized;

                // Dot product to adjust expansion based on angle to avoid pinching
                float dot = Vector2.Dot(normal1, averageNormal);
                float adjustedExpansion = expansion;
                if (dot > 0.1f) // Avoid division by zero or extreme expansions
                {
                    adjustedExpansion = expansion / dot;
                }

                // Clamp extreme expansions
                adjustedExpansion = Mathf.Clamp(adjustedExpansion, -Mathf.Abs(expansion) * 3f, Mathf.Abs(expansion) * 3f);

                expanded.Add(curr + averageNormal * adjustedExpansion);
            }

            return expanded;
        }

        public static List<Vector2> ApplyJitter(List<Vector2> points, float amount, int seed)
        {
            if (amount <= 0) return new List<Vector2>(points);

            Random.State oldState = Random.state;
            Random.InitState(seed);

            List<Vector2> jittered = new List<Vector2>(points.Count);
            foreach (var p in points)
            {
                Vector2 offset = Random.insideUnitCircle * amount;
                jittered.Add(p + offset);
            }

            Random.state = oldState;
            return jittered;
        }
    }
}
