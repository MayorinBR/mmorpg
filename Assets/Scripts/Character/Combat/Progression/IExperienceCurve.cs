namespace Project.Character.Combat
{
    /// <summary>
    /// Calculates how much experience is required to advance from a given
    /// level to the next. Kept as an interface so the curve can be tuned or
    /// replaced (e.g. with the real Ragnarok formula) without changing how
    /// experience is tracked and applied.
    /// </summary>
    public interface IExperienceCurve
    {
        /// <summary>
        /// Gets the experience required to advance from <paramref name="currentLevel"/>
        /// to the next level.
        /// </summary>
        /// <param name="currentLevel">The character's current level.</param>
        /// <returns>The experience amount required for the next level up.</returns>
        int GetRequiredExperience(int currentLevel);
    }
}