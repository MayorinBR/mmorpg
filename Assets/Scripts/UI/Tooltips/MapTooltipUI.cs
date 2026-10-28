using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// A single shared tooltip panel showing a map's display name and, when
    /// the map has one, an enlarged picture of it below the name. The picture
    /// is <see cref="ReferenceSide"/> pixels square at <see cref="ReferenceScreenHeight"/>
    /// screen height and scales with the screen. Mirrors
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
        [SerializeField] private Image previewImage;
        [SerializeField] private Vector2 offsetFromPointer = new Vector2(16f, -16f);

        private static readonly Color BackgroundColor = new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.95f);

        private const float TextPadding = 8f;
        private const float PreviewPadding = 8f;
        private const float HeaderHeight = 32f;

        /// <summary>Side in screen pixels of the picture when the screen is <see cref="ReferenceScreenHeight"/> tall.</summary>
        private const float ReferenceSide = 512f;

        /// <summary>Screen height in pixels at which the picture is exactly <see cref="ReferenceSide"/>.</summary>
        private const float ReferenceScreenHeight = 1080f;

        /// <summary>Largest fraction of the screen height the whole tooltip may take.</summary>
        private const float MaxScreenHeightFraction = 0.9f;

        private Canvas parentCanvas;
        private Vector2 nameOnlySize;

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
            nameOnlySize = rectTransform.sizeDelta;
            rectTransform.pivot = new Vector2(0f, 1f);

            if (root.TryGetComponent<Image>(out var background))
            {
                background.color = BackgroundColor;
                background.raycastTarget = false;
            }

            nameText.color = Color.white;
            nameText.alignment = TextAlignmentOptions.Center;
            nameText.verticalAlignment = VerticalAlignmentOptions.Middle;
            nameText.raycastTarget = false;
            previewImage.raycastTarget = false;
            previewImage.preserveAspect = true;
            Hide();
        }

        /// <summary>
        /// Shows the tooltip with the given map name, and its enlarged picture when there is one, at the given
        /// screen position.
        /// </summary>
        /// <param name="mapName">The map name to display. If null or empty, the tooltip hides instead.</param>
        /// <param name="preview">The enlarged picture of the map, or null to show only the name.</param>
        /// <param name="screenPosition">The screen-space position to anchor the tooltip near (typically the pointer position).</param>
        public void Show(string mapName, Sprite preview, Vector2 screenPosition)
        {
            if (string.IsNullOrEmpty(mapName))
            {
                Hide();
                return;
            }

            nameText.text = mapName;
            Layout(preview);
            root.SetActive(true);
            rectTransform.position = ClampToScreen(screenPosition + offsetFromPointer);
        }

        /// <summary>Hides the tooltip.</summary>
        public void Hide()
        {
            root.SetActive(false);
        }

        private void Layout(Sprite preview)
        {
            var textRect = nameText.rectTransform;
            previewImage.gameObject.SetActive(preview != null);

            if (preview == null)
            {
                rectTransform.sizeDelta = nameOnlySize;
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(TextPadding, 0f);
                textRect.offsetMax = new Vector2(-TextPadding, 0f);
                return;
            }

            var side = PreviewSide();
            rectTransform.sizeDelta = new Vector2(side + PreviewPadding * 2f, HeaderHeight + side + PreviewPadding * 2f);

            textRect.anchorMin = new Vector2(0f, 1f);
            textRect.anchorMax = Vector2.one;
            textRect.pivot = new Vector2(0.5f, 1f);
            textRect.sizeDelta = new Vector2(-TextPadding * 2f, HeaderHeight);
            textRect.anchoredPosition = Vector2.zero;

            var previewRect = previewImage.rectTransform;
            previewRect.anchorMin = previewRect.anchorMax = new Vector2(0.5f, 1f);
            previewRect.pivot = new Vector2(0.5f, 1f);
            previewRect.sizeDelta = new Vector2(side, side);
            previewRect.anchoredPosition = new Vector2(0f, -HeaderHeight);
            previewImage.sprite = preview;
        }

        /// <summary>
        /// Computes the picture side in canvas units: <see cref="ReferenceSide"/> scaled by the screen height and
        /// capped so the whole tooltip fits on screen.
        /// </summary>
        /// <returns>The side of the square picture in canvas units.</returns>
        private float PreviewSide()
        {
            var scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            var sidePixels = ReferenceSide * Screen.height / ReferenceScreenHeight;
            var maxSidePixels = Screen.height * MaxScreenHeightFraction - (HeaderHeight + PreviewPadding * 2f) * scaleFactor;
            return Mathf.Max(1f, Mathf.Min(sidePixels, maxSidePixels) / scaleFactor);
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
