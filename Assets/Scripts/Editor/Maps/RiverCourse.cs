using System.Collections.Generic;
using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>A round pond that closes one end of a river.</summary>
    public readonly struct Pond
    {
        /// <summary>Initializes a new instance of the <see cref="Pond"/> struct.</summary>
        /// <param name="center">Pond center on the XZ plane.</param>
        /// <param name="radius">Radius of the water surface.</param>
        public Pond(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>Gets the pond center.</summary>
        public Vector2 Center { get; }

        /// <summary>Gets the radius of the water surface.</summary>
        public float Radius { get; }
    }

    /// <summary>
    /// A river described by a center line and optional ponds. It tells how far a point is from the water edge and how much
    /// the terrain must be carved there, so the terrain, the vegetation and the water volumes agree.
    /// </summary>
    public sealed class RiverCourse
    {
        private const float WaterlineOffset = 2.9f;
        private const float BankReach = 4.5f;
        private const float WidthVariation = 0.45f;

        private readonly TrailNetwork centerLine;
        private readonly IReadOnlyList<Pond> ponds;
        private readonly float halfWidth;

        /// <summary>Initializes a new instance of the <see cref="RiverCourse"/> class.</summary>
        /// <param name="path">Waypoints of the river center line.</param>
        /// <param name="ponds">Ponds connected to the river.</param>
        /// <param name="halfWidth">Half width of the river bed, before the banks.</param>
        public RiverCourse(Vector2[] path, IReadOnlyList<Pond> ponds, float halfWidth)
        {
            centerLine = new TrailNetwork(new List<Vector2[]> { path });
            this.ponds = ponds;
            this.halfWidth = halfWidth;
        }

        /// <summary>Gets the distance from the water edge to a point, negative when the point is under water.</summary>
        /// <param name="point">Point on the XZ plane.</param>
        /// <returns>Signed distance in meters.</returns>
        public float EdgeDistance(Vector2 point)
        {
            float best = centerLine.DistanceTo(point) - (HalfWidthAt(point) + WaterlineOffset);
            foreach (Pond pond in ponds)
            {
                best = Mathf.Min(best, Vector2.Distance(point, pond.Center) - pond.Radius);
            }

            return best;
        }

        /// <summary>Gets how much the terrain is lowered toward the river bed at a point.</summary>
        /// <param name="point">Point on the XZ plane.</param>
        /// <returns>Weight between 0 (untouched) and 1 (river bed).</returns>
        public float Carve(Vector2 point)
        {
            float best = Profile(centerLine.DistanceTo(point), HalfWidthAt(point));
            foreach (Pond pond in ponds)
            {
                best = Mathf.Max(best, Profile(Vector2.Distance(point, pond.Center), pond.Radius - WaterlineOffset));
            }

            return best;
        }

        private static float Profile(float distance, float width)
        {
            return MapMath.SmoothStep(Mathf.InverseLerp(width + BankReach, width - 1f, distance));
        }

        private float HalfWidthAt(Vector2 point)
        {
            return halfWidth * (1f + (WidthVariation * MapMath.Noise(point.x * 0.05f, point.y * 0.05f)));
        }
    }
}
