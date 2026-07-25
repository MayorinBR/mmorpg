namespace Project.Skills
{
    /// <summary>
    /// Who a skill can affect. <see cref="Self"/> always targets the
    /// caster. <see cref="Ally"/> targets a chosen ally (the caster or
    /// another player) — ally selection isn't implemented yet, so this
    /// currently behaves the same as <see cref="Self"/> until that exists.
    /// <see cref="Enemy"/> uses the caster's currently selected enemy target.
    /// </summary>
    public enum SkillTargetType
    {
        Self,
        Ally,
        Enemy
    }
}