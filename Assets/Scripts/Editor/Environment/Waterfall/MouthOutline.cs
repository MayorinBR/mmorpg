using System;
using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Irregular outline of the waterfall mouth in the plane of the wall, in world X and height.
    /// The bottom of the outline is flattened at the sill so water can flow out over a level floor.
    /// </summary>
    public sealed class MouthOutline
    {
        private const int PointCount = 32;
        private const float FloorNoiseScale = 0.35f;
        private const float FloorExtension = 1.4f;

        private MouthOutline(Vector2[] points, bool[] onSill, Vector2 center)
        {
            Points = points;
            OnSill = onSill;
            Center = center;
        }

        /// <summary>Outline points in counterclockwise order, starting at the bottom.</summary>
        public Vector2[] Points { get; }

        /// <summary>For each point, whether it lies on the flat sill.</summary>
        public bool[] OnSill { get; }

        /// <summary>Center of the opening.</summary>
        public Vector2 Center { get; }

        /// <summary>Width of the flat sill, in meters.</summary>
        public float SillWidth
        {
            get
            {
                var min = float.MaxValue;
                var max = float.MinValue;
                for (var i = 0; i < Points.Length; i++)
                {
                    if (!OnSill[i])
                    {
                        continue;
                    }

                    min = Mathf.Min(min, Points[i].x);
                    max = Mathf.Max(max, Points[i].x);
                }

                return max > min ? max - min : 0f;
            }
        }

        /// <summary>
        /// Creates the outline of a profile. The same seed always yields the same shape.
        /// </summary>
        /// <param name="profile">Profile that sets the size, position and seed.</param>
        /// <returns>The outline.</returns>
        public static MouthOutline Create(WaterfallProfile profile)
        {
            var random = new System.Random(profile.Seed);
            var phaseA = (float)(random.NextDouble() * Math.PI * 2.0);
            var phaseB = (float)(random.NextDouble() * Math.PI * 2.0);
            var phaseC = (float)(random.NextDouble() * Math.PI * 2.0);

            var height = profile.TopHeight - profile.SillHeight;
            var centerY = profile.SillHeight + height * 0.5f;
            var radiusUp = profile.TopHeight - centerY;
            var radiusDown = (centerY - profile.SillHeight) * FloorExtension;

            var points = new Vector2[PointCount];
            var onSill = new bool[PointCount];
            for (var i = 0; i < PointCount; i++)
            {
                var angle = -Mathf.PI * 0.5f + Mathf.PI * 2f * i / PointCount;
                var sin = Mathf.Sin(angle);
                var amplitude = sin < -0.4f ? FloorNoiseScale : 1f;
                var factor = 1f
                    + amplitude * (0.15f * Mathf.Sin(2f * angle + phaseA)
                    + 0.10f * Mathf.Sin(3f * angle + phaseB)
                    + 0.06f * Mathf.Sin(5f * angle + phaseC)
                    + ((float)random.NextDouble() - 0.5f) * 0.12f);

                var x = profile.CenterX + profile.MouthHalfWidth * factor * Mathf.Cos(angle);
                var y = centerY + (sin >= 0f ? radiusUp : radiusDown) * factor * sin;
                onSill[i] = y <= profile.SillHeight;
                points[i] = new Vector2(x, Mathf.Clamp(y, profile.SillHeight, profile.TopHeight));
            }

            return new MouthOutline(points, onSill, new Vector2(profile.CenterX, centerY));
        }

        /// <summary>Tells whether a point of the wall plane lies inside the outline.</summary>
        /// <param name="point">World X and height.</param>
        /// <returns>True when the point is inside.</returns>
        public bool Contains(Vector2 point)
        {
            var inside = false;
            for (int i = 0, j = Points.Length - 1; i < Points.Length; j = i++)
            {
                var a = Points[i];
                var b = Points[j];
                if ((a.y > point.y) != (b.y > point.y) && point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
