namespace Project.Skills
{
    /// <summary>
    /// Who a skill can affect. <see cref="Self"/> always targets the
    /// caster. <see cref="Ally"/> targets a chosen ally (the caster or
    /// another player) — ally selection isn't implemented yet, so this
    /// currently behaves the same as <see cref="Self"/> until that exists.
    /// <see cref="Enemy"/> uses the caster's currently selected enemy target.
    /// <see cref="AreaAroundCaster"/> needs no selected target at all: it
    /// affects every valid target within the skill's own
    /// <see cref="SkillDefinition.AreaRadius"/> of the caster's own
    /// position (see <see cref="SkillDefinition.IsAreaOfEffect"/>) — e.g.
    /// Magnum Break. <see cref="AreaAroundTarget"/> still needs a selected
    /// enemy target in range, like <see cref="Enemy"/>, but affects every
    /// valid target within <see cref="SkillDefinition.AreaRadius"/> of that
    /// target's position instead of hitting only it — e.g. Fire Ball,
    /// Thunderstorm, Arrow Shower.
    /// </summary>
    public enum SkillTargetType
    {
        Self,
        Ally,
        Enemy,
        AreaAroundCaster,
        AreaAroundTarget
    }
}
