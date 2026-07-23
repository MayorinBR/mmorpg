namespace Project.Character.Combat
{
    /// <summary>
    /// Temporary placeholder curve: requires 50 job experience per current
    /// job level. Exists so job leveling can be tested before the real
    /// per-class curve (job level caps differ by class in Ragnarok Online)
    /// is implemented.
    /// </summary>
    public class LinearJobExperienceCurve : IJobExperienceCurve
    {
        private const int ExperiencePerLevel = 50;

        /// <inheritdoc />
        public int GetRequiredExperience(int currentJobLevel)
        {
            return currentJobLevel * ExperiencePerLevel;
        }
    }
}