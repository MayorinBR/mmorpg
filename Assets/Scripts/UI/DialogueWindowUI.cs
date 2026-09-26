using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Character.Movement;
using Project.NPC;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// Shows an NPC's dialogue line, its optional portrait, and a button per
    /// response option. Unlike <see cref="ShopWindowUI"/> and the other
    /// windows, this one does not go through <see cref="WindowPanel"/>: it
    /// always opens at its authored (centered) position and never persists
    /// open/closed state or position between sessions, so it manages its own
    /// GameObject's active state directly instead. <see cref="Open"/> and
    /// <see cref="Close"/> are called directly by
    /// <see cref="PlayerUIController"/> rather than this component
    /// subscribing to <see cref="PlayerNpcInteractionController.DialogueOpened"/>
    /// itself — same reasoning as <see cref="ShopWindowUI"/>: this window's
    /// GameObject starts inactive, and Unity never runs Awake on an
    /// inactive GameObject, so it could never register that subscription on
    /// its own. Closes itself automatically as soon as the player starts
    /// moving again, via <see cref="CharacterMovementController.MovementStarted"/>,
    /// the same way <see cref="ShopWindowUI"/> does.
    /// </summary>
    public class DialogueWindowUI : MonoBehaviour
    {
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private Image portraitImage;
        [SerializeField] private TMP_Text lineText;
        [SerializeField] private DialogueOptionButtonUI optionButtonPrefab;
        [SerializeField] private Transform optionsParent;

        private readonly List<DialogueOptionButtonUI> optionViews = new List<DialogueOptionButtonUI>();

        private NpcDialogueController currentNpc;

        /// <summary>Raised whenever this window closes, for any reason.</summary>
        public event Action Closed;

        /// <summary>
        /// Raised when the player picks an <see cref="NpcDialogueOptionAction.OpenShop"/>
        /// option, carrying the shop to open. <see cref="PlayerUIController"/>
        /// wires this directly to <see cref="ShopWindowUI.Open"/>, the same
        /// direct-wiring reasoning as this class's own summary.
        /// </summary>
        public event Action<NpcShopKeeper> ShopRequested;

        /// <summary>
        /// Raised when the player picks an <see cref="NpcDialogueOptionAction.AcceptQuest"/>
        /// option, carrying the quest to accept. <see cref="PlayerUIController"/>
        /// wires this directly to <see cref="QuestManager.AcceptQuest"/>, the
        /// same direct-wiring reasoning as this class's own summary. Not
        /// raised if the NPC has no <see cref="QuestGiverNpc"/>.
        /// </summary>
        public event Action<QuestDefinition> QuestAcceptRequested;

        private void Awake()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
                movementController.MovementStarted += HandleMovementStarted;
            }
        }

        private void OnDestroy()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
            }
        }

        /// <summary>
        /// Opens this window for the given NPC, always at its authored
        /// (centered) position. Called directly by
        /// <see cref="PlayerUIController"/> — see the class summary for why
        /// this can't be a self-registered event subscription instead.
        /// </summary>
        /// <param name="npc">The NPC whose dialogue line and options to show.</param>
        public void Open(NpcDialogueController npc)
        {
            currentNpc = npc;
            var dialogue = npc.Dialogue;

            gameObject.SetActive(true);

            if (lineText != null)
            {
                lineText.text = dialogue != null ? dialogue.Line : string.Empty;
            }

            if (portraitImage != null)
            {
                var portrait = dialogue != null ? dialogue.Portrait : null;
                portraitImage.gameObject.SetActive(portrait != null);
                portraitImage.sprite = portrait;
            }

            BuildOptions(dialogue != null ? dialogue.Options : Array.Empty<NpcDialogueOption>());
        }

        /// <summary>Closes this window, if open.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        private void BuildOptions(NpcDialogueOption[] options)
        {
            foreach (var view in optionViews)
            {
                Destroy(view.gameObject);
            }

            optionViews.Clear();

            foreach (var option in options)
            {
                var view = Instantiate(optionButtonPrefab, optionsParent);
                view.Setup(option.OptionText, () => HandleOptionClicked(option));
                optionViews.Add(view);
            }
        }

        private void HandleOptionClicked(NpcDialogueOption option)
        {
            Close();

            if (option.Action == NpcDialogueOptionAction.OpenShop && currentNpc.ShopKeeper != null)
            {
                ShopRequested?.Invoke(currentNpc.ShopKeeper);
            }
            else if (option.Action == NpcDialogueOptionAction.AcceptQuest)
            {
                var questGiver = currentNpc.GetComponent<QuestGiverNpc>();

                if (questGiver != null && questGiver.OfferedQuest != null)
                {
                    QuestAcceptRequested?.Invoke(questGiver.OfferedQuest);
                }
            }
        }

        private void HandleMovementStarted()
        {
            if (gameObject.activeSelf)
            {
                Close();
            }
        }
    }
}
