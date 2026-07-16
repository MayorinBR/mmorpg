namespace Project.Character.Stats
{
    /// <summary>
    /// Calculates how many stat points are required to raise a given stat
    /// from its current value to the next point. Kept separate from
    /// <see cref="CharacterBaseStats"/> so the cost curve can be tuned or
    /// replaced without changing how stats are stored or applied.
    /// </summary>
    public interface IStatPointCostStrategy
    {
        /// <summary>
        /// Gets the number of stat points required to raise a stat by one point.
        /// </summary>
        /// <param name="currentValue">The stat's current value before the increase.</param>
        /// <returns>The stat point cost for the next increase.</returns>
        int GetCostForNextPoint(int currentValue);
    }
}