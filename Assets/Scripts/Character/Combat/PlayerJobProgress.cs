using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks the player's job level and experience, separate from base
    /// level. Job level ups grant skill points, spent via
    /// <see cref="TrySpendSkillPoint"/> by <see cref="PlayerSkillBook"/> to
    /// learn or upgrade skills.
    /// </summary>
    public class PlayerJobProgress : MonoBehaviour
    {
        [SerializeField] private int startingJobLevel = 1;
        [SerializeField] private int skillPointsPerJobLevel = 1;

        private IJobExperienceCurve experienceCurve;
        private int currentExperience;

        /// <summary>Raised whenever job experience changes, with (currentExperience, requiredForNextLevel).</summary>
        public event Action<int, int> JobExperienceChanged;

        /// <summary>Raised whenever the player's job level increases, with the new job level.</summary>
        public event Action<int> JobLeveledUp;

        /// <summary>Gets the player's current job level.</summary>
        public int JobLevel { get; private set; }

        /// <summary>Gets the current accumulated job experience toward the next job level.</summary>
        public int CurrentExperience => currentExperience;

        /// <summary>Gets the job experience required to advance from the current job level.</summary>
        public int RequiredExperienceForNextLevel => experienceCurve.GetRequiredExperience(JobLevel);

        /// <summary>Gets the number of unspent skill points.</summary>
        public int AvailableSkillPoints { get; private set; }

        /// <summary>Raised whenever a skill point is spent.</summary>
        public event Action SkillPointSpent;

        /// <summary>
        /// Attempts to spend one skill point, typically to learn or upgrade a skill.
        /// </summary>
        /// <returns>True if a point was available and spent; false otherwise.</returns>
        public bool TrySpendSkillPoint()
        {
            if (AvailableSkillPoints <= 0)
            {
                return false;
            }

            AvailableSkillPoints--;
            SkillPointSpent?.Invoke();
            return true;
        }

        private void Awake()
        {
            JobLevel = startingJobLevel;
            experienceCurve = new LinearJobExperienceCurve();
        }

        private void Start()
        {
            JobExperienceChanged?.Invoke(currentExperience, experienceCurve.GetRequiredExperience(JobLevel));
        }

        /// <summary>
        /// Adds job experience, applying as many job level ups as the amount allows.
        /// </summary>
        /// <param name="amount">The job experience amount to add. Non-positive values are ignored.</param>
        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentExperience += amount;
            var requiredForNextLevel = experienceCurve.GetRequiredExperience(JobLevel);

            while (currentExperience >= requiredForNextLevel)
            {
                currentExperience -= requiredForNextLevel;
                JobLevel++;
                AvailableSkillPoints += skillPointsPerJobLevel;
                JobLeveledUp?.Invoke(JobLevel);
                requiredForNextLevel = experienceCurve.GetRequiredExperience(JobLevel);
            }

            JobExperienceChanged?.Invoke(currentExperience, requiredForNextLevel);
        }
    }
}