namespace Project.Character.Combat
{
    /// <summary>
    /// Calculates how much job experience is required to advance from a
    /// given job level to the next. Separate from <see cref="IExperienceCurve"/>
    /// since base and job level typically progress at different rates in
    /// Ragnarok Online, and each may need independent rebalancing.
    /// </summary>
    public interface IJobExperienceCurve
    {
        /// <summary>
        /// Gets the job experience required to advance from <paramref name="currentJobLevel"/>
        /// to the next job level.
        /// </summary>
        /// <param name="currentJobLevel">The character's current job level.</param>
        /// <returns>The job experience amount required for the next job level up.</returns>
        int GetRequiredExperience(int currentJobLevel);
    }
}