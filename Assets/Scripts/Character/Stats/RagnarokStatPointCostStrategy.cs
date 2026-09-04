namespace Project.Character.Stats
{
    /// <summary>
    /// Real Ragnarok Online stat point cost curve (source: iRO Wiki —
    /// Stats, consulted July 2026): the cost to raise a stat climbs in
    /// bands of 10, replacing the flat 1-point placeholder
    /// <see cref="FlatStatPointCostStrategy"/> used before this curve was
    /// implemented. Covers stat values 1-99, the only range
    /// <see cref="CharacterBaseStats"/> currently allows; the steeper
    /// 100-129 tier (third class only) is out of scope until stats above
    /// 99 are supported.
    /// </summary>
    public class RagnarokStatPointCostStrategy : IStatPointCostStrategy
    {
        /// <inheritdoc />
        public int GetCostForNextPoint(int currentValue)
        {
            return (currentValue - 1) / 10 + 2;
        }
    }
}
