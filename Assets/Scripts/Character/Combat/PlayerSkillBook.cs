using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Persistence;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks which skills the player has learned and their current level.
    /// Learning or upgrading a skill spends one point from
    /// <see cref="PlayerJobProgress"/> and respects the skill's class
    /// restriction and minimum level requirement.
    /// </summary>
    public class PlayerSkillBook : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private PlayerJobProgress jobProgress;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private SkillDatabase skillDatabase;

        [Tooltip("Optional. Source of the player's Base Level, used to enforce SkillDefinition.RequiredLevel. Left empty, skills can be learned regardless of level.")]
        [SerializeField] private MonoBehaviour playerStatsSource;

        private IPlayerLevelProvider levelProvider;
        private readonly Dictionary<SkillDefinition, int> skillLevels = new Dictionary<SkillDefinition, int>();

        private void Awake()
        {
            levelProvider = playerStatsSource as IPlayerLevelProvider;
        }

        /// <summary>Raised whenever a skill is learned or leveled up, with the skill and its new level.</summary>
        public event Action<SkillDefinition, int> SkillLeveledUp;

        /// <summary>
        /// Gets a read-only view of every skill the player has learned,
        /// keyed by skill with its current level. Used by
        /// <see cref="PlayerPassiveSkillController"/> to sum passive
        /// bonuses without needing its own separate list of known skills.
        /// </summary>
        public IReadOnlyDictionary<SkillDefinition, int> LearnedSkills => skillLevels;

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
        /// <returns>True if the skill was learned/upgraded; false if the class doesn't allow it, the level requirement isn't met, it's already at max level, or no skill points are available.</returns>
        public bool TryLearnOrUpgrade(SkillDefinition skill)
        {
            if (levelProvider != null && levelProvider.BaseLevel < skill.RequiredLevel)
            {
                PlayerFeedbackChannel.Publish($"Level {skill.RequiredLevel} required to learn {skill.SkillName}.");
                return false;
            }

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

        /// <summary>
        /// Unlearns every skill. Does not refund the skill points spent
        /// learning them — call <see cref="PlayerJobProgress.ResetProgress"/>
        /// to also reset available skill points. Raises
        /// <see cref="SkillLeveledUp"/> for each previously learned skill so
        /// an open Skill Book panel (and any derived stat display) refreshes
        /// immediately.
        /// </summary>
        public void ResetLearnedSkills()
        {
            var previouslyLearned = new List<SkillDefinition>(skillLevels.Keys);
            skillLevels.Clear();

            foreach (var skill in previouslyLearned)
            {
                SkillLeveledUp?.Invoke(skill, 0);
            }
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.learnedSkillIds.Clear();
            data.learnedSkillLevels.Clear();

            if (skillDatabase == null)
            {
                return;
            }

            foreach (var entry in skillLevels)
            {
                data.learnedSkillIds.Add(skillDatabase.GetId(entry.Key));
                data.learnedSkillLevels.Add(entry.Value);
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            skillLevels.Clear();

            if (skillDatabase == null)
            {
                return;
            }

            for (var i = 0; i < data.learnedSkillIds.Count; i++)
            {
                var skill = skillDatabase.FindById(data.learnedSkillIds[i]);

                if (skill == null)
                {
                    continue;
                }

                skillLevels[skill] = data.learnedSkillLevels[i];
                SkillLeveledUp?.Invoke(skill, data.learnedSkillLevels[i]);
            }
        }
    }
}
