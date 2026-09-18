namespace Project.Combat
{
    /// <summary>
    /// Identifies one of a character's timed status effects (see
    /// <see cref="StatusEffectController"/>), letting a skill or other
    /// producer name which one it applies without referencing the
    /// controller's individual per-status methods directly (see
    /// <see cref="StatusEffectController.Apply"/>).
    /// </summary>
    public enum StatusEffectType
    {
        None,
        Stun,
        Poison,
        Silence,
        Blind,
        Freeze,
        Petrify
    }
}
