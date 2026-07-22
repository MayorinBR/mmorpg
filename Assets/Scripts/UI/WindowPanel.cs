using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// A reusable window panel with open/close/minimize behavior, wired to
    /// its own Close (X) and Minimize (_) buttons. When opened without a
    /// remembered custom position, requests the next cascade slot from
    /// <see cref="WindowLayoutManager"/>. Once dragged (via
    /// <see cref="WindowDragHandler"/>), the custom position is remembered
    /// for the rest of the session and used on every subsequent open.
    /// Clicking the window (or starting to drag it) brings it to front.
    /// </summary>
    public class WindowPanel : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button minimizeButton;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private string id;
        [SerializeField] private WindowLayoutManager layoutManager;

        private RectTransform rectTransform;
        private bool isMinimized;
        private bool hasCustomPosition;
        private Vector2 customPosition;

        /// <summary>
        /// Gets this window's stable identifier, used for lookup by
        /// <see cref="PlayerUIController"/> and never shown to the player —
        /// keeping it separate from the displayed title is what allows the
        /// title to be localized later without breaking button/shortcut references.
        /// </summary>
        public string Id => id;

        /// <summary>Gets a value indicating whether this window is currently open (visible, minimized or not).</summary>
        public bool IsOpen => gameObject.activeSelf;

        /// <summary>Gets this window's own RectTransform, used by <see cref="WindowDragHandler"/> to move it.</summary>
        public RectTransform RectTransform => rectTransform;

        private void Awake()
        {
            rectTransform = (RectTransform)transform;

            if (titleText != null)
            {
                titleText.text = WindowTitleLookup.GetDisplayName(id);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (minimizeButton != null)
            {
                minimizeButton.onClick.AddListener(ToggleMinimize);
            }
        }

        /// <summary>
        /// Opens the window, restoring it from minimized if needed.
        /// Positions it at its remembered custom position if it has been
        /// dragged before this session, or requests the next cascade slot otherwise.
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            isMinimized = false;

            if (contentRoot != null)
            {
                contentRoot.gameObject.SetActive(true);
            }

            if (hasCustomPosition)
            {
                rectTransform.anchoredPosition = customPosition;
            }
            else if (layoutManager != null)
            {
                layoutManager.RegisterOpen(this);
            }
        }

        /// <summary>Fully closes (hides) the window.</summary>
        public void Close()
        {
            gameObject.SetActive(false);

            if (!hasCustomPosition && layoutManager != null)
            {
                layoutManager.RegisterClosed(this);
            }
        }

        /// <summary>Opens the window if closed, or closes it if currently open.</summary>
        public void Toggle()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open();
            }
        }

        /// <summary>Collapses the window to just its title bar, or restores it if already minimized.</summary>
        public void ToggleMinimize()
        {
            isMinimized = !isMinimized;

            if (contentRoot != null)
            {
                contentRoot.gameObject.SetActive(!isMinimized);
            }
        }

        /// <summary>
        /// Records a manually-set position (called by <see cref="WindowDragHandler"/>
        /// while dragging), so future opens use this position instead of the cascade.
        /// </summary>
        /// <param name="position">The new anchored position to remember.</param>
        public void SetCustomPosition(Vector2 position)
        {
            if (!hasCustomPosition && layoutManager != null)
            {
                layoutManager.RegisterClosed(this);
            }

            hasCustomPosition = true;
            customPosition = position;
        }

        /// <summary>
        /// Brings this window to the front, drawn above any other window
        /// sharing the same parent Canvas.
        /// </summary>
        public void BringToFront()
        {
            transform.SetAsLastSibling();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }
    }
}