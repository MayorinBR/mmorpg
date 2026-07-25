using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks which skills the player has learned and their current level.
    /// Learning or upgrading a skill spends one point from
    /// <see cref="PlayerJobProgress"/> and respects the skill's class restriction.
    /// </summary>
    public class PlayerSkillBook : MonoBehaviour
    {
        [SerializeField] private PlayerJobProgress jobProgress;
        [SerializeField] private PlayerClassController classController;

        private readonly Dictionary<SkillDefinition, int> skillLevels = new Dictionary<SkillDefinition, int>();

        /// <summary>Raised whenever a skill is learned or leveled up, with the skill and its new level.</summary>
        public event Action<SkillDefinition, int> SkillLeveledUp;

        /// <summary>
        /// Gets the current level of a skill, or 0 if it hasn't been learned yet.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>The skill's current level.</returns>
        public int GetLevel(SkillDefinition skill)
        {
            return skillLevels.TryGetValue(skill, out var level) ? level : 0;
        }

        /// <summary>
        /// Attempts to learn (if unlearned) or upgrade (if already learned) a skill by one level.
        /// </summary>
        /// <param name="skill">The skill to learn or upgrade.</param>
        /// <returns>True if the skill was learned/upgraded; false if the class doesn't allow it, it's already at max level, or no skill points are available.</returns>
        public bool TryLearnOrUpgrade(SkillDefinition skill)
        {
            if (skill.AllowedClasses.Count > 0 && !skill.AllowedClasses.Contains(classController.CurrentClass))
            {
                return false;
            }

            var currentLevel = GetLevel(skill);

            if (currentLevel >= skill.MaxLevel)
            {
                return false;
            }

            if (!jobProgress.TrySpendSkillPoint())
            {
                return false;
            }

            var newLevel = currentLevel + 1;
            skillLevels[skill] = newLevel;
            SkillLeveledUp?.Invoke(skill, newLevel);
            return true;
        }
    }
}