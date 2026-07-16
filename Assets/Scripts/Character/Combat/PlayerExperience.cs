using System;
using UnityEngine;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks the player's current experience toward the next level. On
    /// level up, advances <see cref="PlayerStatsController.BaseLevel"/>,
    /// grants stat points via <see cref="PlayerStatsController.BaseStats"/>,
    /// and fully heals the player.
    /// </summary>
    public class PlayerExperience : MonoBehaviour
    {
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private HealthComponent health;
        [SerializeField] private int statPointsPerLevel = 5;

        private IExperienceCurve experienceCurve;
        private int currentExperience;

        /// <summary>Raised whenever experience changes, with (currentExperience, requiredForNextLevel).</summary>
        public event Action<int, int> ExperienceChanged;

        /// <summary>Raised whenever the player levels up, with the new level.</summary>
        public event Action<int> LeveledUp;

        /// <summary>Gets the player's current level.</summary>
        public int CurrentLevel => statsController.BaseLevel;

        /// <summary>Gets the current accumulated experience toward the next level.</summary>
        public int CurrentExperience => currentExperience;

        /// <summary>Gets the experience required to advance from the current level.</summary>
        public int RequiredExperienceForNextLevel => experienceCurve.GetRequiredExperience(CurrentLevel);

        private void Awake()
        {
            experienceCurve = new LinearExperienceCurve();
        }

        private void Start()
        {
            ExperienceChanged?.Invoke(currentExperience, experienceCurve.GetRequiredExperience(CurrentLevel));
        }

        /// <summary>
        /// Adds experience, applying as many level ups as the amount allows.
        /// </summary>
        /// <param name="amount">The experience amount to add. Non-positive values are ignored.</param>
        public void AddExperience(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentExperience += amount;
            var requiredForNextLevel = experienceCurve.GetRequiredExperience(CurrentLevel);
            var didLevelUp = false;

            while (currentExperience >= requiredForNextLevel)
            {
                currentExperience -= requiredForNextLevel;
                statsController.BaseLevel++;
                statsController.BaseStats.GrantPoints(statPointsPerLevel);
                didLevelUp = true;
                LeveledUp?.Invoke(statsController.BaseLevel);
                requiredForNextLevel = experienceCurve.GetRequiredExperience(CurrentLevel);
            }

            if (didLevelUp)
            {
                Debug.Log(health != null
                    ? "Leveled up, healing to full."
                    : "Leveled up, but Health reference is NULL on PlayerExperience.");

                if (health != null)
                {
                    health.ResetHealth();
                }
            }

            ExperienceChanged?.Invoke(currentExperience, requiredForNextLevel);
        }
    }
}