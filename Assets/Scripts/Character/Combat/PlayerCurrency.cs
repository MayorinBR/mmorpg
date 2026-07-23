using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks the player's currency (Zeny). This is the first place Zeny
    /// exists in the project — future systems (shops, loot, quest rewards)
    /// should add/spend through this rather than tracking their own copy.
    /// </summary>
    public class PlayerCurrency : MonoBehaviour
    {
        [SerializeField] private int startingZeny;

        /// <summary>Raised whenever the currency amount changes.</summary>
        public event Action<int> ZenyChanged;

        /// <summary>Gets the player's current Zeny amount.</summary>
        public int CurrentZeny { get; private set; }

        private void Awake()
        {
            CurrentZeny = startingZeny;
        }

        /// <summary>
        /// Adds Zeny, typically from a loot drop or quest reward.
        /// </summary>
        /// <param name="amount">The amount to add. Non-positive values are ignored.</param>
        public void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentZeny += amount;
            ZenyChanged?.Invoke(CurrentZeny);
        }

        /// <summary>
        /// Attempts to spend Zeny, typically for a shop purchase.
        /// </summary>
        /// <param name="amount">The amount to spend. Must be positive.</param>
        /// <returns>True if there was enough Zeny and it was spent; false otherwise.</returns>
        public bool TrySpend(int amount)
        {
            if (amount <= 0 || CurrentZeny < amount)
            {
                return false;
            }

            CurrentZeny -= amount;
            ZenyChanged?.Invoke(CurrentZeny);
            return true;
        }
    }
}