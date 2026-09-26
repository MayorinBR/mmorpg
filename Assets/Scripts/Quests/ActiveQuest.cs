using System;

namespace Project.Quests
{
    /// <summary>
    /// Runtime progress for one quest a player has accepted: one progress
    /// count per entry in <see cref="QuestDefinition.Requirements"/>, each
    /// capped at that requirement's own <see cref="QuestRequirement.RequiredCount"/>.
    /// Plain data — <see cref="QuestManager"/> is what updates it as the
    /// quest's <see cref="IQuestRequirementTracker"/>s report progress.
    /// </summary>
    public class ActiveQuest
    {
        private readonly int[] progress;

        /// <summary>Gets the quest this progress belongs to.</summary>
        public QuestDefinition Definition { get; }

        /// <param name="definition">The quest this instance tracks.</param>
        /// <param name="initialProgress">
        /// Per-requirement starting progress, e.g. restored from a save.
        /// Left null for a freshly accepted quest, which starts every
        /// requirement at zero.
        /// </param>
        public ActiveQuest(QuestDefinition definition, int[] initialProgress = null)
        {
            Definition = definition;
            progress = new int[definition.Requirements.Count];

            if (initialProgress == null)
            {
                return;
            }

            for (var i = 0; i < progress.Length && i < initialProgress.Length; i++)
            {
                progress[i] = initialProgress[i];
            }
        }

        /// <summary>Gets whether every requirement has reached its required count.</summary>
        public bool IsComplete
        {
            get
            {
                var requirements = Definition.Requirements;

                for (var i = 0; i < requirements.Count; i++)
                {
                    if (progress[i] < requirements[i].RequiredCount)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        /// <summary>Gets the current progress count for the requirement at <paramref name="requirementIndex"/>.</summary>
        /// <param name="requirementIndex">Index into <see cref="QuestDefinition.Requirements"/>.</param>
        public int GetProgress(int requirementIndex) => progress[requirementIndex];

        /// <summary>Sets the progress count for one requirement, capped at its own required count.</summary>
        /// <param name="requirementIndex">Index into <see cref="QuestDefinition.Requirements"/>.</param>
        /// <param name="value">The new progress count.</param>
        public void SetProgress(int requirementIndex, int value)
        {
            progress[requirementIndex] = Math.Min(value, Definition.Requirements[requirementIndex].RequiredCount);
        }

        /// <summary>Gets a snapshot of every requirement's current progress, for saving.</summary>
        public int[] CopyProgress() => (int[])progress.Clone();
    }
}
