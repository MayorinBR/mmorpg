using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Tracks current mana for a character, sourcing max mana from a shared
    /// <see cref="CharacterStatsDefinition"/> — the same pattern used by
    /// <see cref="HealthComponent"/>, optionally boosted by an
    /// <see cref="IMaxManaBonusProvider"/> (see <see cref="maxManaBonusSource"/>).
    /// Exists ahead of the skill system so skills can consume mana as soon
    /// as they're implemented.
    /// </summary>
    [RequireComponent(typeof(CharacterStatsHolder))]
    public class ManaComponent : MonoBehaviour
    {
        [Tooltip("Optional component adding a bonus on top of the base max mana from Stats (e.g. the player's INT). Left empty, enemies simply use their flat Stats value.")]
        [SerializeField] private MonoBehaviour maxManaBonusSource;

        private CharacterStatsHolder statsHolder;
        private int currentMana;
        private IMaxManaBonusProvider maxManaBonusProvider;

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

        /// <summary>Raised whenever mana changes, with (currentMana, maxMana).</summary>
        public event Action<int, int> ManaChanged;

        /// <summary>
        /// Gets the maximum mana defined by the character's stats, plus
        /// any bonus from <see cref="maxManaBonusSource"/> (e.g. the
        /// player's INT-derived bonus).
        /// </summary>
        public int MaxMana
        {
            get
            {
                var baseMaxMana = StatsHolder.Stats.MaxMana;
                return baseMaxMana + (maxManaBonusProvider?.GetMaxManaBonus(baseMaxMana) ?? 0);
            }
        }

        /// <summary>Gets the current mana value.</summary>
        public int CurrentMana => currentMana;

        private void Awake()
        {
            maxManaBonusProvider = maxManaBonusSource as IMaxManaBonusProvider;
            currentMana = MaxMana;
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
        /// Re-evaluates <see cref="MaxMana"/> and raises
        /// <see cref="ManaChanged"/> with the current values, clamping
        /// <see cref="CurrentMana"/> down if it now exceeds the new
        /// maximum. Call this whenever something feeding
        /// <see cref="maxManaBonusSource"/> changes — a spent stat point,
        /// an equipment swap — since nothing here polls for that on its
        /// own: <see cref="MaxMana"/> is a live computed value, but UI
        /// bound to <see cref="ManaChanged"/> only re-reads it when this
        /// event fires.
        /// </summary>
        public void RefreshMaxMana()
        {
            currentMana = Mathf.Min(currentMana, MaxMana);
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