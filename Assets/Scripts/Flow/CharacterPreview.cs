using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Project.Character.Animation;
using Project.Character.Appearance;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Flow
{
    /// <summary>
    /// Live 3D preview of the character being created. Add it to the RawImage that fills the view
    /// container: a hidden camera far from the rest of the scene renders the model into a texture
    /// that the RawImage shows. The camera orbits the model at the same pitch as the gameplay camera,
    /// turning slowly on its own. Dragging with the left or middle button inside the image turns it
    /// by hand and the scroll wheel zooms; after the button is released the view stays still for a
    /// few seconds before the slow turn resumes.
    /// </summary>
    /// <remarks>
    /// The model is lit by the scene's own directional light, so the character creation scene needs one.
    /// </remarks>
    [RequireComponent(typeof(RawImage))]
    public sealed class CharacterPreview : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
    {
        [Header("Models")]
        [SerializeField] private GameObject malePrefab;
        [SerializeField] private GameObject femalePrefab;
        [SerializeField] private RuntimeAnimatorController sharedAnimatorController;

        [Header("Camera")]
        [Tooltip("Where the model stands. Keep it far from anything else in the scene.")]
        [SerializeField] private Vector3 stagePosition = new Vector3(1000f, 0f, 1000f);
        [SerializeField] private float pitch = 45f;
        [SerializeField] private float fieldOfView = 30f;
        [SerializeField] private float startDistance = 4.5f;
        [SerializeField] private float minDistance = 2.5f;
        [SerializeField] private float maxDistance = 8f;
        [Tooltip("Height of the point the camera looks at, measured from the model's feet.")]
        [SerializeField] private float focusHeight = 0.9f;
        [SerializeField] private float startYaw = 180f;
        [SerializeField] private float renderScale = 1.5f;

        [Header("Interaction")]
        [SerializeField] private float autoRotateSpeed = 20f;
        [SerializeField] private float dragSensitivity = 0.4f;
        [SerializeField] private float zoomStep = 0.5f;
        [SerializeField] private float holdSeconds = 5f;

        private RawImage image;
        private RenderTexture renderTexture;
        private Camera previewCamera;
        private GameObject model;
        private AppearanceOptions options;
        private CharacterGender gender;
        private CharacterAppearanceData appearance;
        private float yaw;
        private float distance;
        private float holdUntil;
        private bool dragging;
        private Vector2Int textureSize;

        private void Awake()
        {
            image = GetComponent<RawImage>();
            yaw = startYaw;
            distance = startDistance;
        }

        private void Start()
        {
            CreateRenderTarget();
            CreateCamera();
        }

        private void LateUpdate()
        {
            if (previewCamera == null)
            {
                return;
            }

            ResizeRenderTargetIfNeeded();

            if (!dragging && Time.unscaledTime >= holdUntil)
            {
                yaw += autoRotateSpeed * Time.unscaledDeltaTime;
            }

            Vector3 focus = stagePosition + Vector3.up * focusHeight;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
            previewCamera.transform.SetPositionAndRotation(focus + rotation * Vector3.back * distance, rotation);
        }

        private void OnDestroy()
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
                Destroy(renderTexture);
            }

            if (previewCamera != null)
            {
                Destroy(previewCamera.gameObject);
            }
        }

        /// <summary>
        /// Shows a model with an appearance, replacing the previous one.
        /// </summary>
        /// <param name="newGender">Gender that selects the base model.</param>
        /// <param name="newAppearance">The appearance to apply.</param>
        /// <param name="newOptions">The option lists used to resolve the appearance names.</param>
        public void Show(CharacterGender newGender, CharacterAppearanceData newAppearance, AppearanceOptions newOptions)
        {
            bool genderChanged = model == null || newGender != gender;
            gender = newGender;
            appearance = newAppearance;
            options = newOptions;

            if (genderChanged)
            {
                SpawnModel();
            }

            Refresh();
        }

        /// <summary>
        /// Applies the stored appearance to the model again, after the appearance data was changed.
        /// </summary>
        public void Refresh()
        {
            AppearanceApplier.Apply(model, appearance, gender, options);
        }

        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            if (IsRotateButton(eventData))
            {
                dragging = true;
            }
        }

        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            if (dragging && IsRotateButton(eventData))
            {
                dragging = false;
                holdUntil = Time.unscaledTime + holdSeconds;
            }
        }

        /// <inheritdoc />
        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
            {
                yaw += eventData.delta.x * dragSensitivity;
            }
        }

        /// <inheritdoc />
        public void OnScroll(PointerEventData eventData)
        {
            distance = Mathf.Clamp(distance - Mathf.Sign(eventData.scrollDelta.y) * zoomStep, minDistance, maxDistance);
            holdUntil = Time.unscaledTime + holdSeconds;
        }

        private static bool IsRotateButton(PointerEventData eventData)
        {
            return eventData.button == PointerEventData.InputButton.Left
                || eventData.button == PointerEventData.InputButton.Middle;
        }

        private void SpawnModel()
        {
            if (model != null)
            {
                Destroy(model);
            }

            GameObject prefab = gender == CharacterGender.Female ? femalePrefab : malePrefab;
            if (prefab == null)
            {
                Debug.LogError($"CharacterPreview has no prefab for {gender}.", this);
                return;
            }

            model = Instantiate(prefab, stagePosition, Quaternion.identity);
            model.name = $"Preview_{gender}";

            var animator = model.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.runtimeAnimatorController = sharedAnimatorController;
                animator.applyRootMotion = false;
            }
        }

        private void CreateRenderTarget()
        {
            Canvas.ForceUpdateCanvases();
            textureSize = MeasureTargetSize();
            renderTexture = new RenderTexture(textureSize.x, textureSize.y, 24, RenderTextureFormat.ARGB32);
            image.texture = renderTexture;
        }

        private Vector2Int MeasureTargetSize()
        {
            Vector2 size = Vector2.Scale(((RectTransform)transform).rect.size, transform.lossyScale);
            return new Vector2Int(
                Mathf.Max(64, Mathf.RoundToInt(size.x * renderScale)),
                Mathf.Max(64, Mathf.RoundToInt(size.y * renderScale)));
        }

        private void ResizeRenderTargetIfNeeded()
        {
            Vector2Int size = MeasureTargetSize();
            if (Mathf.Abs(size.x - textureSize.x) < 8 && Mathf.Abs(size.y - textureSize.y) < 8)
            {
                return;
            }

            textureSize = size;
            previewCamera.targetTexture = null;
            renderTexture.Release();
            Destroy(renderTexture);
            renderTexture = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
            image.texture = renderTexture;
            previewCamera.targetTexture = renderTexture;
        }

        private void CreateCamera()
        {
            var cameraObject = new GameObject("CharacterPreviewCamera");
            previewCamera = cameraObject.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = Color.clear;
            previewCamera.fieldOfView = fieldOfView;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 50f;
            previewCamera.targetTexture = renderTexture;
        }
    }
}
