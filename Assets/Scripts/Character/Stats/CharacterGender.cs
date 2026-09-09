namespace Project.Character.Stats
{
    /// <summary>
    /// The player's chosen gender, set once on the Character Selection
    /// screen. Purely cosmetic today — Ragnarok Online itself gives gender
    /// no mechanical effect, so this only decides which 3D model represents
    /// the player in-game (see <see cref="Combat.PlayerGenderController"/>).
    /// </summary>
    public enum CharacterGender
    {
        Male,
        Female
    }
}
