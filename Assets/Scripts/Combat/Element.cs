namespace Project.Combat
{
    /// <summary>
    /// Ghost, Holy and Poison were appended later than the other six
    /// (append-only, per this enum's own Unity raw-int serialization
    /// convention) to cover Soul Strike/Napalm Beat, Holy Light/Ruwach/
    /// Heal's Undead-damage interaction, and Envenom respectively.
    /// The affinity an attack carries — an elemental one, or plain physical
    /// damage, modeled here as <see cref="Neutral"/> rather than as a
    /// special "no element" case, matching how Ragnarok Online itself
    /// treats Neutral as a real, resistable property. Carried by the
    /// Mage's basic attack (<see cref="Project.Character.Combat.PlayerElementController.CurrentElement"/>)
    /// and optionally by a <see cref="Project.Skills.SkillDefinition"/>, and
    /// read by an optional <see cref="ElementalResistanceComponent"/> on the
    /// target to scale incoming damage of any of these values, Neutral
    /// included. <see cref="Neutral"/> is the default for every source of
    /// damage that isn't explicitly elemental: basic attacks from every
    /// non-Mage class, and any skill that doesn't set one.
    /// </summary>
    public enum Element
    {
        Water,
        Fire,
        Grass,
        Ground,
        Electric,
        Neutral,
        Ghost,
        Holy,
        Poison
    }
}