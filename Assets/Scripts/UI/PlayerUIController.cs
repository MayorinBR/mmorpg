using System.Collections.Generic;
using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Maps window IDs (each <see cref="WindowPanel"/>'s own <see cref="WindowPanel.Id"/>)
    /// to their references, exposing a single <see cref="ToggleWindow"/>
    /// entry point used by both HUD buttons and keyboard shortcuts. Adding
    /// a new window later (quests, skills, etc.) only requires dragging it
    /// into the array in the Inspector, not new code here.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private WindowPanel[] windows;

        private Dictionary<string, WindowPanel> windowsById;

        private void Awake()
        {
            windowsById = new Dictionary<string, WindowPanel>();

            foreach (var window in windows)
            {
                windowsById[window.Id] = window;
            }
        }

        /// <summary>
        /// Toggles the window whose <see cref="WindowPanel.Id"/> matches the
        /// given ID. Does nothing if no window matches.
        /// </summary>
        /// <param name="id">The window's ID, matching its own configured title.</param>
        public void ToggleWindow(string id)
        {
            if (windowsById.TryGetValue(id, out var window))
            {
                window.Toggle();
            }
        }
    }
}