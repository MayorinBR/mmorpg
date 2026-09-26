using System;
using UnityEngine;
using Project.Items;

namespace Project.Quests
{
    /// <summary>
    /// Satisfied by holding a given quantity of an item in the player's
    /// inventory. Progress reflects the current quantity owned rather than
    /// a cumulative "picked up" counter, matching classic Ragnarok Online
    /// quest items — it self-heals if items are lost or sold between
    /// sessions instead of trusting a stale accumulated count.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCollectItemRequirement", menuName = "Project/Quests/Requirements/Collect Item")]
    public class CollectItemRequirement : QuestRequirement
    {
        [SerializeField] private ItemDefinition targetItem;
        [SerializeField] private int requiredCount = 1;

        /// <inheritdoc />
        public override int RequiredCount => requiredCount;

        /// <inheritdoc />
        public override string GetProgressText(int currentProgress) => $"Collect {targetItem.ItemName}: {currentProgress}/{requiredCount}";

        /// <inheritdoc />
        public override IQuestRequirementTracker CreateTracker(QuestRequirementContext context, int initialProgress) =>
            new Tracker(context.PlayerInventory, targetItem, requiredCount);

        private sealed class Tracker : IQuestRequirementTracker
        {
            private readonly PlayerInventory playerInventory;
            private readonly ItemDefinition targetItem;
            private readonly int requiredCount;

            /// <inheritdoc />
            // ponytail: recomputes on every inventory change rather than only when
            // targetItem's own quantity changes — fine at prototype inventory sizes,
            // revisit if a large inventory makes this noticeably hot.
            public int CurrentProgress => playerInventory != null ? Math.Min(playerInventory.Items.GetQuantity(targetItem), requiredCount) : 0;

            /// <inheritdoc />
            public event Action<int> ProgressChanged;

            public Tracker(PlayerInventory playerInventory, ItemDefinition targetItem, int requiredCount)
            {
                this.playerInventory = playerInventory;
                this.targetItem = targetItem;
                this.requiredCount = requiredCount;

                if (playerInventory != null)
                {
                    playerInventory.Items.InventoryChanged += HandleInventoryChanged;
                }
            }

            private void HandleInventoryChanged()
            {
                ProgressChanged?.Invoke(CurrentProgress);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                if (playerInventory != null)
                {
                    playerInventory.Items.InventoryChanged -= HandleInventoryChanged;
                }
            }
        }
    }
}
