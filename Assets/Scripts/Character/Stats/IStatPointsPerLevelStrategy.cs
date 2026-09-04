namespace Project.Character.Stats
{
    /// <summary>
    /// Calculates how many stat points a character receives when advancing
    /// from one Base Level to the next. Kept separate from the experience
    /// system so the reward curve can be tuned or replaced without
    /// changing how leveling itself is tracked — the same separation
    /// <see cref="IStatPointCostStrategy"/> already applies to the cost of
    /// raising a stat.
    /// </summary>
    public interface IStatPointsPerLevelStrategy
    {
        /// <summary>
        /// Gets the number of stat points granted for advancing from the
        /// given level to the next one.
        /// </summary>
        /// <param name="currentLevel">The character's level before this level-up.</param>
        /// <returns>The number of stat points to grant.</returns>
        int GetPointsForLevelUp(int currentLevel);
    }
}
