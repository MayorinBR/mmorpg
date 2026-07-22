using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.UI
{
    /// <summary>
    /// Placed on a window's title bar. Dragging it moves the associated
    /// <see cref="WindowPanel"/> and records the resulting position as that
    /// window's custom position, so it's remembered for the rest of the session.
    /// </summary>
    public class WindowDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private WindowPanel windowPanel;

        public void OnBeginDrag(PointerEventData eventData)
        {
            windowPanel.BringToFront();
        }

        public void OnDrag(PointerEventData eventData)
        {
            var scaleFactor = GetCanvasScaleFactor();
            windowPanel.RectTransform.anchoredPosition += eventData.delta / scaleFactor;
            windowPanel.SetCustomPosition(windowPanel.RectTransform.anchoredPosition);
        }

        private float GetCanvasScaleFactor()
        {
            var canvas = windowPanel.RectTransform.GetComponentInParent<Canvas>();
            return canvas != null ? canvas.scaleFactor : 1f;
        }
    }
}