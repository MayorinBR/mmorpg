namespace Project.Character.Stats
{
    /// <summary>
    /// Progression rules that depend on a character's class: the level cap
    /// that forces a Novice to evolve, and the Job Level required to do so.
    /// </summary>
    public static class ClassProgression
    {
        /// <summary>The Base Level and Job Level at which a Novice stops gaining experience.</summary>
        public const int NoviceLevelCap = 10;

        /// <summary>The Job Level a Novice needs to evolve into a first class.</summary>
        public const int EvolutionJobLevel = 10;

        /// <summary>
        /// Gets the highest Base Level and Job Level a class can reach.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The level cap, or <see cref="int.MaxValue"/> if the class is uncapped.</returns>
        public static int GetLevelCap(CharacterClass characterClass)
        {
            return characterClass == CharacterClass.Novice ? NoviceLevelCap : int.MaxValue;
        }
    }
}
