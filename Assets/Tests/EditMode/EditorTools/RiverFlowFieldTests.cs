using NUnit.Framework;
using UnityEngine;

namespace Project.EditorTools.Tests
{
    /// <summary>
    /// Tests <see cref="RiverFlowField"/>, which derives the direction and speed of a river from its water surface.
    /// </summary>
    public class RiverFlowFieldTests
    {
        private const int Size = 32;
        private const float CellSize = 1f;

        private static WaterSurfaceGrid CreateGrid(System.Func<int, int, float> height, bool allWater = true)
        {
            var heights = new float[Size * Size];
            var water = new bool[Size * Size];
            for (var row = 0; row < Size; row++)
            {
                for (var col = 0; col < Size; col++)
                {
                    heights[row * Size + col] = height(col, row);
                    water[row * Size + col] = allWater;
                }
            }

            return new WaterSurfaceGrid(Size, CellSize, Vector2.zero, heights, water);
        }

        /// <summary>Water that descends toward +X flows along +X.</summary>
        [Test]
        public void Compute_SlopeDownTowardPositiveX_FlowsAlongPositiveX()
        {
            var grid = CreateGrid((col, row) => 10f - 0.05f * col);

            var field = RiverFlowField.Compute(grid, 3, 0.12f);

            var center = field.Flow[16 * Size + 16];
            Assert.That(center.x, Is.GreaterThan(0.95f));
            Assert.That(center.y, Is.InRange(-0.05f, 0.05f));
        }

        /// <summary>The speed is the slope divided by the slope that maps to full speed.</summary>
        [Test]
        public void Compute_Slope_ScalesSpeed()
        {
            var grid = CreateGrid((col, row) => 10f - 0.06f * row);

            var field = RiverFlowField.Compute(grid, 3, 0.12f);

            Assert.That(field.Speed[16 * Size + 16], Is.EqualTo(0.5f).Within(0.02f));
        }

        /// <summary>Level water such as a lake does not flow.</summary>
        [Test]
        public void Compute_LevelWater_HasNoFlow()
        {
            var grid = CreateGrid((col, row) => 5f);

            var field = RiverFlowField.Compute(grid, 3, 0.12f);

            Assert.That(field.Flow[16 * Size + 16], Is.EqualTo(Vector2.zero));
            Assert.That(field.Speed[16 * Size + 16], Is.EqualTo(0f));
        }

        /// <summary>A grid without water produces an empty field.</summary>
        [Test]
        public void Compute_NoWater_HasNoFlow()
        {
            var grid = CreateGrid((col, row) => 10f - 0.05f * col, false);

            var field = RiverFlowField.Compute(grid, 3, 0.12f);

            Assert.That(field.Flow[16 * Size + 16], Is.EqualTo(Vector2.zero));
        }
    }
}
