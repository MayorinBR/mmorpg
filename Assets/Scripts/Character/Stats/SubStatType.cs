namespace Project.Character.Stats
{
    /// <summary>
    /// The status-derived sub-stats shown on the player's status window,
    /// following the Ragnarok Online model (Atk, Matk, Def, MDef, Hit, Flee,
    /// Critical, Aspd). Unlike <see cref="StatType"/>, these are calculated
    /// values the player cannot invest points into directly.
    /// </summary>
    public enum SubStatType
    {
        Atk,
        Matk,
        Def,
        MDef,
        Hit,
        Flee,
        CriticalRate,
        Aspd
    }
}
