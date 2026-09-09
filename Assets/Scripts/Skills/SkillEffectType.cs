namespace Project.Skills
{
    /// <summary>
    /// Whether a skill deals damage to an enemy, restores health, or
    /// applies a standing bonus just from being learned
    /// (<see cref="Passive"/> — e.g. Sword Mastery). A passive skill is
    /// never actually cast: its bonus is read directly from
    /// <see cref="SkillDefinition"/> by whatever system it affects (see
    /// <see cref="Character.Combat.PlayerPassiveSkillController"/>)
    /// whenever the skill is learned, with no cooldown or mana cost
    /// involved at all.
    /// </summary>
    public enum SkillEffectType
    {
        Damage,
        Heal,
        Passive
    }
}