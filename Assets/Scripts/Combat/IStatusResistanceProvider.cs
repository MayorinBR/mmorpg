namespace Project.Combat
{
    /// <summary>
    /// Optional hook that lets a component supply a character's chance to
    /// fully resist an incoming status effect (see
    /// <see cref="StatusEffectController.ApplyStun"/> and its Apply*
    /// siblings). Mirrors the single-optional-hook pattern already used by
    /// <see cref="IMaxHealthBonusProvider"/> — for the player, this is
    /// VIT-derived (see <see cref="Project.Character.Combat.PlayerStatsController"/>);
    /// enemies have no implementer and fall back to their flat
    /// <see cref="Project.Character.Stats.CharacterStatsDefinition.StatusResistChance"/>.
    /// </summary>
    public interface IStatusResistanceProvider
    {
        /// <summary>Gets the chance, from 0 to 1, to fully resist an incoming status effect.</summary>
        float GetStatusResistChance();
    }
}
