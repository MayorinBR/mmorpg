namespace Project.Character.Stats
{
    /// <summary>
    /// Flat 1-point-per-increase cost strategy, regardless of current
    /// value. No longer used by <c>PlayerStatsController</c>, which now
    /// uses the real cost curve in <see cref="RagnarokStatPointCostStrategy"/>;
    /// kept as a simple reference implementation of
    /// <see cref="IStatPointCostStrategy"/> (e.g. for unit tests that want
    /// a predictable, non-scaling cost).
    /// </summary>
    public class FlatStatPointCostStrategy : IStatPointCostStrategy
    {
        /// <inheritdoc />
        public int GetCostForNextPoint(int currentValue)
        {
            return 1;
        }
    }
}