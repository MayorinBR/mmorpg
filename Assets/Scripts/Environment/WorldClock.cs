using System;
using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// The world's clock. By default the position in the day and night cycle is derived from the real UTC
    /// clock, so it needs no saving and is the same for everyone. Debug tools can shift the position or
    /// speed it up, and <see cref="ResetToRealTime"/> returns to the real clock. A hidden ticker object
    /// raises <see cref="PeriodChanged"/> and switches the registered lights when the period flips.
    /// </summary>
    public static class WorldClock
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static double anchorRealSeconds;
        private static double anchorCycleSeconds;
        private static float timeScale = 1f;
        private static DayNightPeriod lastPeriod;

        /// <summary>
        /// Raised once whenever the world passes from day to night or from night to day.
        /// </summary>
        public static event Action<DayNightPeriod> PeriodChanged;

        /// <summary>Gets the speed multiplier of the clock; 1 is real time.</summary>
        public static float TimeScale => timeScale;

        /// <summary>Gets the current position in the cycle, in seconds.</summary>
        public static float CycleSeconds => CycleSecondsAt(RealSeconds());

        /// <summary>Gets the current position in the cycle, from 0 (sunrise) to 1.</summary>
        public static float Normalized => DayNightCycle.Normalized(CycleSeconds);

        /// <summary>Gets the current period.</summary>
        public static DayNightPeriod Period => DayNightCycle.PeriodAt(CycleSeconds);

        /// <summary>Gets a value indicating whether it is currently night.</summary>
        public static bool IsNight => Period == DayNightPeriod.Night;

        /// <summary>
        /// Gets the current real time as seconds since the Unix epoch, in UTC.
        /// </summary>
        /// <returns>The real seconds.</returns>
        public static double RealSeconds()
        {
            return (DateTime.UtcNow - UnixEpoch).TotalSeconds;
        }

        /// <summary>
        /// Computes the position in the cycle at a given real time.
        /// </summary>
        /// <param name="realSeconds">Real seconds since the Unix epoch.</param>
        /// <returns>The position in the cycle, in seconds.</returns>
        public static float CycleSecondsAt(double realSeconds)
        {
            return DayNightCycle.Wrap(anchorCycleSeconds + (realSeconds - anchorRealSeconds) * timeScale);
        }

        /// <summary>
        /// Moves the clock to a position in the cycle, keeping the current speed. Meant for debug tools.
        /// </summary>
        /// <param name="cycleSeconds">The position to jump to, in seconds.</param>
        public static void SetCycleSeconds(float cycleSeconds)
        {
            anchorRealSeconds = RealSeconds();
            anchorCycleSeconds = DayNightCycle.Wrap(cycleSeconds);
        }

        /// <summary>
        /// Changes the speed of the clock without a jump in the current position. Meant for debug tools.
        /// </summary>
        /// <param name="scale">The speed multiplier; 1 is real time.</param>
        public static void SetTimeScale(float scale)
        {
            SetCycleSeconds(CycleSeconds);
            timeScale = Mathf.Max(0f, scale);
        }

        /// <summary>
        /// Returns the clock to the real UTC clock at normal speed.
        /// </summary>
        public static void ResetToRealTime()
        {
            anchorRealSeconds = 0d;
            anchorCycleSeconds = 0d;
            timeScale = 1f;
        }

        /// <summary>
        /// Checks whether the period changed since the last call and raises <see cref="PeriodChanged"/> if so.
        /// Called every frame by the ticker.
        /// </summary>
        internal static void Tick()
        {
            var period = Period;

            if (period == lastPeriod)
            {
                return;
            }

            lastPeriod = period;
            PeriodChanged?.Invoke(period);
        }

        /// <summary>
        /// Records the current period as already seen, so the first tick does not report a change.
        /// </summary>
        internal static void SyncPeriod()
        {
            lastPeriod = Period;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            ResetToRealTime();
            PeriodChanged = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateTicker()
        {
            var host = new GameObject(nameof(WorldClockTicker)) { hideFlags = HideFlags.HideInHierarchy };
            host.AddComponent<WorldClockTicker>();
            UnityEngine.Object.DontDestroyOnLoad(host);
        }
    }
}
