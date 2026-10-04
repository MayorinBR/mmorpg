using System;

namespace Project.Character.Animation
{
    /// <summary>
    /// Maps one <see cref="FacialExpression"/> to the frame indices of the eye
    /// sheet and the mouth sheet used by the face overlay in the toon shader.
    /// Frames are counted left to right, then top to bottom, starting at 0.
    /// </summary>
    [Serializable]
    public struct FaceExpressionPreset
    {
        /// <summary>The expression this preset describes.</summary>
        public FacialExpression expression;

        /// <summary>Frame index in the eye sheet.</summary>
        public int eyeFrame;

        /// <summary>Frame index in the mouth sheet.</summary>
        public int mouthFrame;

        /// <summary>
        /// Creates a preset.
        /// </summary>
        /// <param name="expression">The expression this preset describes.</param>
        /// <param name="eyeFrame">Frame index in the eye sheet.</param>
        /// <param name="mouthFrame">Frame index in the mouth sheet.</param>
        public FaceExpressionPreset(FacialExpression expression, int eyeFrame, int mouthFrame)
        {
            this.expression = expression;
            this.eyeFrame = eyeFrame;
            this.mouthFrame = mouthFrame;
        }
    }
}
