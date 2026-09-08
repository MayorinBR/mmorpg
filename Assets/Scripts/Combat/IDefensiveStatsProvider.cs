namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component contribute a character's
    /// defensive profile — physical defense, magical defense and flee
    /// rating — on top of the flat values defined by its
    /// <see cref="Project.Character.Stats.CharacterStatsDefinition"/>. Mirrors
    /// the single-optional-hook pattern already used by
    /// <see cref="IDamageModifier"/> and <see cref="IMaxHealthBonusProvider"/>,
    /// so <see cref="HealthComponent"/> stays unaware of where the values
    /// come from — for the player, this is <see cref="Project.Character.Combat.PlayerStatsController"/>
    /// (DEX/AGI/VIT/INT-derived, via its calculated sub-stats); enemies have
    /// no implementer and simply fall back to their flat Stats values.
    /// </summary>
    /// <remarks>
    /// Bundled into a single interface rather than three single-method ones:
    /// unlike <see cref="IMaxHealthBonusProvider"/>/<see cref="IMaxManaBonusProvider"/>,
    /// which are independent bonuses to unrelated resources, physical
    /// defense, magical defense and flee rating are always consulted
    /// together as one "defensive profile" concept, and any implementer
    /// (a stats controller) naturally has all three at once.
    /// </remarks>
    public interface IDefensiveStatsProvider
    {
        /// <summary>Gets the rating that reduces incoming <see cref="DamageCategory.Physical"/> damage.</summary>
        int GetPhysicalDefense();

        /// <summary>Gets the rating that reduces incoming <see cref="DamageCategory.Magical"/> damage.</summary>
        int GetMagicalDefense();

        /// <summary>Gets the dodge rating used to resolve an attacker's hit chance against this character.</summary>
        int GetFleeRating();
    }
}
