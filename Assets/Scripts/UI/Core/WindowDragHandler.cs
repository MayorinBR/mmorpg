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
            windowPanel.Drag(eventData.delta);
        }
    }
}