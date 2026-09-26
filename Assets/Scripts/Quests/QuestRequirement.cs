using System;
using Project.Character.Movement;
using Project.Items;

namespace Project.Quests
{
    /// <summary>
    /// The player-side references a <see cref="QuestRequirement"/> may need
    /// to create its <see cref="IQuestRequirementTracker"/>. Not every
    /// requirement type uses every field — e.g. a kill-creature requirement
    /// needs neither — so this stays a plain data carrier rather than an
    /// interface each requirement would have to partially implement.
    /// </summary>
    public readonly struct QuestRequirementContext
    {
        /// <summary>Gets the player's inventory, used by item-based requirements.</summary>
        public PlayerInventory PlayerInventory { get; }

        /// <summary>Gets the player's NPC interaction controller, used by talk-to-NPC requirements.</summary>
        public PlayerNpcInteractionController NpcInteraction { get; }

        /// <param name="playerInventory">The player's inventory.</param>
        /// <param name="npcInteraction">The player's NPC interaction controller.</param>
        public QuestRequirementContext(PlayerInventory playerInventory, PlayerNpcInteractionController npcInteraction)
        {
            PlayerInventory = playerInventory;
            NpcInteraction = npcInteraction;
        }
    }

    /// <summary>
    /// Watches one accepted quest's requirement for progress. Created by
    /// <see cref="QuestRequirement.CreateTracker"/> when a quest is accepted
    /// (or restored from a save) and disposed by <see cref="QuestManager"/>
    /// once the quest completes, so a requirement's event subscriptions
    /// never outlive the quest they belong to.
    /// </summary>
    public interface IQuestRequirementTracker : IDisposable
    {
        /// <summary>Gets this tracker's progress count, valid at any time (not only right after <see cref="ProgressChanged"/> fires).</summary>
        int CurrentProgress { get; }

        /// <summary>Raised with the new progress count whenever it changes.</summary>
        event Action<int> ProgressChanged;
    }

    /// <summary>
    /// One trackable condition inside a <see cref="QuestDefinition"/> — kill
    /// creatures, collect items, or talk to an NPC today. Implemented as a
    /// ScriptableObject asset so a new objective type can be added later as
    /// its own subclass without <see cref="QuestManager"/> or
    /// <see cref="QuestDefinition"/> ever changing (Open/Closed principle).
    /// Holds only authored data — per-quest runtime progress lives on the
    /// <see cref="IQuestRequirementTracker"/> it creates, never on the asset
    /// itself, since a ScriptableObject asset is shared and must stay
    /// stateless between plays.
    /// </summary>
    public abstract class QuestRequirement : UnityEngine.ScriptableObject
    {
        /// <summary>Gets the number of times this requirement must be satisfied to be complete.</summary>
        public abstract int RequiredCount { get; }

        /// <summary>Gets the quest log line for this requirement at the given progress, e.g. "Poring defeated: 2/5".</summary>
        /// <param name="currentProgress">The progress count to display.</param>
        public abstract string GetProgressText(int currentProgress);

        /// <summary>
        /// Creates a tracker that watches this requirement's condition for
        /// one accepted quest, starting from <paramref name="initialProgress"/>
        /// — zero for a freshly accepted quest, or a restored count when
        /// loading a save. A requirement whose progress is derived from
        /// live state instead of an accumulated count (e.g. "how many of
        /// this item do I currently hold") is free to ignore the seed and
        /// recompute it instead, which is strictly more correct there.
        /// </summary>
        /// <param name="context">The player-side references this tracker may need.</param>
        /// <param name="initialProgress">The progress count to start from.</param>
        public abstract IQuestRequirementTracker CreateTracker(QuestRequirementContext context, int initialProgress);
    }
}
