using NUnit.Framework;
using UnityEngine;

namespace Project.World.Tests
{
    /// <summary>
    /// Tests <see cref="NavMeshTrianglePicker"/>, which chooses the random spot Teleport sends the player to.
    /// </summary>
    public class NavMeshTrianglePickerTests
    {
        private static readonly Vector3[] Square =
        {
            new Vector3(0f, 0f, 0f), new Vector3(100f, 0f, 0f), new Vector3(100f, 0f, 100f), new Vector3(0f, 0f, 100f)
        };

        private static readonly int[] SquareIndices = { 0, 2, 1, 0, 3, 2 };

        /// <summary>An empty mesh has nowhere to teleport to.</summary>
        [Test]
        public void TryPick_EmptyMesh_ReturnsFalse()
        {
            var picked = NavMeshTrianglePicker.TryPick(new Vector3[0], new int[0], Vector3.zero, 8f, () => 0.5f, out _);

            Assert.IsFalse(picked);
        }

        /// <summary>Every picked point lies on the mesh surface.</summary>
        [Test]
        public void TryPick_PointStaysInsideMesh()
        {
            var random = new System.Random(7);

            for (var i = 0; i < 200; i++)
            {
                NavMeshTrianglePicker.TryPick(Square, SquareIndices, Vector3.zero, 0f, () => (float)random.NextDouble(), out var point);

                Assert.That(point.x, Is.InRange(0f, 100f));
                Assert.That(point.z, Is.InRange(0f, 100f));
            }
        }

        /// <summary>On a large mesh the point is moved away from where the player stands.</summary>
        [Test]
        public void TryPick_LargeMesh_RespectsMinimumDistance()
        {
            var random = new System.Random(11);

            for (var i = 0; i < 200; i++)
            {
                NavMeshTrianglePicker.TryPick(Square, SquareIndices, new Vector3(50f, 0f, 50f), 8f, () => (float)random.NextDouble(), out var point);

                var distance = new Vector2(point.x - 50f, point.z - 50f).magnitude;
                Assert.That(distance, Is.GreaterThanOrEqualTo(8f));
            }
        }

        /// <summary>A mesh smaller than the minimum distance still yields a point instead of failing.</summary>
        [Test]
        public void TryPick_TinyMesh_StillReturnsPoint()
        {
            var vertices = new[] { Vector3.zero, new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 1f) };

            var picked = NavMeshTrianglePicker.TryPick(vertices, new[] { 0, 1, 2 }, Vector3.zero, 8f, () => 0.3f, out _);

            Assert.IsTrue(picked);
        }
    }
}
