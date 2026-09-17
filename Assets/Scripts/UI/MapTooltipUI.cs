using UnityEngine;
using TMPro;

namespace Project.UI
{
    /// <summary>
    /// A single shared tooltip panel showing a map's display name. Mirrors
    /// <see cref="SkillTooltipUI"/>: hover sources across the UI show and
    /// hide this same instance rather than each owning their own panel.
    /// </summary>
    public class MapTooltipUI : MonoBehaviour
    {
        /// <summary>Gets the active tooltip instance in the scene.</summary>
        public static MapTooltipUI Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Vector2 offsetFromPointer = new Vector2(16f, -16f);

        private Canvas parentCanvas;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A short-lived duplicate from a map scene being reloaded
                // (e.g. warping back into a previously visited map) — its
                // parent Canvas is already being destroyed as a duplicate.
                // Don't let it steal Instance from the real, persisted panel.
                return;
            }

            Instance = this;
            parentCanvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>
        /// Shows the tooltip with the given map name at the given screen position.
        /// </summary>
        /// <param name="mapName">The map name to display. If null or empty, the tooltip hides instead.</param>
        /// <param name="screenPosition">The screen-space position to anchor the tooltip near (typically the pointer position).</param>
        public void Show(string mapName, Vector2 screenPosition)
        {
            if (string.IsNullOrEmpty(mapName))
            {
                Hide();
                return;
            }

            nameText.text = mapName;
            rectTransform.position = ClampToScreen(screenPosition + offsetFromPointer);
            root.SetActive(true);
        }

        /// <summary>Hides the tooltip.</summary>
        public void Hide()
        {
            root.SetActive(false);
        }

        private Vector2 ClampToScreen(Vector2 desiredPosition)
        {
            var scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            var size = rectTransform.rect.size * scaleFactor;
            var pivot = rectTransform.pivot;

            var minX = desiredPosition.x - (size.x * pivot.x);
            var maxX = desiredPosition.x + (size.x * (1f - pivot.x));
            var minY = desiredPosition.y - (size.y * pivot.y);
            var maxY = desiredPosition.y + (size.y * (1f - pivot.y));

            var clamped = desiredPosition;

            if (maxX > Screen.width)
            {
                clamped.x -= maxX - Screen.width;
            }

            if (minX < 0f)
            {
                clamped.x -= minX;
            }

            if (maxY > Screen.height)
            {
                clamped.y -= maxY - Screen.height;
            }

            if (minY < 0f)
            {
                clamped.y -= minY;
            }

            return clamped;
        }
    }
}
