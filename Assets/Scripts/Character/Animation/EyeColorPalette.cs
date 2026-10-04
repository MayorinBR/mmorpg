using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Maps each <see cref="EyeColor"/> to the tint applied to the iris. Colors are
    /// chosen to be clearly different from each other once multiplied with the
    /// grayscale iris drawing.
    /// </summary>
    public static class EyeColorPalette
    {
        private static readonly Color[] Colors =
        {
            new Color(0.45f, 0.28f, 0.15f, 1f),
            new Color(0.20f, 0.45f, 0.95f, 1f),
            new Color(0.20f, 0.70f, 0.30f, 1f),
            new Color(0.70f, 0.50f, 0.95f, 1f),
            new Color(0.85f, 0.12f, 0.15f, 1f),
            new Color(0.95f, 0.72f, 0.12f, 1f),
            new Color(1.00f, 0.45f, 0.70f, 1f),
            new Color(0.10f, 0.85f, 0.85f, 1f),
            new Color(0.62f, 0.66f, 0.72f, 1f),
            new Color(0.14f, 0.12f, 0.18f, 1f)
        };

        /// <summary>
        /// Returns the iris tint for a predefined eye color.
        /// </summary>
        /// <param name="color">The predefined color.</param>
        /// <returns>The tint to multiply with the iris drawing.</returns>
        public static Color Resolve(EyeColor color)
        {
            int index = Mathf.Clamp((int)color, 0, Colors.Length - 1);
            return Colors[index];
        }
    }
}
