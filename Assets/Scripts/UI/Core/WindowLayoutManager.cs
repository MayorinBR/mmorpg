using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Tracks currently-open, auto-positioned windows (ones that haven't
    /// been manually dragged) and recomputes their cascade positions
    /// whenever one opens or closes — so closing a window frees its slot
    /// for the next one instead of leaving a gap. Assumes windows use a
    /// top-left anchor and pivot, so X grows rightward and Y grows downward
    /// (as negative anchoredPosition.y). The first column starts just below
    /// <see cref="hudPanel"/> (see <see cref="FirstColumnStart"/>); any
    /// column after that starts at the topmost available position, using
    /// the same top margin as the HUD panel itself (see <see cref="TopMarginY"/>).
    /// </summary>
    public class WindowLayoutManager : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;
        [SerializeField] private RectTransform hudPanel;
        [SerializeField] private float spacing = 8f;

        private Vector2 FirstColumnStart => new Vector2(
            hudPanel.anchoredPosition.x,
            hudPanel.anchoredPosition.y - hudPanel.rect.height - spacing);

        private float TopMarginY => hudPanel.anchoredPosition.y;

        private readonly List<WindowPanel> openAutoPositionedWindows = new List<WindowPanel>();

        /// <summary>
        /// Registers a window as open and auto-positioned, then recomputes
        /// the cascade for every window currently registered.
        /// </summary>
        /// <param name="window">The window that just opened.</param>
        public void RegisterOpen(WindowPanel window)
        {
            if (!openAutoPositionedWindows.Contains(window))
            {
                openAutoPositionedWindows.Add(window);
            }

            Relayout();
        }

        /// <summary>
        /// Unregisters a window (closed, or just started being dragged),
        /// freeing its slot and recomputing the cascade for the remaining windows.
        /// </summary>
        /// <param name="window">The window to remove from the cascade.</param>
        public void RegisterClosed(WindowPanel window)
        {
            if (openAutoPositionedWindows.Remove(window))
            {
                Relayout();
            }
        }

        private void Relayout()
        {
            var cursorX = FirstColumnStart.x;
            var currentColumnStartY = FirstColumnStart.y;
            var cursorY = currentColumnStartY;
            var columnWidth = 0f;
            var availableHeight = canvasRect.rect.height;

            foreach (var window in openAutoPositionedWindows)
            {
                var rect = window.RectTransform;
                var width = rect.rect.width;
                var height = rect.rect.height;

                var wouldOverflow = Mathf.Abs(cursorY) + height > availableHeight;

                if (wouldOverflow && !Mathf.Approximately(cursorY, currentColumnStartY))
                {
                    cursorX += columnWidth + spacing;
                    currentColumnStartY = TopMarginY;
                    cursorY = currentColumnStartY;
                    columnWidth = 0f;
                }

                rect.anchoredPosition = new Vector2(cursorX, cursorY);

                cursorY -= height + spacing;
                columnWidth = Mathf.Max(columnWidth, width);
            }
        }
    }
}