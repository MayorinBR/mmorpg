using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Quests
{
    /// <summary>
    /// Satisfied by defeating a given number of a specific species,
    /// identified by its <see cref="CharacterStatsDefinition"/>. Also covers
    /// the backlog's "kill a boss" quest type — a boss is just a species
    /// with <see cref="RequiredCount"/> set to 1 — so no separate boss
    /// requirement class exists.
    /// </summary>
    [CreateAssetMenu(fileName = "NewKillCreatureRequirement", menuName = "Project/Quests/Requirements/Kill Creature")]
    public class KillCreatureRequirement : QuestRequirement
    {
        [SerializeField] private CharacterStatsDefinition targetSpecies;
        [SerializeField] private string displayName = "creature";
        [SerializeField] private int requiredCount = 1;

        /// <inheritdoc />
        public override int RequiredCount => requiredCount;

        /// <inheritdoc />
        public override string GetProgressText(int currentProgress) => $"Defeat {displayName}: {currentProgress}/{requiredCount}";

        /// <inheritdoc />
        public override IQuestRequirementTracker CreateTracker(QuestRequirementContext context, int initialProgress) =>
            new Tracker(targetSpecies, requiredCount, initialProgress);

        private sealed class Tracker : IQuestRequirementTracker
        {
            private readonly CharacterStatsDefinition targetSpecies;
            private readonly int requiredCount;
            private int killCount;

            /// <inheritdoc />
            public int CurrentProgress => killCount;

            /// <inheritdoc />
            public event Action<int> ProgressChanged;

            public Tracker(CharacterStatsDefinition targetSpecies, int requiredCount, int initialProgress)
            {
                this.targetSpecies = targetSpecies;
                this.requiredCount = requiredCount;
                killCount = initialProgress;
                EnemyDeathEvents.EnemyKilled += HandleEnemyKilled;
            }

            private void HandleEnemyKilled(CharacterStatsDefinition species)
            {
                if (species != targetSpecies || killCount >= requiredCount)
                {
                    return;
                }

                killCount++;
                ProgressChanged?.Invoke(killCount);
            }

            /// <inheritdoc />
            public void Dispose()
            {
                EnemyDeathEvents.EnemyKilled -= HandleEnemyKilled;
            }
        }
    }
}
