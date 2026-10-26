using UnityEngine;

namespace Project.EditorTools.Maps
{
    /// <summary>Dimensions shared by the 160 m prototype maps and small math helpers used by their terrain shapes.</summary>
    public static class MapMath
    {
        /// <summary>Side length of the standard prototype maps in meters.</summary>
        public const float DefaultSize = 160f;

        /// <summary>Gets the side length of the map being built, in meters.</summary>
        public static float Size { get; private set; } = DefaultSize;

        /// <summary>Gets half of the side length of the map being built, in meters.</summary>
        public static float Half => Size * 0.5f;

        /// <summary>Selects the side length used by all map builders until it is changed again.</summary>
        /// <param name="size">Side length in meters.</param>
        public static void Use(float size)
        {
            Size = size;
        }

        private static readonly Vector2 NoiseOffset = new Vector2(137.5f, 59.3f);

        /// <summary>Gets the distance from a point to the nearest map edge.</summary>
        /// <param name="point">Point on the XZ plane.</param>
        /// <returns>Distance in meters.</returns>
        public static float EdgeDistance(Vector2 point)
        {
            return Half - Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y));
        }

        /// <summary>Applies a smooth step easing to a value clamped to the 0 to 1 range.</summary>
        /// <param name="t">The input value.</param>
        /// <returns>The eased value.</returns>
        public static float SmoothStep(float t)
        {
            t = Mathf.Clamp01(t);
            return t * t * (3f - (2f * t));
        }

        /// <summary>Samples Perlin noise centered on zero.</summary>
        /// <param name="x">First coordinate.</param>
        /// <param name="z">Second coordinate.</param>
        /// <returns>A value between -0.5 and 0.5.</returns>
        public static float Noise(float x, float z)
        {
            return Mathf.PerlinNoise(x + NoiseOffset.x, z + NoiseOffset.y) - 0.5f;
        }

        /// <summary>Returns the smooth fade from 1 at the near value to 0 at the far value.</summary>
        /// <param name="value">The measured value.</param>
        /// <param name="near">Value at which the result is 1.</param>
        /// <param name="far">Value at which the result is 0.</param>
        /// <returns>Weight between 0 and 1.</returns>
        public static float Fade(float value, float near, float far)
        {
            return SmoothStep(Mathf.InverseLerp(far, near, value));
        }
    }
}
