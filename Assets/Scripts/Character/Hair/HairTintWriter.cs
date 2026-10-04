using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// Writes the hair color into every renderer of a hair model through a
    /// <see cref="MaterialPropertyBlock"/>. The shared hair material stays untouched, so each
    /// character can use a different color with the same material asset.
    /// </summary>
    public sealed class HairTintWriter
    {
        private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");

        private MaterialPropertyBlock block;

        /// <summary>
        /// Applies the color to all renderers under the hair model.
        /// </summary>
        /// <param name="hair">Root of the instantiated hair model.</param>
        /// <param name="color">Base color of the hair.</param>
        public void Apply(GameObject hair, Color color)
        {
            block ??= new MaterialPropertyBlock();

            foreach (Renderer target in hair.GetComponentsInChildren<Renderer>(true))
            {
                target.GetPropertyBlock(block);
                block.SetColor(BaseColorProperty, color);
                target.SetPropertyBlock(block);
            }
        }
    }
}
