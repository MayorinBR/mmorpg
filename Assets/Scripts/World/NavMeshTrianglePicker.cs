using System;
using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Picks a random point on a triangle mesh, weighting each triangle by its area so every part of the
    /// surface is equally likely. Used to find a random walkable spot from the NavMesh triangulation.
    /// </summary>
    public static class NavMeshTrianglePicker
    {
        private const int MaxAttempts = 24;

        /// <summary>
        /// Picks a random point on the mesh that is at least <paramref name="minDistance"/> away from the origin
        /// on the horizontal plane. When no attempt satisfies the distance, the farthest candidate is returned,
        /// so a small mesh still yields a point.
        /// </summary>
        /// <param name="vertices">The mesh vertices.</param>
        /// <param name="indices">Three vertex indices per triangle.</param>
        /// <param name="origin">The position the point should be away from.</param>
        /// <param name="minDistance">The preferred minimum horizontal distance from the origin.</param>
        /// <param name="random01">Returns a random value in [0, 1).</param>
        /// <param name="point">The picked point.</param>
        /// <returns>True if the mesh has at least one triangle.</returns>
        public static bool TryPick(Vector3[] vertices, int[] indices, Vector3 origin, float minDistance, Func<float> random01, out Vector3 point)
        {
            point = default;
            var triangleCount = indices.Length / 3;

            if (triangleCount == 0)
            {
                return false;
            }

            var cumulativeArea = BuildCumulativeAreas(vertices, indices, triangleCount);
            var total = cumulativeArea[triangleCount - 1];
            var bestDistance = -1f;

            for (var attempt = 0; attempt < MaxAttempts; attempt++)
            {
                var candidate = SamplePoint(vertices, indices, cumulativeArea, total, random01);
                var distance = HorizontalDistance(candidate, origin);

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    point = candidate;
                }

                if (distance >= minDistance)
                {
                    return true;
                }
            }

            return true;
        }

        private static float[] BuildCumulativeAreas(Vector3[] vertices, int[] indices, int triangleCount)
        {
            var cumulative = new float[triangleCount];
            var running = 0f;

            for (var i = 0; i < triangleCount; i++)
            {
                var a = vertices[indices[i * 3]];
                var b = vertices[indices[i * 3 + 1]];
                var c = vertices[indices[i * 3 + 2]];
                running += 0.5f * Vector3.Cross(b - a, c - a).magnitude;
                cumulative[i] = running;
            }

            return cumulative;
        }

        private static Vector3 SamplePoint(Vector3[] vertices, int[] indices, float[] cumulativeArea, float total, Func<float> random01)
        {
            var target = random01() * total;
            var triangle = Array.FindIndex(cumulativeArea, area => area >= target);
            triangle = triangle < 0 ? cumulativeArea.Length - 1 : triangle;

            var a = vertices[indices[triangle * 3]];
            var b = vertices[indices[triangle * 3 + 1]];
            var c = vertices[indices[triangle * 3 + 2]];

            var u = random01();
            var v = random01();

            if (u + v > 1f)
            {
                u = 1f - u;
                v = 1f - v;
            }

            return a + u * (b - a) + v * (c - a);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            var dx = a.x - b.x;
            var dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
