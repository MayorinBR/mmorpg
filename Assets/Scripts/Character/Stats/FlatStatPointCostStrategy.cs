namespace Project.Character.Stats
{
    /// <summary>
    /// Temporary placeholder strategy that costs a flat 1 point per stat
    /// increase, regardless of current value. Exists so
    /// <see cref="CharacterBaseStats"/> can be used for testing before the
    /// real Ragnarok cost curve (which scales past 100) is implemented.
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