using UnityEngine;

namespace Project.Character.Appearance
{
    /// <summary>
    /// Writes the skin color into every renderer of a model whose material has a skin color
    /// property, through a <see cref="MaterialPropertyBlock"/>. The shared toon material stays
    /// untouched, so characters with different skin tones can share the same material asset.
    /// </summary>
    public static class SkinTint
    {
        private static readonly int SkinColorProperty = Shader.PropertyToID("_SkinColor");

        /// <summary>
        /// Applies a skin color to a character model.
        /// </summary>
        /// <param name="model">Root of the instantiated character model.</param>
        /// <param name="color">The skin color.</param>
        public static void Apply(GameObject model, Color color)
        {
            var block = new MaterialPropertyBlock();

            foreach (Renderer target in model.GetComponentsInChildren<Renderer>(true))
            {
                Material material = target.sharedMaterial;
                if (material == null || !material.HasProperty(SkinColorProperty))
                {
                    continue;
                }

                target.GetPropertyBlock(block);
                block.SetColor(SkinColorProperty, color);
                target.SetPropertyBlock(block);
            }
        }
    }
}
