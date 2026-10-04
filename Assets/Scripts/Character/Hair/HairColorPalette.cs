using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// Maps each <see cref="HairColor"/> to the base color applied to the hair material.
    /// </summary>
    public static class HairColorPalette
    {
        private static readonly Color[] Colors =
        {
            new Color(0.10f, 0.09f, 0.11f, 1f),
            new Color(0.24f, 0.15f, 0.10f, 1f),
            new Color(0.42f, 0.26f, 0.14f, 1f),
            new Color(0.93f, 0.78f, 0.38f, 1f),
            new Color(0.78f, 0.18f, 0.12f, 1f),
            new Color(0.97f, 0.55f, 0.72f, 1f),
            new Color(0.25f, 0.45f, 0.90f, 1f),
            new Color(0.30f, 0.72f, 0.40f, 1f),
            new Color(0.58f, 0.38f, 0.82f, 1f),
            new Color(0.72f, 0.75f, 0.80f, 1f),
            new Color(0.96f, 0.96f, 0.98f, 1f)
        };

        /// <summary>
        /// Returns the base color for a predefined hair color.
        /// </summary>
        /// <param name="color">The predefined color.</param>
        /// <returns>The color to apply to the hair material.</returns>
        public static Color Resolve(HairColor color)
        {
            int index = Mathf.Clamp((int)color, 0, Colors.Length - 1);
            return Colors[index];
        }
    }
}
