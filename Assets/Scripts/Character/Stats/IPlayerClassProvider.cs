namespace Project.Character.Stats
{
    /// <summary>
    /// Exposes a character's current class. Implemented by
    /// <see cref="Combat.PlayerClassController"/> so systems like
    /// equipment requirement checks can read the class without depending
    /// on the Character.Combat assembly (which itself depends on Items).
    /// </summary>
    public interface IPlayerClassProvider
    {
        /// <summary>Gets the character's current class.</summary>
        CharacterClass CurrentClass { get; }
    }
}