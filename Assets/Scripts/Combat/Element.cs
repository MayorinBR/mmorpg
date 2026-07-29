namespace Project.Combat
{
    /// <summary>
    /// An elemental affinity an attack can carry. Currently only tracked
    /// for the Mage's basic attack; has no mechanical effect on damage yet
    /// since no per-element resistance system exists on enemies (tracked
    /// as planned work in FUTURE_IMPROVEMENTS.md).
    /// </summary>
    public enum Element
    {
        Water,
        Fire,
        Grass,
        Ground,
        Electric
    }
}