namespace Project.Character.Stats
{
    /// <summary>
    /// Exposes a character's current base level. Implemented by
    /// <see cref="Combat.PlayerStatsController"/> so systems like
    /// equipment requirement checks can read the level without depending
    /// on the Character.Combat assembly (which itself depends on Items).
    /// </summary>
    public interface IPlayerLevelProvider
    {
        /// <summary>Gets the character's current base level.</summary>
        int BaseLevel { get; }
    }
}