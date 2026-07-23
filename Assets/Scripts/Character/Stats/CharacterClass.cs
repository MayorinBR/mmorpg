namespace Project.Character.Stats
{
    /// <summary>
    /// The six Ragnarok Online base classes. A character starts as one of
    /// these; class evolution (e.g. Swordman promoting to Knight or
    /// Crusader) is intentionally not modeled yet — <see cref="Combat.PlayerClassController.ChangeClass"/>
    /// is the hook where that will plug in once evolutions are designed.
    /// </summary>
    public enum CharacterClass
    {
        Swordman,
        Archer,
        Merchant,
        Acolyte,
        Thief,
        Mage
    }
}