using System.Collections.Generic;
using UnityEngine;
using Project.Character.Movement;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// Maps window IDs (each <see cref="WindowPanel"/>'s own <see cref="WindowPanel.Id"/>)
    /// to their references, exposing a single <see cref="ToggleWindow"/>
    /// entry point used by both HUD buttons and keyboard shortcuts. Adding
    /// a new window later (quests, skills, etc.) only requires dragging it
    /// into the array in the Inspector, not new code here.
    /// Also wires <see cref="DialogueWindowUI"/> to
    /// <see cref="PlayerNpcInteractionController"/> directly, rather than
    /// letting the dialogue window subscribe to it in its own Awake: the
    /// dialogue window's GameObject starts inactive, and Unity never runs
    /// Awake on an inactive GameObject, so it could never subscribe to
    /// anything on its own. This controller's GameObject is always active,
    /// so it can safely mediate. <see cref="ShopWindowUI"/> is wired the
    /// same way, but to <see cref="DialogueWindowUI.ShopRequested"/> rather
    /// than to the interaction controller directly — walking up to an NPC
    /// now always opens its dialogue first, and the shop only opens if the
    /// player picks a "open shop" response from it. <see cref="QuestManager"/>
    /// is wired the same way, to <see cref="DialogueWindowUI.QuestAcceptRequested"/>,
    /// and <see cref="QuestBoardWindowUI"/> to <see cref="DialogueWindowUI.QuestBoardRequested"/>
    /// — for an interactable (e.g. a bulletin board) that posts several
    /// quests at once rather than offering one directly through dialogue.
    /// </summary>
    public class PlayerUIController : MonoBehaviour
    {
        [SerializeField] private WindowPanel[] windows;
        [SerializeField] private PlayerNpcInteractionController npcInteractionController;
        [SerializeField] private DialogueWindowUI dialogueWindow;
        [SerializeField] private ShopWindowUI shopWindow;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private QuestBoardWindowUI questBoardWindow;

        private Dictionary<string, WindowPanel> windowsById;

        private void Awake()
        {
            windowsById = new Dictionary<string, WindowPanel>();

            foreach (var window in windows)
            {
                windowsById[window.Id] = window;
            }

            if (npcInteractionController != null && dialogueWindow != null)
            {
                npcInteractionController.DialogueOpened -= dialogueWindow.Open;
                npcInteractionController.DialogueOpened += dialogueWindow.Open;

                npcInteractionController.DialogueCloseRequested -= dialogueWindow.Close;
                npcInteractionController.DialogueCloseRequested += dialogueWindow.Close;

                dialogueWindow.Closed -= npcInteractionController.ClearOpenDialogue;
                dialogueWindow.Closed += npcInteractionController.ClearOpenDialogue;
            }

            if (dialogueWindow != null && shopWindow != null)
            {
                dialogueWindow.ShopRequested -= shopWindow.Open;
                dialogueWindow.ShopRequested += shopWindow.Open;
            }

            if (dialogueWindow != null && questManager != null)
            {
                dialogueWindow.QuestAcceptRequested -= questManager.AcceptQuest;
                dialogueWindow.QuestAcceptRequested += questManager.AcceptQuest;
            }

            if (dialogueWindow != null && questBoardWindow != null)
            {
                dialogueWindow.QuestBoardRequested -= questBoardWindow.Open;
                dialogueWindow.QuestBoardRequested += questBoardWindow.Open;
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