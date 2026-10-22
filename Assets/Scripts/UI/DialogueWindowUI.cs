using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Character.Combat;
using Project.Character.Movement;
using Project.Character.Stats;
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

        [Header("Class promotion")]
        [SerializeField] private QuestManager questManager;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerJobProgress jobProgress;

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

        /// <summary>
        /// Raised when the player picks an <see cref="NpcDialogueOptionAction.OpenQuestBoard"/>
        /// option, carrying the board to show. <see cref="PlayerUIController"/>
        /// wires this directly to <see cref="QuestBoardWindowUI.Open"/>, the
        /// same direct-wiring reasoning as this class's own summary. Not
        /// raised if the NPC has no <see cref="QuestBoardNpc"/>.
        /// </summary>
        public event Action<QuestBoardNpc> QuestBoardRequested;

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

            var options = dialogue != null ? dialogue.Options : Array.Empty<NpcDialogueOption>();
            BuildOptions(options.Select(option => (option.OptionText, (Action)(() => HandleOptionClicked(option)))));
        }

        /// <summary>Closes this window, if open.</summary>
        public void Close()
        {
            gameObject.SetActive(false);
            Closed?.Invoke();
        }

        private void BuildOptions(IEnumerable<(string text, Action onClick)> options)
        {
            foreach (var view in optionViews)
            {
                Destroy(view.gameObject);
            }

            optionViews.Clear();

            foreach (var option in options)
            {
                var view = Instantiate(optionButtonPrefab, optionsParent);
                view.Setup(option.text, option.onClick);
                optionViews.Add(view);
            }
        }

        private void HandleOptionClicked(NpcDialogueOption option)
        {
            if (option.Action == NpcDialogueOptionAction.ClassPromotion && currentNpc.GetComponent<PromotionNpc>() != null)
            {
                ShowPromotion(currentNpc.GetComponent<PromotionNpc>());
                return;
            }

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
            else if (option.Action == NpcDialogueOptionAction.OpenQuestBoard)
            {
                var board = currentNpc.GetComponent<QuestBoardNpc>();

                if (board != null)
                {
                    QuestBoardRequested?.Invoke(board);
                }
            }
        }

        /// <summary>
        /// Shows the trial master's conversation for the player's current trial stage.
        /// </summary>
        /// <param name="npc">The trial master being talked to.</param>
        private void ShowPromotion(PromotionNpc npc)
        {
            var leave = ("Leave.", (Action)Close);
            var completed = npc.Catalog.FindTrial(questManager.IsCompleted);
            var active = npc.Catalog.FindTrial(questManager.IsActive);
            var stage = ClassTrialRules.Resolve(
                classController.CurrentClass == CharacterClass.Novice,
                completed != null,
                active != null,
                classController.CanEvolve);

            switch (stage)
            {
                case ClassTrialStage.AlreadyPromoted:
                    ShowDynamic("You have already completed the trial. Go now and seek new adventures.", leave);
                    break;
                case ClassTrialStage.ClaimPromotion:
                    ShowDynamic($"You have completed the trial of the {completed.TargetClass}. Turn it in and your new path begins.",
                        ("Turn in the trial.", () => ConfirmClass(completed.TargetClass)), leave);
                    break;
                case ClassTrialStage.TrialActive:
                    ShowDynamic($"{active.Quest.Title}\n\n{active.Quest.Description}",
                        ("Abandon the trial.", () => AbandonTrial(npc, active)), leave);
                    break;
                case ClassTrialStage.NotReady:
                    ShowDynamic(GetNotReadyLine(), leave);
                    break;
                default:
                    ShowTrialChoices(npc, leave);
                    break;
            }
        }

        /// <summary>
        /// Gets the line explaining why the Novice cannot start a trial yet.
        /// </summary>
        /// <returns>The line to show.</returns>
        private string GetNotReadyLine()
        {
            return jobProgress.JobLevel < ClassProgression.EvolutionJobLevel
                ? $"Come back when you reach Job Level {ClassProgression.EvolutionJobLevel}."
                : "Spend every skill point on Novice skills before taking a trial.";
        }

        /// <summary>
        /// Lists one trial per class, plus a way to leave.
        /// </summary>
        /// <param name="npc">The trial master being talked to.</param>
        /// <param name="leave">The option that closes the window.</param>
        private void ShowTrialChoices(PromotionNpc npc, (string, Action) leave)
        {
            var choices = npc.Catalog.Trials
                .Select(trial => ($"Trial of the {trial.TargetClass}.", (Action)(() => ShowTrialOffer(npc, trial))))
                .Append(leave)
                .ToArray();

            ShowDynamic("Choose the class you wish to become. You can hold only one trial at a time.", choices);
        }

        /// <summary>
        /// Describes one trial and lets the player accept it or go back.
        /// </summary>
        /// <param name="npc">The trial master being talked to.</param>
        /// <param name="trial">The trial being offered.</param>
        private void ShowTrialOffer(PromotionNpc npc, ClassTrial trial)
        {
            ShowDynamic(trial.Quest.Description,
                ("Accept the trial.", () =>
                {
                    questManager.AcceptQuest(trial.Quest);
                    Close();
                }),
                ("Back.", () => ShowPromotion(npc)));
        }

        /// <summary>
        /// Abandons a trial, which discards its progress, and shows the options again.
        /// </summary>
        /// <param name="npc">The trial master being talked to.</param>
        /// <param name="trial">The trial to abandon.</param>
        private void AbandonTrial(PromotionNpc npc, ClassTrial trial)
        {
            questManager.AbandonQuest(trial.Quest);
            ShowPromotion(npc);
        }

        /// <summary>
        /// Evolves the player into the chosen class and closes the window.
        /// </summary>
        /// <param name="target">The class to become.</param>
        private void ConfirmClass(CharacterClass target)
        {
            classController.TryEvolve(target);
            Close();
        }

        private void ShowDynamic(string line, params (string text, Action onClick)[] options)
        {
            if (lineText != null)
            {
                lineText.text = line;
            }

            BuildOptions(options);
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
