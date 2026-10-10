namespace Project.Character.Stats
{
    /// <summary>
    /// The Ragnarok Online classes. Every character starts as a
    /// <see cref="Novice"/> and evolves into one of the six first classes
    /// through <see cref="Combat.PlayerClassController.TryEvolve"/>. Values
    /// are only ever appended, since they are serialized as raw integers.
    /// </summary>
    public enum CharacterClass
    {
        Swordman,
        Archer,
        Merchant,
        Acolyte,
        Thief,
        Mage,
        Novice
    }
}