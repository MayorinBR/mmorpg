using System;
using UnityEngine;
using Project.Character.Movement;
using Project.Items;
using Project.NPC;

namespace Project.Quests
{
    /// <summary>
    /// Satisfied by opening dialogue with a specific NPC while still holding
    /// a specific item — the classic courier objective: an NPC hands the
    /// player a unique key item on quest accept (see
    /// <see cref="QuestDefinition.GrantItemOnAccept"/>), and this requirement
    /// completes only once the player reaches the delivery NPC with that
    /// item still in inventory. The item is removed from inventory the
    /// moment delivery is recognized, mirroring a real hand-off rather than
    /// leaving it behind for the player to keep. Talking to the delivery NPC
    /// without the item does nothing, so the player can freely browse
    /// dialogue before actually completing the delivery.
    /// </summary>
    [CreateAssetMenu(fileName = "NewDeliverItemRequirement", menuName = "Project/Quests/Requirements/Deliver Item")]
    public class DeliverItemRequirement : QuestRequirement
    {
        [SerializeField] private ItemDefinition itemToDeliver;
        [SerializeField] private NpcDialogueDefinition deliverToDialogue;
        [SerializeField] private string npcDisplayName = "the NPC";

        /// <inheritdoc />
        public override int RequiredCount => 1;

        /// <inheritdoc />
        public override string GetProgressText(int currentProgress) => $"Deliver {itemToDeliver.ItemName} to {npcDisplayName}: {currentProgress}/1";

        /// <inheritdoc />
        public override IQuestRequirementTracker CreateTracker(QuestRequirementContext context, int initialProgress) =>
            new Tracker(context.PlayerInventory, context.NpcInteraction, itemToDeliver, deliverToDialogue, initialProgress);

        private sealed class Tracker : IQuestRequirementTracker
        {
            private readonly PlayerInventory playerInventory;
            private readonly PlayerNpcInteractionController npcInteraction;
            private readonly ItemDefinition itemToDeliver;
            private readonly NpcDialogueDefinition deliverToDialogue;
            private bool delivered;

            /// <inheritdoc />
            public int CurrentProgress => delivered ? 1 : 0;

            /// <inheritdoc />
            public event Action<int> ProgressChanged;

            public Tracker(PlayerInventory playerInventory, PlayerNpcInteractionController npcInteraction, ItemDefinition itemToDeliver, NpcDialogueDefinition deliverToDialogue, int initialProgress)
            {
                this.playerInventory = playerInventory;
                this.npcInteraction = npcInteraction;
                this.itemToDeliver = itemToDeliver;
                this.deliverToDialogue = deliverToDialogue;
                delivered = initialProgress >= 1;

                if (npcInteraction != null)
                {
                    npcInteraction.DialogueOpened += HandleDialogueOpened;
                }
            }

            private void HandleDialogueOpened(NpcDialogueController npc)
            {
                if (delivered || npc.Dialogue != deliverToDialogue)
                {
                    return;
                }

                if (playerInventory == null || !playerInventory.Items.TryRemoveItem(itemToDeliver, 1))
                {
                    return;
                }

                delivered = true;
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
