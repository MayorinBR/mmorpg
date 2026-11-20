using NUnit.Framework;

namespace Project.Ambience.Tests
{
    /// <summary>
    /// Tests for <see cref="DayNightCycle"/> and the position maths of <see cref="WorldClock"/>.
    /// </summary>
    public class DayNightCycleTests
    {
        [TearDown]
        public void ResetClock()
        {
            WorldClock.ResetToRealTime();
        }

        [TestCase(0d, 0f)]
        [TestCase(7200d, 0f)]
        [TestCase(7300d, 100f)]
        [TestCase(-100d, 7100f)]
        public void Wrap_KeepsPositionInsideOneCycle(double seconds, float expected)
        {
            Assert.AreEqual(expected, DayNightCycle.Wrap(seconds), 0.001f);
        }

        [TestCase(0f, DayNightPeriod.Day)]
        [TestCase(3599.9f, DayNightPeriod.Day)]
        [TestCase(3600f, DayNightPeriod.Night)]
        [TestCase(7199f, DayNightPeriod.Night)]
        public void PeriodAt_SplitsTheCycleInHalf(float cycleSeconds, DayNightPeriod expected)
        {
            Assert.AreEqual(expected, DayNightCycle.PeriodAt(cycleSeconds));
        }

        [TestCase(0f, 0f)]
        [TestCase(1800f, 0.5f)]
        [TestCase(3600f, 0f)]
        [TestCase(5400f, 0.5f)]
        public void ProgressInPeriod_RestartsAtEachPeriod(float cycleSeconds, float expected)
        {
            var normalized = DayNightCycle.Normalized(cycleSeconds);
            Assert.AreEqual(expected, DayNightCycle.ProgressInPeriod(normalized), 0.001f);
        }

        [Test]
        public void CycleSecondsAt_AdvancesWithRealTime()
        {
            WorldClock.SetCycleSeconds(100f);

            var later = WorldClock.CycleSecondsAt(WorldClock.RealSeconds() + 50d);

            Assert.AreEqual(150f, later, 1f);
        }

        [Test]
        public void SetTimeScale_SpeedsUpTheClockWithoutJumping()
        {
            WorldClock.SetCycleSeconds(0f);
            WorldClock.SetTimeScale(60f);

            var later = WorldClock.CycleSecondsAt(WorldClock.RealSeconds() + 10d);

            Assert.AreEqual(600f, later, 5f);
        }

        [Test]
        public void ResetToRealTime_RestoresNormalSpeed()
        {
            WorldClock.SetTimeScale(600f);
            WorldClock.ResetToRealTime();

            Assert.AreEqual(1f, WorldClock.TimeScale);
        }
    }
}
