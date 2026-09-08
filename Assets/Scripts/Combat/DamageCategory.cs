namespace Project.Combat
{
    /// <summary>
    /// Whether an attack's damage should be mitigated by a target's physical
    /// or magical defense. Independent of <see cref="Element"/> — a Fire
    /// Bolt is Magical damage carrying the Fire element, while an ordinary
    /// sword swing is Physical damage carrying Neutral.
    /// </summary>
    public enum DamageCategory
    {
        Physical,
        Magical
    }
}
