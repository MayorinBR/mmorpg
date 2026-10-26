using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Records circular footprints on the XZ plane so props do not overlap each other.</summary>
    public sealed class Occupancy
    {
        private const float CellSize = 6f;
        private const int SearchCells = 3;

        private readonly Dictionary<Vector2Int, List<(Vector2 Center, float Radius)>> cells =
            new Dictionary<Vector2Int, List<(Vector2 Center, float Radius)>>();

        /// <summary>Checks whether a circle overlaps nothing that was recorded.</summary>
        /// <param name="center">Circle center.</param>
        /// <param name="radius">Circle radius.</param>
        /// <returns>True when the circle is free.</returns>
        public bool IsFree(Vector2 center, float radius)
        {
            Vector2Int cell = ToCell(center);
            for (int dz = -SearchCells; dz <= SearchCells; dz++)
            {
                for (int dx = -SearchCells; dx <= SearchCells; dx++)
                {
                    if (!cells.TryGetValue(new Vector2Int(cell.x + dx, cell.y + dz), out var entries))
                    {
                        continue;
                    }

                    foreach (var entry in entries)
                    {
                        float limit = entry.Radius + radius;
                        if ((entry.Center - center).sqrMagnitude < limit * limit)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>Records a circle as occupied.</summary>
        /// <param name="center">Circle center.</param>
        /// <param name="radius">Circle radius.</param>
        public void Add(Vector2 center, float radius)
        {
            Vector2Int cell = ToCell(center);
            if (!cells.TryGetValue(cell, out var entries))
            {
                entries = new List<(Vector2 Center, float Radius)>();
                cells[cell] = entries;
            }

            entries.Add((center, radius));
        }

        private static Vector2Int ToCell(Vector2 point)
        {
            return new Vector2Int(Mathf.FloorToInt(point.x / CellSize), Mathf.FloorToInt(point.y / CellSize));
        }
    }
}
