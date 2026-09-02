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
    [RequireComponent(typeof(CharacterStatsHolder))]
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private MonoBehaviour damageModifierSource;

        private CharacterStatsHolder statsHolder;
        private int currentHealth;
        private IDamageModifier damageModifier;

        private CharacterStatsHolder StatsHolder
        {
            get
            {
                if (statsHolder == null)
                {
                    statsHolder = GetComponent<CharacterStatsHolder>();
                }

                return statsHolder;
            }
        }

        /// <summary>Raised whenever health changes, with (currentHealth, maxHealth).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Died;

        /// <summary>Gets the maximum health defined by the character's stats.</summary>
        public int MaxHealth => StatsHolder.Stats.MaxHealth;

        /// <summary>Gets the current health value.</summary>
        public int CurrentHealth => currentHealth;

        /// <inheritdoc />
        public bool IsDead => currentHealth <= 0;

        private void Awake()
        {
            currentHealth = MaxHealth;
            damageModifier = damageModifierSource as IDamageModifier;
        }

        /// <inheritdoc />
        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            if (damageModifier != null)
            {
                amount = damageModifier.ModifyIncomingDamage(amount);
            }

            if (amount <= 0)
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

        /// <summary>
        /// Restores a partial amount of health, clamped to the maximum.
        /// Does nothing if already dead.
        /// </summary>
        /// <param name="amount">The amount of health to restore. Non-positive values are ignored.</param>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        /// <summary>
        /// Directly sets the current health to a previously-known value,
        /// clamped to [0, MaxHealth], without raising <see cref="Died"/>.
        /// Intended for restoring captured state (e.g. enemy world-state
        /// restore across map switches) rather than as a combat action —
        /// normal gameplay should keep using <see cref="TakeDamage"/> and
        /// <see cref="Heal"/>.
        /// </summary>
        /// <param name="value">The health value to restore.</param>
        public void SetCurrentHealth(int value)
        {
            currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}
