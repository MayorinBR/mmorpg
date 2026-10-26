using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>
    /// A set of smooth trails sampled as points in a spatial grid so the distance to the nearest trail can be queried cheaply.
    /// </summary>
    public sealed class TrailNetwork
    {
        private const float CellSize = 4f;
        private const float SampleStep = 0.75f;
        private const float SearchLimit = 12f;

        private readonly Dictionary<Vector2Int, List<Vector2>> grid = new Dictionary<Vector2Int, List<Vector2>>();

        /// <summary>Initializes a new instance of the <see cref="TrailNetwork"/> class.</summary>
        /// <param name="paths">Waypoints of every trail.</param>
        public TrailNetwork(IEnumerable<Vector2[]> paths)
        {
            foreach (Vector2[] path in paths)
            {
                SamplePath(path);
            }
        }

        /// <summary>Gets the distance from a point to the nearest trail.</summary>
        /// <param name="point">Point on the XZ plane.</param>
        /// <returns>The distance in meters, capped at the search limit.</returns>
        public float DistanceTo(Vector2 point)
        {
            Vector2Int cell = ToCell(point);
            int reach = Mathf.CeilToInt(SearchLimit / CellSize);
            float best = SearchLimit * SearchLimit;
            for (int dz = -reach; dz <= reach; dz++)
            {
                for (int dx = -reach; dx <= reach; dx++)
                {
                    if (!grid.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dz), out List<Vector2> samples))
                    {
                        continue;
                    }

                    foreach (Vector2 sample in samples)
                    {
                        best = Mathf.Min(best, (sample - point).sqrMagnitude);
                    }
                }
            }

            return Mathf.Sqrt(best);
        }

        private void SamplePath(Vector2[] path)
        {
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 p0 = path[Mathf.Max(i - 1, 0)];
                Vector2 p1 = path[i];
                Vector2 p2 = path[i + 1];
                Vector2 p3 = path[Mathf.Min(i + 2, path.Length - 1)];
                int steps = Mathf.Max(2, Mathf.CeilToInt(Vector2.Distance(p1, p2) / SampleStep));
                for (int s = 0; s <= steps; s++)
                {
                    Add(CatmullRom(p0, p1, p2, p3, s / (float)steps));
                }
            }
        }

        private void Add(Vector2 point)
        {
            Vector2Int cell = ToCell(point);
            if (!grid.TryGetValue(cell, out List<Vector2> list))
            {
                list = new List<Vector2>();
                grid[cell] = list;
            }

            list.Add(point);
        }

        private static Vector2Int ToCell(Vector2 point)
        {
            return new Vector2Int(Mathf.FloorToInt(point.x / CellSize), Mathf.FloorToInt(point.y / CellSize));
        }

        private static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 0.5f * ((2f * p1) + ((p2 - p0) * t) + (((2f * p0) - (5f * p1) + (4f * p2) - p3) * t2) + (((3f * p1) - p0 - (3f * p2) + p3) * t3));
        }
    }
}
