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
        /// <param name="stats">A provider of the character's current effective stat values.</param>
        /// <param name="baseLevel">The character's current base level.</param>
        /// <param name="weaponIsRanged">
        /// Whether the character's equipped weapon is a ranged type (bow,
        /// gun, instrument or whip). Ragnarok Online derives the physical
        /// attack stat from STR for melee weapons but from DEX for ranged
        /// ones — the weapon decides, not the class — so this is what makes
        /// an Archer's bow scale off DEX while a Swordman's sword scales
        /// off STR.
        /// </param>
        /// <param name="baseAspd">
        /// The Aspd rating (classic Ragnarok Online's 0-190 display scale)
        /// before AGI/DEX are added on top — supplied by the caller (see
        /// <see cref="Combat.PlayerStatsController"/>) instead of being a
        /// fixed constant here, so base attack speed can be tuned per
        /// design pass without changing this formula.
        /// </param>
        /// <returns>The calculated status-derived sub-stats.</returns>
        SubStats Calculate(IStatProvider stats, int baseLevel, bool weaponIsRanged, int baseAspd);
    }
}