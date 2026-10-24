using UnityEngine;
using UnityEngine.UI;
using Project.CameraSystem;
using Project.Character.Combat;

namespace Project.UI
{
    /// <summary>
    /// Shows a small progress bar under the character while a skill with a
    /// cast time is being cast. The bar lives on its own screen-space canvas
    /// and follows the character's projected position, so scene geometry can
    /// never hide it. Builds itself at runtime (no prefab or scene wiring
    /// besides adding this component to the player), fills from left to
    /// right over the cast duration and hides when the cast completes or is
    /// interrupted.
    /// </summary>
    public class CastBarWorldUI : MonoBehaviour
    {
        private const int SortingOrder = 50;

        [Tooltip("Optional. Resolved from this object's hierarchy root when left empty.")]
        [SerializeField] private PlayerSkillCaster caster;

        [Tooltip("Offset in screen pixels from the character's feet. Keep Y negative so the bar sits under the character.")]
        [SerializeField] private Vector2 screenOffset = new Vector2(0f, -14f);

        [SerializeField] private Vector2 size = new Vector2(100f, 8f);
        [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.75f);
        [SerializeField] private Color fillColor = Color.white;

        private GameObject canvasRoot;
        private RectTransform bar;
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

            if (canvasRoot == null)
            {
                isCasting = false;
                return;
            }

            var progress = duration > 0f ? Mathf.Clamp01((Time.time - startTime) / duration) : 1f;
            fill.anchorMax = new Vector2(progress, 1f);

            if (viewCamera == null)
            {
                viewCamera = IsometricCameraController.Instance != null
                    ? IsometricCameraController.Instance.GetComponent<Camera>()
                    : Camera.main;
            }

            if (viewCamera == null)
            {
                return;
            }

            var screenPoint = viewCamera.WorldToScreenPoint(transform.position);
            canvasRoot.SetActive(screenPoint.z > 0f);
            bar.position = new Vector3(screenPoint.x + screenOffset.x, screenPoint.y + screenOffset.y, 0f);
        }

        private void OnDestroy()
        {
            if (canvasRoot != null)
            {
                Destroy(canvasRoot);
            }
        }

        private void HandleCastStarted(float castDuration)
        {
            startTime = Time.time;
            duration = castDuration;
            isCasting = true;

            if (canvasRoot == null)
            {
                BuildBar();
            }

            canvasRoot.SetActive(true);
        }

        private void HandleCastEnded()
        {
            isCasting = false;

            if (canvasRoot != null)
            {
                canvasRoot.SetActive(false);
            }
        }

        private void BuildBar()
        {
            canvasRoot = new GameObject("CastBar", typeof(RectTransform));
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(canvasRoot, gameObject.scene);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = SortingOrder;

            var barObject = new GameObject("Bar", typeof(RectTransform));
            bar = (RectTransform)barObject.transform;
            bar.SetParent(canvasRoot.transform, false);
            bar.anchorMin = Vector2.zero;
            bar.anchorMax = Vector2.zero;
            bar.pivot = new Vector2(0.5f, 1f);
            bar.sizeDelta = size;

            var background = barObject.AddComponent<Image>();
            background.color = backgroundColor;
            background.raycastTarget = false;

            var fillObject = new GameObject("Fill", typeof(RectTransform));
            fill = (RectTransform)fillObject.transform;
            fill.SetParent(bar, false);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(0f, 1f);
            fill.offsetMin = new Vector2(1f, 1f);
            fill.offsetMax = new Vector2(-1f, -1f);

            var fillImage = fillObject.AddComponent<Image>();
            fillImage.color = fillColor;
            fillImage.raycastTarget = false;

            canvasRoot.SetActive(false);
        }
    }
}
