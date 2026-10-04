using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Writes the current eye and mouth frame into a renderer through a
    /// <see cref="MaterialPropertyBlock"/>. The shared toon material stays
    /// untouched, so every character can show a different expression while
    /// using the same material asset.
    /// </summary>
    public sealed class FaceFrameWriter
    {
        private static readonly int EyeFrameProperty = Shader.PropertyToID("_EyeFrame");
        private static readonly int MouthFrameProperty = Shader.PropertyToID("_MouthFrame");
        private static readonly int IrisColorProperty = Shader.PropertyToID("_IrisColor");

        private readonly Renderer target;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

        /// <summary>
        /// Creates a writer for the renderer that draws the character's face.
        /// </summary>
        /// <param name="target">The renderer whose material uses the face overlay.</param>
        public FaceFrameWriter(Renderer target)
        {
            this.target = target;
        }

        /// <summary>
        /// Applies the given frames to the target renderer.
        /// </summary>
        /// <param name="eyeFrame">Frame index in the eye sheet.</param>
        /// <param name="mouthFrame">Frame index in the mouth sheet.</param>
        public void Apply(int eyeFrame, int mouthFrame)
        {
            target.GetPropertyBlock(block);
            block.SetFloat(EyeFrameProperty, eyeFrame);
            block.SetFloat(MouthFrameProperty, mouthFrame);
            target.SetPropertyBlock(block);
        }

        /// <summary>
        /// Applies the iris tint to the target renderer.
        /// </summary>
        /// <param name="color">The iris color.</param>
        public void ApplyIrisColor(Color color)
        {
            target.GetPropertyBlock(block);
            block.SetColor(IrisColorProperty, color);
            target.SetPropertyBlock(block);
        }
    }
}
