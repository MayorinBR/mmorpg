using UnityEngine;
using UnityEngine.UI;
using Project.CameraSystem;
using Project.Character.Combat;

namespace Project.UI
{
    /// <summary>
    /// Shows a small world-space progress bar under the character while a
    /// skill with a cast time is being cast. Builds its own canvas at runtime
    /// (no prefab or scene wiring besides adding this component to the
    /// player), fills from left to right over the cast duration and hides
    /// itself when the cast completes or is interrupted.
    /// </summary>
    public class CastBarWorldUI : MonoBehaviour
    {
        private const float CanvasScale = 0.01f;

        [Tooltip("Optional. Resolved from this object's hierarchy root when left empty.")]
        [SerializeField] private PlayerSkillCaster caster;

        [Tooltip("Offset from the character's position. Keep Y negative so the bar sits below the character, under its other bars.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, -0.45f, 0f);

        [SerializeField] private Vector2 size = new Vector2(100f, 8f);
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);
        [SerializeField] private Color fillColor = new Color(0.95f, 0.8f, 0.2f, 1f);

        private GameObject barRoot;
        private RectTransform fill;
        private Camera viewCamera;
        private float startTime;
        private float duration;
        private bool isCasting;

        private void Awake()
        {
            if (caster == null)
            {
                caster = transform.root.GetComponentInChildren<PlayerSkillCaster>(true);
            }

            BuildBar();
        }

        private void OnEnable()
        {
            if (caster == null)
            {
                return;
            }

            caster.CastStarted += HandleCastStarted;
            caster.CastEnded += HandleCastEnded;
        }

        private void OnDisable()
        {
            if (caster != null)
            {
                caster.CastStarted -= HandleCastStarted;
                caster.CastEnded -= HandleCastEnded;
            }

            HandleCastEnded();
        }

        private void LateUpdate()
        {
            if (!isCasting)
            {
                return;
            }

            var progress = duration > 0f ? Mathf.Clamp01((Time.time - startTime) / duration) : 1f;
            fill.anchorMax = new Vector2(progress, 1f);

            barRoot.transform.position = transform.position + offset;

            if (viewCamera == null)
            {
                viewCamera = IsometricCameraController.Instance != null
                    ? IsometricCameraController.Instance.GetComponent<Camera>()
                    : Camera.main;
            }

            if (viewCamera != null)
            {
                barRoot.transform.forward = viewCamera.transform.forward;
            }
        }

        private void OnDestroy()
        {
            if (barRoot != null)
            {
                Destroy(barRoot);
            }
        }

        private void HandleCastStarted(float castDuration)
        {
            startTime = Time.time;
            duration = castDuration;
            isCasting = true;
            barRoot.SetActive(true);
        }

        private void HandleCastEnded()
        {
            isCasting = false;

            if (barRoot != null)
            {
                barRoot.SetActive(false);
            }
        }

        private void BuildBar()
        {
            barRoot = new GameObject("CastBar", typeof(RectTransform));
            var rootRect = (RectTransform)barRoot.transform;
            rootRect.sizeDelta = size;
            rootRect.localScale = Vector3.one * CanvasScale;

            var canvas = barRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var background = barRoot.AddComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;

            var fillObject = new GameObject("Fill", typeof(RectTransform));
            fill = (RectTransform)fillObject.transform;
            fill.SetParent(rootRect, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.offsetMin = new Vector2(1f, 1f);
            fill.offsetMax = new Vector2(-1f, -1f);

            var fillImage = fillObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            barRoot.SetActive(false);
        }
    }
}
