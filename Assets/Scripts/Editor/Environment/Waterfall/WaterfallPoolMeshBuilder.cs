using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Builds the water that fills the low ground at the foot of the fall. The river mesh stops short of the wall, so any
    /// part of the terrain that lies below the water level next to the landing point would stay dry. This builder floods
    /// that ground from the river outwards and returns a flat surface that joins the river mesh without a gap.
    /// </summary>
    public static class WaterfallPoolMeshBuilder
    {
        private const float BankTolerance = 0.3f;

        /// <summary>
        /// Builds the pool surface in world space.
        /// </summary>
        /// <param name="terrain">Terrain whose low ground is filled.</param>
        /// <param name="river">Water surface of the river, used as the seed and for stitching the edge.</param>
        /// <param name="seed">World X and Z of a point that is over the river water.</param>
        /// <param name="level">Height of the pool surface, in meters.</param>
        /// <param name="region">World rectangle (X, Z) that limits the flood, so it cannot spill along the river banks.</param>
        /// <returns>The pool mesh, or an empty mesh when no dry low ground touches the river.</returns>
        public static Mesh Build(Terrain terrain, WaterSurfaceGrid river, Vector2 seed, float level, Rect region)
        {
            var size = river.Size;
            var cell = river.CellSize;
            var origin = river.Origin;
            var seedCol = Mathf.RoundToInt((seed.x - origin.x) / cell);
            var seedRow = Mathf.RoundToInt((seed.y - origin.y) / cell);
            var flooded = new HashSet<long>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(new Vector2Int(seedCol, seedRow));
            flooded.Add(Key(seedCol, seedRow, size));

            var offsets = new[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var offset in offsets)
                {
                    var next = current + offset;
                    if (!IsInside(next, size) || !region.Contains(new Vector2(origin.x + next.x * cell, origin.y + next.y * cell)))
                    {
                        continue;
                    }

                    var key = Key(next.x, next.y, size);
                    if (flooded.Contains(key) || !IsFillable(terrain, river, next, origin, cell, level))
                    {
                        continue;
                    }

                    flooded.Add(key);
                    queue.Enqueue(next);
                }
            }

            return BuildSurface(river, flooded, origin, cell, level);
        }

        private static Mesh BuildSurface(WaterSurfaceGrid river, HashSet<long> flooded, Vector2 origin, float cell, float level)
        {
            var size = river.Size;
            var vertices = new List<Vector3>();
            var indices = new Dictionary<long, int>();
            var triangles = new List<int>();

            foreach (var key in flooded)
            {
                var col = (int)(key % size);
                var row = (int)(key / size);
                for (var dc = -1; dc <= 0; dc++)
                {
                    for (var dr = -1; dr <= 0; dr++)
                    {
                        TryAddQuad(river, flooded, col + dc, row + dr, origin, cell, level, vertices, indices, triangles);
                    }
                }
            }

            var mesh = new Mesh { name = "Waterfall_Pool" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void TryAddQuad(WaterSurfaceGrid river, HashSet<long> flooded, int col, int row, Vector2 origin, float cell, float level,
            List<Vector3> vertices, Dictionary<long, int> indices, List<int> triangles)
        {
            var size = river.Size;
            var corners = new[] { new Vector2Int(col, row), new Vector2Int(col + 1, row), new Vector2Int(col + 1, row + 1), new Vector2Int(col, row + 1) };
            var hasDryCorner = false;
            foreach (var corner in corners)
            {
                if (!IsInside(corner, size) || !(flooded.Contains(Key(corner.x, corner.y, size)) || river.IsWater(corner.x, corner.y)))
                {
                    return;
                }

                hasDryCorner |= !river.IsWater(corner.x, corner.y);
            }

            if (!hasDryCorner)
            {
                return;
            }

            var quadKey = Key(col, row, size) + (long)size * size;
            if (indices.ContainsKey(quadKey))
            {
                return;
            }

            indices[quadKey] = 0;
            var first = new int[4];
            for (var i = 0; i < 4; i++)
            {
                first[i] = VertexIndex(river, corners[i], origin, cell, level, vertices, indices);
            }

            triangles.AddRange(new[] { first[0], first[2], first[1], first[0], first[3], first[2] });
        }

        private static int VertexIndex(WaterSurfaceGrid river, Vector2Int corner, Vector2 origin, float cell, float level, List<Vector3> vertices, Dictionary<long, int> indices)
        {
            var key = Key(corner.x, corner.y, river.Size);
            if (indices.TryGetValue(key, out var index))
            {
                return index;
            }

            var height = river.IsWater(corner.x, corner.y) ? river.HeightAt(corner.x, corner.y) : level;
            index = vertices.Count;
            indices[key] = index;
            vertices.Add(new Vector3(origin.x + corner.x * cell, height + 0.01f, origin.y + corner.y * cell));
            return index;
        }

        private static bool IsFillable(Terrain terrain, WaterSurfaceGrid river, Vector2Int point, Vector2 origin, float cell, float level)
        {
            if (river.IsWater(point.x, point.y))
            {
                return true;
            }

            var world = new Vector3(origin.x + point.x * cell, 0f, origin.y + point.y * cell);
            var height = terrain.SampleHeight(world) + terrain.transform.position.y;
            return height < level + BankTolerance;
        }

        private static bool IsInside(Vector2Int point, int size)
        {
            return point.x >= 0 && point.y >= 0 && point.x < size && point.y < size;
        }

        private static long Key(int col, int row, int size)
        {
            return (long)row * size + col;
        }
    }
}
