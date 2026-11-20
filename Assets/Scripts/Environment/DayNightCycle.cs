namespace Project.Ambience
{
    /// <summary>
    /// Pure maths of the day and night cycle, free of Unity types so it can be unit tested. A cycle lasts
    /// <see cref="CycleSeconds"/>: the first half is day, the second half is night. The normalized value runs
    /// from 0 to 1, where 0 is sunrise, 0.25 noon, 0.5 sunset and 0.75 midnight.
    /// </summary>
    public static class DayNightCycle
    {
        /// <summary>Length of the day period, and of the night period, in seconds.</summary>
        public const float PeriodSeconds = 3600f;

        /// <summary>Length of a full cycle in seconds.</summary>
        public const float CycleSeconds = PeriodSeconds * 2f;

        /// <summary>
        /// Wraps any amount of seconds into the range [0, <see cref="CycleSeconds"/>).
        /// </summary>
        /// <param name="seconds">The seconds to wrap; may be negative or larger than a cycle.</param>
        /// <returns>The equivalent position inside one cycle.</returns>
        public static float Wrap(double seconds)
        {
            var wrapped = seconds % CycleSeconds;

            if (wrapped < 0d)
            {
                wrapped += CycleSeconds;
            }

            return (float)wrapped;
        }

        /// <summary>
        /// Gets the period a position in the cycle belongs to.
        /// </summary>
        /// <param name="cycleSeconds">A position in the cycle, in seconds.</param>
        /// <returns>Day for the first half of the cycle, Night for the second.</returns>
        public static DayNightPeriod PeriodAt(float cycleSeconds)
        {
            return cycleSeconds < PeriodSeconds ? DayNightPeriod.Day : DayNightPeriod.Night;
        }

        /// <summary>
        /// Converts a position in the cycle into the normalized range [0, 1).
        /// </summary>
        /// <param name="cycleSeconds">A position in the cycle, in seconds.</param>
        /// <returns>The normalized position.</returns>
        public static float Normalized(float cycleSeconds)
        {
            return cycleSeconds / CycleSeconds;
        }

        /// <summary>
        /// Gets how far through the current period a normalized position is.
        /// </summary>
        /// <param name="normalized">A normalized position in the cycle.</param>
        /// <returns>0 at the start of the period and approaching 1 at its end.</returns>
        public static float ProgressInPeriod(float normalized)
        {
            return normalized * 2f % 1f;
        }
    }
}
