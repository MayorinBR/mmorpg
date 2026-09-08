namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component contribute a bonus to a
    /// character's maximum mana, on top of the flat value defined by its
    /// <see cref="Project.Character.Stats.CharacterStatsDefinition"/>. Mirrors
    /// the single-optional-hook pattern already used by
    /// <see cref="IDamageModifier"/>, so <see cref="ManaComponent"/> stays
    /// unaware of where the bonus comes from — for the player, this is INT
    /// (see <see cref="Project.Character.Combat.PlayerStatsController"/>);
    /// enemies have no implementer and simply get no bonus.
    /// </summary>
    public interface IMaxManaBonusProvider
    {
        /// <summary>
        /// Calculates the bonus to add on top of a character's base maximum mana.
        /// </summary>
        /// <param name="baseMaxMana">The character's max mana before this bonus, as defined by its stats asset.</param>
        /// <returns>The bonus amount to add. Zero for no effect.</returns>
        int GetMaxManaBonus(int baseMaxMana);
    }
}
