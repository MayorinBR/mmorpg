using System;
using System.Collections.Generic;

namespace Project.Character.Stats
{
    /// <summary>
    /// Holds the base value of each of the six RO-style stats for a single
    /// character, along with the stat points available to spend. Total stats
    /// (base plus equipment/buff bonuses) are intentionally out of scope here
    /// and belong to a separate aggregation step closer to combat.
    /// </summary>
    public class CharacterBaseStats : IStatProvider
    {
        private const int MinStatValue = 1;
        private const int MaxStatValue = 99;

        private readonly Dictionary<StatType, int> baseValues;
        private readonly IStatPointCostStrategy costStrategy;

        /// <summary>
        /// Gets the number of unspent stat points available for investment.
        /// </summary>
        public int AvailablePoints { get; private set; }

        /// <summary>
        /// Initializes a new stat block with every stat set to its minimum value.
        /// </summary>
        /// <param name="costStrategy">Strategy used to price each stat increase.</param>
        public CharacterBaseStats(IStatPointCostStrategy costStrategy)
        {
            this.costStrategy = costStrategy;

            baseValues = new Dictionary<StatType, int>
            {
                { StatType.Strength, MinStatValue },
                { StatType.Agility, MinStatValue },
                { StatType.Vitality, MinStatValue },
                { StatType.Intelligence, MinStatValue },
                { StatType.Dexterity, MinStatValue },
                { StatType.Luck, MinStatValue }
            };
        }

        /// <summary>
        /// Gets the current base value of the given stat.
        /// </summary>
        /// <param name="stat">The stat to read.</param>
        /// <returns>The stat's current base value.</returns>
        public int GetValue(StatType stat)
        {
            return baseValues[stat];
        }

        /// <summary>
        /// Grants stat points to the character, typically called on level up.
        /// </summary>
        /// <param name="amount">The number of points to add. Must be positive.</param>
        public void GrantPoints(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Granted points must be positive.");
            }

            AvailablePoints += amount;
        }

        /// <summary>
        /// Attempts to raise a stat by one point, spending the required
        /// amount from <see cref="AvailablePoints"/>.
        /// </summary>
        /// <param name="stat">The stat to raise.</param>
        /// <returns>True if the stat was raised; false if the stat is at its
        /// maximum value or there are not enough available points.</returns>
        public bool TryIncreaseStat(StatType stat)
        {
            var currentValue = baseValues[stat];

            if (currentValue >= MaxStatValue)
            {
                return false;
            }

            var cost = costStrategy.GetCostForNextPoint(currentValue);

            if (cost > AvailablePoints)
            {
                return false;
            }

            AvailablePoints -= cost;
            baseValues[stat] = currentValue + 1;
            return true;
        }
    }
}