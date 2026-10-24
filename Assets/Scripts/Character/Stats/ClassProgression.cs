namespace Project.Character.Stats
{
    /// <summary>
    /// Progression rules that depend on a character's class: the level cap
    /// that forces a Novice to evolve, the Job Level required to do so and
    /// the level cap of every evolved class.
    /// </summary>
    public static class ClassProgression
    {
        /// <summary>The Base Level and Job Level at which a Novice stops gaining experience.</summary>
        public const int NoviceLevelCap = 10;

        /// <summary>The highest Base Level and Job Level of an evolved class.</summary>
        public const int EvolvedLevelCap = 99;

        /// <summary>The Job Level a Novice needs to evolve into a first class.</summary>
        public const int EvolutionJobLevel = 10;

        /// <summary>
        /// Gets the highest Base Level and Job Level a class can reach.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The level cap: 10 for a Novice, 99 for any evolved class.</returns>
        public static int GetLevelCap(CharacterClass characterClass)
        {
            return characterClass == CharacterClass.Novice ? NoviceLevelCap : EvolvedLevelCap;
        }
    }
}
