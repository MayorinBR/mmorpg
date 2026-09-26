using System.Collections.Generic;
using UnityEngine;
using Project.Character.Movement;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// Lists every unfinished quest posted on a <see cref="QuestBoardNpc"/>,
    /// each with its own Accept button. Opened directly by
    /// <see cref="PlayerUIController"/> from
    /// <see cref="DialogueWindowUI.QuestBoardRequested"/> rather than
    /// subscribing itself — same reasoning as <see cref="ShopWindowUI"/>:
    /// this window's GameObject starts inactive, so it could never register
    /// that subscription on its own. Rebuilds its list on open and again on
    /// every <see cref="QuestManager.QuestsChanged"/> while open, so
    /// accepting one posting removes it from the list immediately without
    /// needing to reopen the board.
    /// </summary>
    public class QuestBoardWindowUI : MonoBehaviour
    {
        [SerializeField] private WindowPanel windowPanel;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private QuestManager questManager;
        [SerializeField] private Transform entryParent;

        private readonly List<QuestBoardEntryUI> entryViews = new List<QuestBoardEntryUI>();
        private QuestBoardNpc currentBoard;

        private void Awake()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
                movementController.MovementStarted += HandleMovementStarted;
            }

            if (windowPanel != null)
            {
                windowPanel.Closed -= HandleClosed;
                windowPanel.Closed += HandleClosed;
            }
        }

        private void OnDestroy()
        {
            if (movementController != null)
            {
                movementController.MovementStarted -= HandleMovementStarted;
            }

            if (windowPanel != null)
            {
                windowPanel.Closed -= HandleClosed;
            }

            if (questManager != null)
            {
                questManager.QuestsChanged -= Rebuild;
            }
        }

        /// <summary>
        /// Opens this window for the given board. Called directly by
        /// <see cref="PlayerUIController"/> — see the class summary for why
        /// this can't be a self-registered event subscription instead.
        /// </summary>
        /// <param name="board">The board whose posted quests to show.</param>
        public void Open(QuestBoardNpc board)
        {
            currentBoard = board;
            windowPanel.Open();

            questManager.QuestsChanged -= Rebuild;
            questManager.QuestsChanged += Rebuild;

            Rebuild();
        }

        /// <summary>Closes this window, if open.</summary>
        public void Close()
        {
            windowPanel.Close();
        }

        private void Rebuild()
        {
            foreach (var view in entryViews)
            {
                Destroy(view.gameObject);
            }

            entryViews.Clear();

            if (currentBoard == null)
            {
                return;
            }

            foreach (var quest in currentBoard.OfferedQuests)
            {
                if (quest == null || questManager.IsActive(quest) || questManager.IsCompleted(quest))
                {
                    continue;
                }

                var view = QuestBoardEntryUI.Create(entryParent, quest, HandleAcceptClicked);
                entryViews.Add(view);
            }
        }

        private void HandleAcceptClicked(QuestDefinition quest)
        {
            questManager.AcceptQuest(quest);
        }

        private void HandleClosed()
        {
            questManager.QuestsChanged -= Rebuild;
        }

        private void HandleMovementStarted()
        {
            if (windowPanel.IsOpen)
            {
                Close();
            }
        }
    }
}
