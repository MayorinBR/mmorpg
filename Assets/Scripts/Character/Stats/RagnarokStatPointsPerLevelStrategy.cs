namespace Project.Character.Stats
{
    /// <summary>
    /// Real Ragnarok Online stat points-per-level-up curve (source: iRO
    /// Wiki — Stats, consulted July 2026): the reward increases across
    /// three tiers as Base Level climbs, replacing the flat 5-points
    /// placeholder used before this curve was implemented. Levels above
    /// 200 have no documented case in the source; this keeps applying the
    /// 151-200 tier's formula for them rather than throwing, since no
    /// level cap exists yet in <c>PlayerExperience</c>.
    /// </summary>
    public class RagnarokStatPointsPerLevelStrategy : IStatPointsPerLevelStrategy
    {
        /// <inheritdoc />
        /// <remarks>
        /// The source's own lookup table lists 27 points for level 150
        /// (as part of its "140-150" band) while this tier's formula
        /// evaluates to 28 at that exact level — a discrepancy in the
        /// source data itself at the single boundary level between the
        /// 100-150 and 151-200 tiers. The formula is used as written here
        /// rather than special-cased for level 150, since every other
        /// level in both tables is consistent with it.
        /// </remarks>
        public int GetPointsForLevelUp(int currentLevel)
        {
            if (currentLevel < 100)
            {
                return currentLevel / 5 + 3;
            }

            if (currentLevel <= 150)
            {
                return currentLevel / 10 + 13;
            }

            return (currentLevel - 150) / 7 + 28;
        }
    }
}
