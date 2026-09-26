using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Project.Character.Combat;
using Project.Character.Movement;
using Project.Items;
using Project.Persistence;

namespace Project.Quests
{
    /// <summary>
    /// Owns every quest the player has accepted or completed. Creates one
    /// <see cref="IQuestRequirementTracker"/> per requirement on accept —
    /// via <see cref="QuestRequirement.CreateTracker"/>, so this class never
    /// needs to know how any particular objective type is satisfied — and
    /// disposes them on completion. Grants <see cref="QuestDefinition"/>'s
    /// reward on completion and persists progress via
    /// <see cref="ISaveParticipant"/>, mirroring <see cref="PlayerInventory"/>.
    /// </summary>
    public class QuestManager : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private QuestDatabase questDatabase;
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private PlayerNpcInteractionController npcInteractionController;
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private PlayerJobProgress playerJobProgress;

        private readonly List<ActiveQuest> activeQuests = new List<ActiveQuest>();
        private readonly List<QuestDefinition> completedQuests = new List<QuestDefinition>();
        private readonly Dictionary<ActiveQuest, IQuestRequirementTracker[]> trackersByQuest = new Dictionary<ActiveQuest, IQuestRequirementTracker[]>();

        /// <summary>Raised whenever a quest is accepted, its progress changes, or it completes.</summary>
        public event Action QuestsChanged;

        /// <summary>Gets every quest currently accepted and in progress.</summary>
        public IReadOnlyList<ActiveQuest> ActiveQuests => activeQuests;

        /// <summary>Gets every quest already completed.</summary>
        public IReadOnlyList<QuestDefinition> CompletedQuests => completedQuests;

        /// <summary>Gets whether the given quest is currently active.</summary>
        /// <param name="quest">The quest to check.</param>
        public bool IsActive(QuestDefinition quest) => activeQuests.Any(active => active.Definition == quest);

        /// <summary>Gets whether the given quest has already been completed.</summary>
        /// <param name="quest">The quest to check.</param>
        public bool IsCompleted(QuestDefinition quest) => completedQuests.Contains(quest);

        /// <summary>
        /// Accepts a quest, starting progress tracking for each of its
        /// requirements. Does nothing if <paramref name="quest"/> is null or
        /// already accepted or completed.
        /// </summary>
        /// <param name="quest">The quest to accept.</param>
        public void AcceptQuest(QuestDefinition quest)
        {
            AcceptQuest(quest, null);
        }

        private void AcceptQuest(QuestDefinition quest, int[] initialProgress)
        {
            if (quest == null || IsActive(quest) || IsCompleted(quest))
            {
                return;
            }

            var active = new ActiveQuest(quest, initialProgress);
            var context = new QuestRequirementContext(playerInventory, npcInteractionController);
            var requirements = quest.Requirements;
            var trackers = new IQuestRequirementTracker[requirements.Count];

            for (var i = 0; i < requirements.Count; i++)
            {
                var requirementIndex = i;
                var tracker = requirements[i].CreateTracker(context, active.GetProgress(i));
                tracker.ProgressChanged += progress => HandleProgress(active, requirementIndex, progress);
                trackers[i] = tracker;

                // Primes progress derived from live state (e.g. items already
                // held) instead of only reacting to the next change.
                active.SetProgress(requirementIndex, tracker.CurrentProgress);
            }

            trackersByQuest[active] = trackers;
            activeQuests.Add(active);

            if (active.IsComplete)
            {
                CompleteQuest(active);
            }

            QuestsChanged?.Invoke();
        }

        private void HandleProgress(ActiveQuest active, int requirementIndex, int progress)
        {
            active.SetProgress(requirementIndex, progress);
            QuestsChanged?.Invoke();

            if (active.IsComplete)
            {
                CompleteQuest(active);
            }
        }

        private void CompleteQuest(ActiveQuest active)
        {
            StopTracking(active);
            activeQuests.Remove(active);
            completedQuests.Add(active.Definition);

            var definition = active.Definition;
            playerExperience?.AddExperience(definition.RewardExperience);
            playerJobProgress?.AddExperience(definition.RewardJobExperience);

            if (definition.RewardItem != null)
            {
                playerInventory?.Items.TryAddItem(definition.RewardItem, definition.RewardItemQuantity);
            }

            QuestsChanged?.Invoke();
        }

        private void StopTracking(ActiveQuest active)
        {
            if (!trackersByQuest.TryGetValue(active, out var trackers))
            {
                return;
            }

            foreach (var tracker in trackers)
            {
                tracker.Dispose();
            }

            trackersByQuest.Remove(active);
        }

        private void OnDestroy()
        {
            foreach (var active in activeQuests.ToArray())
            {
                StopTracking(active);
            }
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.activeQuestIds.Clear();
            data.activeQuestProgress.Clear();

            foreach (var active in activeQuests)
            {
                data.activeQuestIds.Add(questDatabase.GetId(active.Definition));
                data.activeQuestProgress.Add(string.Join(",", active.CopyProgress()));
            }

            data.completedQuestIds.Clear();

            foreach (var quest in completedQuests)
            {
                data.completedQuestIds.Add(questDatabase.GetId(quest));
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            completedQuests.Clear();

            foreach (var id in data.completedQuestIds)
            {
                var quest = questDatabase.FindById(id);

                if (quest != null)
                {
                    completedQuests.Add(quest);
                }
            }

            for (var i = 0; i < data.activeQuestIds.Count; i++)
            {
                var quest = questDatabase.FindById(data.activeQuestIds[i]);

                if (quest == null)
                {
                    continue;
                }

                var progressText = i < data.activeQuestProgress.Count ? data.activeQuestProgress[i] : string.Empty;
                AcceptQuest(quest, ParseProgress(progressText));
            }

            QuestsChanged?.Invoke();
        }

        private static int[] ParseProgress(string progressText)
        {
            if (string.IsNullOrEmpty(progressText))
            {
                return Array.Empty<int>();
            }

            var parts = progressText.Split(',');
            var result = new int[parts.Length];

            for (var i = 0; i < parts.Length; i++)
            {
                int.TryParse(parts[i], out result[i]);
            }

            return result;
        }
    }
}
