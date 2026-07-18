namespace Project.Character.Stats
{
    /// <summary>
    /// Provides a value for each of the six base stats. Implemented by
    /// <see cref="CharacterBaseStats"/> directly, and by wrapper types (such
    /// as one combining base stats with equipment bonuses) that need to
    /// feed <see cref="ISubStatsCalculator"/> a combined view without
    /// CharacterBaseStats itself knowing about equipment.
    /// </summary>
    public interface IStatProvider
    {
        /// <summary>
        /// Gets the effective value for the given stat.
        /// </summary>
        /// <param name="stat">The stat to read.</param>
        /// <returns>The effective value, including any bonuses the implementation applies.</returns>
        int GetValue(StatType stat);
    }
}