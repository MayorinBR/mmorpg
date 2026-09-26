using System;
using UnityEngine;
using Project.Character.Movement;
using Project.NPC;

namespace Project.Quests
{
    /// <summary>
    /// Satisfied by opening dialogue with a specific NPC — covers the
    /// backlog's "travel to a location and talk to an NPC" quest type,
    /// since reaching interaction range is already required to open an
    /// NPC's dialogue in the first place (see
    /// <see cref="PlayerNpcInteractionController"/>).
    /// </summary>
    [CreateAssetMenu(fileName = "NewTalkToNpcRequirement", menuName = "Project/Quests/Requirements/Talk To NPC")]
    public class TalkToNpcRequirement : QuestRequirement
    {
        [SerializeField] private NpcDialogueDefinition targetDialogue;
        [SerializeField] private string npcDisplayName = "the NPC";

        /// <inheritdoc />
        public override int RequiredCount => 1;

        /// <inheritdoc />
        public override string GetProgressText(int currentProgress) => $"Talk to {npcDisplayName}: {currentProgress}/1";

        /// <inheritdoc />
        public override IQuestRequirementTracker CreateTracker(QuestRequirementContext context, int initialProgress) =>
            new Tracker(context.NpcInteraction, targetDialogue, initialProgress);

        private sealed class Tracker : IQuestRequirementTracker
        {
            private readonly PlayerNpcInteractionController npcInteraction;
            private readonly NpcDialogueDefinition targetDialogue;
            private bool hasTalked;

            /// <inheritdoc />
            public int CurrentProgress => hasTalked ? 1 : 0;

            /// <inheritdoc />
            public event Action<int> ProgressChanged;

            public Tracker(PlayerNpcInteractionController npcInteraction, NpcDialogueDefinition targetDialogue, int initialProgress)
            {
                this.npcInteraction = npcInteraction;
                this.targetDialogue = targetDialogue;
                hasTalked = initialProgress >= 1;

                if (npcInteraction != null)
                {
                    npcInteraction.DialogueOpened += HandleDialogueOpened;
                }
            }

            private void HandleDialogueOpened(NpcDialogueController npc)
            {
                if (hasTalked || npc.Dialogue != targetDialogue)
                {
                    return;
                }

                hasTalked = true;
                ProgressChanged?.Invoke(1);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (npcInteraction != null)
                {
                    npcInteraction.DialogueOpened -= HandleDialogueOpened;
                }
            }
        }
    }
}
