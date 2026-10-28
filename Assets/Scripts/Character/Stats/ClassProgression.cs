namespace Project.Character.Stats
{
    /// <summary>
    /// Progression rules that depend on a character's class: the level caps
    /// that force a Novice to evolve, the Job Level required to do so and
    /// the Base Level and Job Level caps of every first class.
    /// </summary>
    public static class ClassProgression
    {
        /// <summary>The Base Level at which a Novice stops gaining experience.</summary>
        public const int NoviceBaseLevelCap = 10;

        /// <summary>The Job Level at which a Novice stops gaining experience.</summary>
        public const int NoviceJobLevelCap = 10;

        /// <summary>The highest Base Level of a first class.</summary>
        public const int FirstClassBaseLevelCap = 99;

        /// <summary>The highest Job Level of a first class.</summary>
        public const int FirstClassJobLevelCap = 50;

        /// <summary>The Job Level a Novice needs to evolve into a first class.</summary>
        public const int EvolutionJobLevel = 10;

        /// <summary>
        /// Gets the highest Base Level a class can reach.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The Base Level cap: 10 for a Novice, 99 for any first class.</returns>
        public static int GetBaseLevelCap(CharacterClass characterClass)
        {
            return characterClass == CharacterClass.Novice ? NoviceBaseLevelCap : FirstClassBaseLevelCap;
        }

        /// <summary>
        /// Gets the highest Job Level a class can reach.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The Job Level cap: 10 for a Novice, 50 for any first class.</returns>
        public static int GetJobLevelCap(CharacterClass characterClass)
        {
            return characterClass == CharacterClass.Novice ? NoviceJobLevelCap : FirstClassJobLevelCap;
        }
    }
}
