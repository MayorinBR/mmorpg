using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Drives <see cref="WorldClock"/> every frame and, when the period flips, asks every registered light
    /// to switch for the new period. Created automatically before the first scene loads and kept for the
    /// whole session.
    /// </summary>
    internal sealed class WorldClockTicker : MonoBehaviour
    {
        private void Awake()
        {
            WorldClock.SyncPeriod();
        }

        private void OnEnable()
        {
            WorldClock.PeriodChanged += SwitchLights;
        }

        private void OnDisable()
        {
            WorldClock.PeriodChanged -= SwitchLights;
        }

        private void Update()
        {
            WorldClock.Tick();
        }

        private static void SwitchLights(DayNightPeriod period)
        {
            LightRegistry.TransitionAll(period == DayNightPeriod.Night);
        }
    }
}
