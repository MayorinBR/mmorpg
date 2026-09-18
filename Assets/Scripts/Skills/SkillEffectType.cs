namespace Project.Skills
{
    /// <summary>
    /// Whether a skill deals damage to an enemy, restores health, applies a
    /// standing bonus just from being learned (<see cref="Passive"/> — e.g.
    /// Sword Mastery), applies a temporary, timed stat modifier
    /// (<see cref="Buff"/> — e.g. Provoke, Endure) to itself or a target via
    /// <see cref="Combat.BuffController"/>, or spawns a persistent,
    /// ground-placed damaging area (<see cref="Zone"/> — e.g. Fire Wall)
    /// that ticks damage against anything standing inside it for a limited
    /// duration via <see cref="Combat.SkillZoneController"/>, flips an
    /// untimed status flag on the caster (<see cref="Toggle"/> — currently
    /// only Hiding, recast to turn back off), or clears Hiding from every
    /// hidden target within <see cref="SkillDefinition.AreaRadius"/> of the
    /// caster (<see cref="Reveal"/> — e.g. Sight, Ruwach), optionally also
    /// damaging each one revealed this way if the skill's own damage
    /// fields are non-zero (e.g. Ruwach). A passive
    /// skill is never actually cast: its bonus is read directly from
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
        Buff,
        Zone,
        Toggle,
        Reveal
    }
}
