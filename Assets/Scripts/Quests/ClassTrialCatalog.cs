using System;
using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Quests
{
    /// <summary>
    /// The list of class trials, one per first class. Lives in an asset, not on
    /// the NPC, so the player can be promoted when a trial completes even while
    /// the trial master's scene is not loaded.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassTrialCatalog", menuName = "Project/Quests/Class Trial Catalog")]
    public class ClassTrialCatalog : ScriptableObject
    {
        [SerializeField] private ClassTrial[] trials;

        /// <summary>Gets every class trial, in the order the NPC lists them.</summary>
        public IReadOnlyList<ClassTrial> Trials => trials ?? Array.Empty<ClassTrial>();

        /// <summary>
        /// Finds the first trial whose quest satisfies a condition.
        /// </summary>
        /// <param name="matches">The condition tested against each trial's quest.</param>
        /// <returns>The matching trial, or null if none matches.</returns>
        public ClassTrial FindTrial(Func<QuestDefinition, bool> matches)
        {
            foreach (var trial in Trials)
            {
                if (trial != null && trial.Quest != null && matches(trial.Quest))
                {
                    return trial;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the class a trial quest promotes to.
        /// </summary>
        /// <param name="quest">The quest to look up.</param>
        /// <param name="targetClass">The class the quest promotes to, when found.</param>
        /// <returns>True if <paramref name="quest"/> is one of the class trials.</returns>
        public bool TryGetTargetClass(QuestDefinition quest, out CharacterClass targetClass)
        {
            var trial = FindTrial(candidate => candidate == quest);
            targetClass = trial != null ? trial.TargetClass : default;
            return trial != null;
        }
    }
}
