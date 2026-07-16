using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Tracks current health for a character, sourcing max health from a
    /// shared <see cref="CharacterStatsDefinition"/> so player and enemies
    /// use the same data-driven stat asset. Raises events on change and
    /// death so other systems (UI, AI, loot) can react without polling.
    /// </summary>
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private CharacterStatsDefinition stats;

        private int currentHealth;

        /// <summary>Raised whenever health changes, with (currentHealth, maxHealth).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Died;

        /// <summary>Gets the maximum health defined by the character's stats.</summary>
        public int MaxHealth => stats.MaxHealth;

        /// <summary>Gets the current health value.</summary>
        public int CurrentHealth => currentHealth;

        /// <inheritdoc />
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = stats.MaxHealth;
        }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Max(currentHealth - amount, 0);
            HealthChanged?.Invoke(currentHealth, MaxHealth);

            if (currentHealth == 0)
            {
                Died?.Invoke();
            }
        }

        /// <summary>
        /// Restores health to its maximum value, typically called when
        /// reviving an entity after death (e.g. enemy respawn).
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = MaxHealth;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}