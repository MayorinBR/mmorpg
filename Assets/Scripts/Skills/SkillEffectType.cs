namespace Project.Skills
{
    /// <summary>
    /// Whether a skill deals damage to an enemy, restores health, applies a
    /// standing bonus just from being learned (<see cref="Passive"/> — e.g.
    /// Sword Mastery), or applies a temporary, timed stat modifier
    /// (<see cref="Buff"/> — e.g. Provoke, Endure) to itself or a target via
    /// <see cref="Combat.BuffController"/>. A passive skill is never
    /// actually cast: its bonus is read directly from
    /// <see cref="SkillDefinition"/> by whatever system it affects (see
    /// <see cref="Character.Combat.PlayerPassiveSkillController"/>)
    /// whenever the skill is learned, with no cooldown or mana cost
    /// involved at all.
    /// </summary>
    public enum SkillEffectType
    {
        Damage,
        Heal,
        Passive,
        Buff
    }
}