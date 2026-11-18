using System;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Regular lattice of water surface heights extracted from a river mesh whose vertices form a square grid.
    /// Only vertices referenced by at least one triangle count as water.
    /// </summary>
    public sealed class WaterSurfaceGrid
    {
        private readonly float[] heights;
        private readonly bool[] water;

        /// <summary>
        /// Creates a grid from precomputed samples stored row by row, with the column index running fastest.
        /// </summary>
        /// <param name="size">Number of samples along each side.</param>
        /// <param name="cellSize">Distance between neighbouring samples, in meters.</param>
        /// <param name="origin">World X and Z of the first sample.</param>
        /// <param name="heights">World height of every sample.</param>
        /// <param name="water">Whether each sample belongs to the water surface.</param>
        public WaterSurfaceGrid(int size, float cellSize, Vector2 origin, float[] heights, bool[] water)
        {
            Size = size;
            CellSize = cellSize;
            Origin = origin;
            this.heights = heights;
            this.water = water;
        }

        /// <summary>Number of samples along each side.</summary>
        public int Size { get; }

        /// <summary>Distance between neighbouring samples, in meters.</summary>
        public float CellSize { get; }

        /// <summary>World X and Z of the first sample.</summary>
        public Vector2 Origin { get; }

        /// <summary>
        /// Builds a grid from a river mesh laid out as a square lattice.
        /// </summary>
        /// <param name="mesh">Mesh with size x size vertices ordered row by row.</param>
        /// <param name="transform">Transform that places the mesh in the world.</param>
        /// <returns>The sampled grid.</returns>
        /// <exception cref="ArgumentException">The vertex count is not a perfect square.</exception>
        public static WaterSurfaceGrid FromMesh(Mesh mesh, Transform transform)
        {
            var vertices = mesh.vertices;
            var size = Mathf.RoundToInt(Mathf.Sqrt(vertices.Length));
            if (size * size != vertices.Length)
            {
                throw new ArgumentException($"Mesh '{mesh.name}' is not a square lattice ({vertices.Length} vertices).");
            }

            var water = new bool[vertices.Length];
            foreach (var index in mesh.triangles)
            {
                water[index] = true;
            }

            var heights = new float[vertices.Length];
            for (var i = 0; i < vertices.Length; i++)
            {
                heights[i] = transform.TransformPoint(vertices[i]).y;
            }

            var first = transform.TransformPoint(vertices[0]);
            var second = transform.TransformPoint(vertices[1]);
            return new WaterSurfaceGrid(size, second.x - first.x, new Vector2(first.x, first.z), heights, water);
        }

        /// <summary>Tells whether the sample at the given lattice position is water.</summary>
        /// <param name="col">Column of the sample.</param>
        /// <param name="row">Row of the sample.</param>
        /// <returns>True when the position is inside the grid and holds water.</returns>
        public bool IsWater(int col, int row)
        {
            return col >= 0 && row >= 0 && col < Size && row < Size && water[row * Size + col];
        }

        /// <summary>Returns the world height of the sample at the given lattice position.</summary>
        /// <param name="col">Column of the sample.</param>
        /// <param name="row">Row of the sample.</param>
        /// <returns>The stored height.</returns>
        public float HeightAt(int col, int row)
        {
            return heights[row * Size + col];
        }

        /// <summary>
        /// Interpolates the water surface height at a world position, using only the water samples around it.
        /// </summary>
        /// <param name="x">World X.</param>
        /// <param name="z">World Z.</param>
        /// <param name="height">The interpolated world height.</param>
        /// <returns>False when no water sample is close enough.</returns>
        public bool TryGetHeight(float x, float z, out float height)
        {
            var gx = (x - Origin.x) / CellSize;
            var gz = (z - Origin.y) / CellSize;
            var col = Mathf.FloorToInt(gx);
            var row = Mathf.FloorToInt(gz);
            var fx = gx - col;
            var fz = gz - row;

            var sum = 0f;
            var weight = 0f;
            for (var dr = 0; dr <= 1; dr++)
            {
                for (var dc = 0; dc <= 1; dc++)
                {
                    if (!IsWater(col + dc, row + dr))
                    {
                        continue;
                    }

                    var w = (dc == 0 ? 1f - fx : fx) * (dr == 0 ? 1f - fz : fz);
                    sum += w * HeightAt(col + dc, row + dr);
                    weight += w;
                }
            }

            height = weight > 1e-4f ? sum / weight : 0f;
            return weight > 1e-4f;
        }
    }
}
