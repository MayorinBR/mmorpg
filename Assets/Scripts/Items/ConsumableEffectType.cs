namespace Project.Items
{
    /// <summary>
    /// How a consumable item's health/mana restore amounts are applied:
    /// all at once, or split into repeated smaller ticks over a duration.
    /// </summary>
    public enum ConsumableEffectType
    {
        Instant,
        OverTime
    }
}
