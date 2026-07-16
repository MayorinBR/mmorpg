namespace Project.Character.Stats
{
    /// <summary>
    /// Calculates the status-derived sub-stats for a character at a given
    /// base level. Separated as an interface so the underlying formulas can
    /// be replaced or rebalanced without changing how base stats are stored.
    /// </summary>
    public interface ISubStatsCalculator
    {
        /// <summary>
        /// Calculates the sub-stats for a character.
        /// </summary>
        /// <param name="baseStats">The character's current base stat values.</param>
        /// <param name="baseLevel">The character's current base level.</param>
        /// <returns>The calculated status-derived sub-stats.</returns>
        SubStats Calculate(CharacterBaseStats baseStats, int baseLevel);
    }
}