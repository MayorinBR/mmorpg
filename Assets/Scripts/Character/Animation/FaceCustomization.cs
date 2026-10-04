using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Project.Character.Animation
{
    /// <summary>
    /// Holds the player-chosen look of a face: iris color, eye drawing and mouth
    /// drawing. Attach it to the root of a character model next to
    /// <see cref="FacialExpressionController"/>. Changes made in the inspector are
    /// visible immediately, in edit mode and in play mode.
    /// </summary>
    [ExecuteAlways]
    public sealed class FaceCustomization : MonoBehaviour
    {
        [Tooltip("Renderer of the head mesh. When empty, the first child renderer named Face Mesh Name is used.")]
        [SerializeField] private Renderer faceRenderer;

        [SerializeField] private string faceMeshName = "part_0";

        [Tooltip("Names of the eye and mouth drawings, shown as dropdowns in the inspector.")]
        [SerializeField] private FacePartCatalog catalog;

        [SerializeField] private EyeColor eyeColor = EyeColor.Brown;

        [FormerlySerializedAs("eyeStyle")]
        [SerializeField] private int eyeIndex;

        [FormerlySerializedAs("mouthStyle")]
        [SerializeField] private int mouthIndex = 5;

        /// <summary>
        /// Raised after the look changed and was written to the renderer.
        /// </summary>
        public event Action Changed;

        /// <summary>
        /// Layer in the eye texture array of the chosen eye drawing.
        /// </summary>
        public int EyeFrame => eyeIndex;

        /// <summary>
        /// Layer in the mouth texture array of the chosen mouth drawing.
        /// </summary>
        public int MouthFrame => mouthIndex;

        /// <summary>
        /// Names of the selectable drawings, or null when no catalog is assigned.
        /// </summary>
        public FacePartCatalog Catalog => catalog;

        /// <summary>
        /// Chooses a new look and applies it. This is the entry point for a future
        /// character creation screen.
        /// </summary>
        /// <param name="color">Iris color.</param>
        /// <param name="eyes">Index of the eye drawing in the catalog.</param>
        /// <param name="mouth">Index of the mouth drawing in the catalog.</param>
        public void Apply(EyeColor color, int eyes, int mouth)
        {
            eyeColor = color;
            eyeIndex = eyes;
            mouthIndex = mouth;
            Refresh();
        }

        private void OnEnable()
        {
            Refresh();
        }

        private void OnValidate()
        {
            if (isActiveAndEnabled)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
            Renderer target = FaceRendererLocator.Find(this, faceRenderer, faceMeshName);
            if (target == null)
            {
                return;
            }

            var writer = new FaceFrameWriter(target);
            writer.ApplyIrisColor(EyeColorPalette.Resolve(eyeColor));
            writer.Apply(EyeFrame, MouthFrame);
            Changed?.Invoke();
        }
    }
}
