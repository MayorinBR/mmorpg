namespace Project.Character.Combat
{
    /// <summary>
    /// Flat placeholder curve: requires 100 experience per current level
    /// (level 1 needs 100, level 2 needs 200, etc). No longer used by
    /// <see cref="PlayerExperience"/> — replaced by <see cref="RealRagnarokExperienceCurve"/>,
    /// the sourced iRO Wiki table. Kept as a simple, predictable, non-scaling
    /// <see cref="IExperienceCurve"/> implementation, useful for tests that
    /// want an easy-to-reason-about curve instead of the real one's huge
    /// late-game jumps (mirrors why <c>FlatStatPointCostStrategy</c> was
    /// kept around after <c>RagnarokStatPointCostStrategy</c> replaced it).
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