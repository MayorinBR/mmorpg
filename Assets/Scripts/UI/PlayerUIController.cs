using System.Collections.Generic;
using UnityEngine;
using Project.Character.Movement;

namespace Project.UI
{
    /// <summary>
    /// Maps window IDs (each <see cref="WindowPanel"/>'s own <see cref="WindowPanel.Id"/>)
    /// to their references, exposing a single <see cref="ToggleWindow"/>
    /// entry point used by both HUD buttons and keyboard shortcuts. Adding
    /// a new window later (quests, skills, etc.) only requires dragging it
    /// into the array in the Inspector, not new code here.
    /// Also wires <see cref="ShopWindowUI"/> to <see cref="PlayerNpcInteractionController"/>
    /// directly, rather than letting the shop window subscribe to it in its
    /// own Awake: the shop window's GameObject starts inactive, and Unity
    /// never runs Awake on an inactive GameObject, so it could never
    /// subscribe to anything on its own. This controller's GameObject is
    /// always active, so it can safely mediate.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private WindowPanel[] windows;
        [SerializeField] private PlayerNpcInteractionController npcInteractionController;
        [SerializeField] private ShopWindowUI shopWindow;

        private Dictionary<string, WindowPanel> windowsById;

        private void Awake()
        {
            windowsById = new Dictionary<string, WindowPanel>();

            foreach (var window in windows)
            {
                windowsById[window.Id] = window;
            }

            if (npcInteractionController != null && shopWindow != null)
            {
                npcInteractionController.ShopOpened -= shopWindow.Open;
                npcInteractionController.ShopOpened += shopWindow.Open;

                npcInteractionController.ShopCloseRequested -= shopWindow.Close;
                npcInteractionController.ShopCloseRequested += shopWindow.Close;

                shopWindow.Closed -= npcInteractionController.ClearOpenShop;
                shopWindow.Closed += npcInteractionController.ClearOpenShop;
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