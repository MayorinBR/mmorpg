namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component contribute a bonus to a
    /// character's maximum health, on top of the flat value defined by its
    /// <see cref="Project.Character.Stats.CharacterStatsDefinition"/>. Mirrors
    /// the single-optional-hook pattern already used by
    /// <see cref="IDamageModifier"/>, so <see cref="HealthComponent"/> stays
    /// unaware of where the bonus comes from — for the player, this is VIT
    /// (see <see cref="Project.Character.Combat.PlayerStatsController"/>);
    /// enemies have no implementer and simply get no bonus.
    /// </summary>
    public interface IMaxHealthBonusProvider
    {
        /// <summary>
        /// Calculates the bonus to add on top of a character's base maximum health.
        /// </summary>
        /// <param name="baseMaxHealth">The character's max health before this bonus, as defined by its stats asset.</param>
        /// <returns>The bonus amount to add. Zero for no effect.</returns>
        int GetMaxHealthBonus(int baseMaxHealth);
    }
}
