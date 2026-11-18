using System;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Downhill flow derived from a <see cref="WaterSurfaceGrid"/>: the direction is the steepest descent of the water
    /// surface, smoothed so the mesh stepping does not show, and the speed grows with the slope.
    /// </summary>
    public sealed class RiverFlowField
    {
        private const float FullStrengthSlope = 0.01f;
        private const float MinWeight = 1e-3f;
        private const float MinSlope = 1e-6f;

        private RiverFlowField(int size, Vector2[] flow, float[] speed)
        {
            Size = size;
            Flow = flow;
            Speed = speed;
        }

        /// <summary>Number of samples along each side.</summary>
        public int Size { get; }

        /// <summary>
        /// Unit direction of the flow in world X and Z, scaled from 0 to 1 by how clearly the water is moving.
        /// Level water such as lakes gets a zero vector.
        /// </summary>
        public Vector2[] Flow { get; }

        /// <summary>Normalized speed from 0 to 1, proportional to the slope of the water surface.</summary>
        public float[] Speed { get; }

        /// <summary>
        /// Computes the flow field of a water surface.
        /// </summary>
        /// <param name="grid">Water surface heights.</param>
        /// <param name="blurRadius">Smoothing radius in samples.</param>
        /// <param name="slopeForMaxSpeed">Slope in meters per meter that maps to the maximum speed.</param>
        /// <returns>The flow field, with the same layout as the grid.</returns>
        public static RiverFlowField Compute(WaterSurfaceGrid grid, int blurRadius, float slopeForMaxSpeed)
        {
            var size = grid.Size;
            var count = size * size;
            var slopeX = new float[count];
            var slopeZ = new float[count];
            var weight = new float[count];

            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    if (!grid.IsWater(col, row))
                    {
                        continue;
                    }

                    var index = row * size + col;
                    slopeX[index] = -Slope(grid, col, row, 1, 0);
                    slopeZ[index] = -Slope(grid, col, row, 0, 1);
                    weight[index] = 1f;
                }
            }

            var kernel = BuildKernel(blurRadius);
            Blur(slopeX, slopeZ, weight, size, kernel, true);
            Blur(slopeX, slopeZ, weight, size, kernel, false);

            var flow = new Vector2[count];
            var speed = new float[count];
            for (var i = 0; i < count; i++)
            {
                if (weight[i] < MinWeight)
                {
                    continue;
                }

                var gradient = new Vector2(slopeX[i], slopeZ[i]) / weight[i];
                var magnitude = gradient.magnitude;
                if (magnitude < MinSlope)
                {
                    continue;
                }

                flow[i] = gradient / magnitude * Mathf.Clamp01(magnitude / FullStrengthSlope);
                speed[i] = Mathf.Clamp01(magnitude / slopeForMaxSpeed);
            }

            return new RiverFlowField(size, flow, speed);
        }

        private static float Slope(WaterSurfaceGrid grid, int col, int row, int stepCol, int stepRow)
        {
            var hasBefore = grid.IsWater(col - stepCol, row - stepRow);
            var hasAfter = grid.IsWater(col + stepCol, row + stepRow);
            var here = grid.HeightAt(col, row);

            if (hasBefore && hasAfter)
            {
                return (grid.HeightAt(col + stepCol, row + stepRow) - grid.HeightAt(col - stepCol, row - stepRow)) / (2f * grid.CellSize);
            }

            if (hasAfter)
            {
                return (grid.HeightAt(col + stepCol, row + stepRow) - here) / grid.CellSize;
            }

            if (hasBefore)
            {
                return (here - grid.HeightAt(col - stepCol, row - stepRow)) / grid.CellSize;
            }

            return 0f;
        }

        private static float[] BuildKernel(int radius)
        {
            var kernel = new float[radius + 1];
            var sigma = Mathf.Max(radius * 0.5f, 0.5f);
            for (var k = 0; k <= radius; k++)
            {
                kernel[k] = Mathf.Exp(-k * k / (2f * sigma * sigma));
            }

            return kernel;
        }

        private static void Blur(float[] a, float[] b, float[] c, int size, float[] kernel, bool horizontal)
        {
            var radius = kernel.Length - 1;
            var resultA = new float[a.Length];
            var resultB = new float[a.Length];
            var resultC = new float[a.Length];

            for (var row = 0; row < size; row++)
            {
                for (var col = 0; col < size; col++)
                {
                    float sumA = 0f, sumB = 0f, sumC = 0f;
                    for (var k = -radius; k <= radius; k++)
                    {
                        var sampleCol = horizontal ? col + k : col;
                        var sampleRow = horizontal ? row : row + k;
                        if (sampleCol < 0 || sampleRow < 0 || sampleCol >= size || sampleRow >= size)
                        {
                            continue;
                        }

                        var j = sampleRow * size + sampleCol;
                        var w = kernel[Math.Abs(k)];
                        sumA += a[j] * w;
                        sumB += b[j] * w;
                        sumC += c[j] * w;
                    }

                    var i = row * size + col;
                    resultA[i] = sumA;
                    resultB[i] = sumB;
                    resultC[i] = sumC;
                }
            }

            Array.Copy(resultA, a, a.Length);
            Array.Copy(resultB, b, b.Length);
            Array.Copy(resultC, c, c.Length);
        }
    }
}
