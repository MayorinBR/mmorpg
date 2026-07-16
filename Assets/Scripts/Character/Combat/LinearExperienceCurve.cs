namespace Project.Character.Combat
{
    /// <summary>
    /// Temporary placeholder curve: requires 100 experience per current
    /// level (level 1 needs 100, level 2 needs 200, etc). Exists so leveling
    /// can be tested before the real Ragnarok experience curve is implemented.
    /// </summary>
    public class LinearExperienceCurve : IExperienceCurve
    {
        private const int ExperiencePerLevel = 100;

        /// <inheritdoc />
        public int GetRequiredExperience(int currentLevel)
        {
            return currentLevel * ExperiencePerLevel;
        }
    }
}