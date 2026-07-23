using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Tracks current mana for a character, sourcing max mana from a shared
    /// <see cref="CharacterStatsDefinition"/> — the same pattern used by
    /// <see cref="HealthComponent"/>. Exists ahead of the skill system so
    /// skills can consume mana as soon as they're implemented.
    /// </summary>
    public class ManaComponent : MonoBehaviour
    {
        [SerializeField] private CharacterStatsDefinition stats;

        private int currentMana;

        /// <summary>Raised whenever mana changes, with (currentMana, maxMana).</summary>
        public event Action<int, int> ManaChanged;

        /// <summary>Gets the maximum mana defined by the character's stats.</summary>
        public int MaxMana => stats.MaxMana;

        /// <summary>Gets the current mana value.</summary>
        public int CurrentMana => currentMana;

        private void Awake()
        {
            currentMana = stats.MaxMana;
        }

        /// <summary>
        /// Attempts to consume mana, typically for casting a skill.
        /// </summary>
        /// <param name="amount">The amount of mana to consume. Must be positive.</param>
        /// <returns>True if there was enough mana and it was consumed; false otherwise.</returns>
        public bool TryConsumeMana(int amount)
        {
            if (amount <= 0 || currentMana < amount)
            {
                return false;
            }

            currentMana -= amount;
            ManaChanged?.Invoke(currentMana, MaxMana);
            return true;
        }

        /// <summary>
        /// Restores mana, clamped to the maximum.
        /// </summary>
        /// <param name="amount">The amount of mana to restore. Non-positive values are ignored.</param>
        public void RestoreMana(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            currentMana = Mathf.Min(currentMana + amount, MaxMana);
            ManaChanged?.Invoke(currentMana, MaxMana);
        }

        /// <summary>
        /// Restores mana to its maximum value, typically called on level up or respawn.
        /// </summary>
        public void ResetMana()
        {
            currentMana = MaxMana;
            ManaChanged?.Invoke(currentMana, MaxMana);
        }
    }
}