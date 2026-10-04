using System.Collections;
using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Drives a character's face: holds the current <see cref="FacialExpression"/>
    /// and blinks automatically while the eyes are open. Attach it to the root of
    /// a character model (for example Male_Body or Female_Body). Gameplay code
    /// only calls <see cref="SetExpression"/>; frame indices stay in the presets.
    /// </summary>
    public class FacialExpressionController : MonoBehaviour
    {
        [Tooltip("Renderer of the head mesh. When empty, the first child renderer named Face Mesh Name is used.")]
        [SerializeField] private Renderer faceRenderer;

        [SerializeField] private string faceMeshName = "part_0";

        [Tooltip("Optional. When present, the Neutral expression and blinking use its chosen eye and mouth drawings.")]
        [SerializeField] private FaceCustomization customization;

        [SerializeField] private FacialExpression startingExpression = FacialExpression.Neutral;

        [SerializeField] private FaceExpressionPreset[] presets =
        {
            new FaceExpressionPreset(FacialExpression.Neutral, 0, 5),
            new FaceExpressionPreset(FacialExpression.Happy, 0, 7),
            new FaceExpressionPreset(FacialExpression.Sad, 7, 0),
            new FaceExpressionPreset(FacialExpression.Surprised, 5, 2),
            new FaceExpressionPreset(FacialExpression.Sleeping, 11, 5)
        };

        [Header("Blink")]
        [SerializeField] private bool autoBlink = true;

        [Tooltip("Eye frame that counts as open when there is no Face Customization. Only expressions that use this frame blink.")]
        [SerializeField] private int openEyeFrame;

        [Tooltip("Eye frames shown in order during one blink.")]
        [SerializeField] private int[] blinkFrames = { 11 };

        [SerializeField] private float blinkFrameSeconds = 0.06f;
        [SerializeField] private float minSecondsBetweenBlinks = 2f;
        [SerializeField] private float maxSecondsBetweenBlinks = 5f;

        private FaceFrameWriter writer;
        private int currentEyeFrame;
        private int currentMouthFrame;

        /// <summary>
        /// The expression currently shown.
        /// </summary>
        public FacialExpression Current { get; private set; }

        /// <summary>
        /// Shows the given expression. Unknown expressions fall back to the first preset.
        /// </summary>
        /// <param name="expression">The expression to show.</param>
        public void SetExpression(FacialExpression expression)
        {
            if (writer == null)
            {
                return;
            }

            FaceExpressionPreset preset = FindPreset(expression);
            Current = preset.expression;
            bool useChosenLook = customization != null && preset.expression == FacialExpression.Neutral;
            currentEyeFrame = useChosenLook ? customization.EyeFrame : preset.eyeFrame;
            currentMouthFrame = useChosenLook ? customization.MouthFrame : preset.mouthFrame;
            writer.Apply(currentEyeFrame, currentMouthFrame);
        }

        private void Awake()
        {
            if (customization == null)
            {
                customization = GetComponent<FaceCustomization>();
            }

            Renderer target = FaceRendererLocator.Find(this, faceRenderer, faceMeshName);
            if (target == null)
            {
                Debug.LogWarning($"{nameof(FacialExpressionController)} on '{name}' found no face renderer.", this);
                enabled = false;
                return;
            }

            writer = new FaceFrameWriter(target);
            SetExpression(startingExpression);
        }

        private void OnEnable()
        {
            if (customization != null)
            {
                customization.Changed += RefreshExpression;
            }

            if (autoBlink && writer != null)
            {
                StartCoroutine(BlinkLoop());
            }
        }

        private void OnDisable()
        {
            if (customization != null)
            {
                customization.Changed -= RefreshExpression;
            }

            StopAllCoroutines();
            writer?.Apply(currentEyeFrame, currentMouthFrame);
        }

        private int OpenEyeFrame => customization != null ? customization.EyeFrame : openEyeFrame;

        private void RefreshExpression()
        {
            SetExpression(Current);
        }

        private FaceExpressionPreset FindPreset(FacialExpression expression)
        {
            foreach (FaceExpressionPreset preset in presets)
            {
                if (preset.expression == expression)
                {
                    return preset;
                }
            }

            return presets.Length > 0 ? presets[0] : default(FaceExpressionPreset);
        }

        private IEnumerator BlinkLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minSecondsBetweenBlinks, maxSecondsBetweenBlinks));

                if (currentEyeFrame != OpenEyeFrame)
                {
                    continue;
                }

                foreach (int frame in blinkFrames)
                {
                    if (currentEyeFrame != OpenEyeFrame)
                    {
                        break;
                    }

                    writer.Apply(frame, currentMouthFrame);
                    yield return new WaitForSeconds(blinkFrameSeconds);
                }

                writer.Apply(currentEyeFrame, currentMouthFrame);
            }
        }
    }
}
