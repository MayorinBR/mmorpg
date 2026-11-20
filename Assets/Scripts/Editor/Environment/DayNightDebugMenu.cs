using UnityEditor;
using UnityEngine;
using Project.Ambience;

namespace Project.EditorTools
{
    /// <summary>
    /// Editor menu to jump to a time of day and change the clock speed while testing in Play mode.
    /// The clock is the real UTC clock outside of these overrides.
    /// </summary>
    public static class DayNightDebugMenu
    {
        private const string Root = "Tools/Environment/Day Night/";

        [MenuItem(Root + "Sunrise")]
        private static void Sunrise() => Jump(0f);

        [MenuItem(Root + "Noon")]
        private static void Noon() => Jump(DayNightCycle.PeriodSeconds * 0.5f);

        [MenuItem(Root + "Sunset (shortly before)")]
        private static void Sunset() => Jump(DayNightCycle.PeriodSeconds - 60f);

        [MenuItem(Root + "Midnight")]
        private static void Midnight() => Jump(DayNightCycle.PeriodSeconds * 1.5f);

        [MenuItem(Root + "Speed x1")]
        private static void Speed1() => WorldClock.SetTimeScale(1f);

        [MenuItem(Root + "Speed x60")]
        private static void Speed60() => WorldClock.SetTimeScale(60f);

        [MenuItem(Root + "Speed x600")]
        private static void Speed600() => WorldClock.SetTimeScale(600f);

        [MenuItem(Root + "Back to real time")]
        private static void RealTime() => WorldClock.ResetToRealTime();

        [MenuItem(Root + "Log current time")]
        private static void LogTime()
        {
            Debug.Log($"World clock: {WorldClock.Period}, {WorldClock.CycleSeconds / 60f:0.0} min into the cycle, speed x{WorldClock.TimeScale}.");
        }

        private static void Jump(float cycleSeconds)
        {
            WorldClock.SetCycleSeconds(cycleSeconds);
        }
    }
}
