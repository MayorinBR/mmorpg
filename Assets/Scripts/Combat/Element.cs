namespace Project.Combat
{
    /// <summary>
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
        Neutral
    }
}