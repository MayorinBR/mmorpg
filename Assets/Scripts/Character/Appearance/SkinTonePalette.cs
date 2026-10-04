using UnityEngine;

namespace Project.Character.Appearance
{
    /// <summary>
    /// Maps each <see cref="SkinTone"/> to the color painted over the skin areas of the body
    /// textures. The first entry matches the color baked into the textures.
    /// </summary>
    public static class SkinTonePalette
    {
        private static readonly Color[] Colors =
        {
            new Color(0.94f, 0.82f, 0.78f, 1f),
            new Color(0.90f, 0.74f, 0.64f, 1f),
            new Color(0.78f, 0.60f, 0.46f, 1f),
            new Color(0.62f, 0.43f, 0.31f, 1f),
            new Color(0.38f, 0.25f, 0.19f, 1f)
        };

        /// <summary>
        /// Returns the skin color of a predefined tone.
        /// </summary>
        /// <param name="tone">The predefined tone.</param>
        /// <returns>The color to write into the skin color property of the toon material.</returns>
        public static Color Resolve(SkinTone tone)
        {
            int index = Mathf.Clamp((int)tone, 0, Colors.Length - 1);
            return Colors[index];
        }
    }
}
