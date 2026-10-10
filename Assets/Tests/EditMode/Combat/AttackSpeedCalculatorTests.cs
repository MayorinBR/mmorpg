using NUnit.Framework;

namespace Project.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="AttackSpeedCalculator"/>.
    /// </summary>
    public class AttackSpeedCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        [TestCase(150, 1f, 0.5f)]
        [TestCase(150, 0.8f, 0.625f)]
        [TestCase(150, 2f, 0.25f)]
        public void GetAttackIntervalSeconds_AppliesWeaponMultiplier(int aspd, float multiplier, float expected)
        {
            Assert.AreEqual(expected, AttackSpeedCalculator.GetAttackIntervalSeconds(aspd, multiplier), Tolerance);
        }

        [TestCase(195, 1f)]
        [TestCase(195, 2f)]
        [TestCase(190, 3f)]
        public void GetAttackIntervalSeconds_NeverDropsUnderMinimum(int aspd, float multiplier)
        {
            Assert.AreEqual(0.1f, AttackSpeedCalculator.GetAttackIntervalSeconds(aspd, multiplier), Tolerance);
        }

        [Test]
        public void GetAttackIntervalSeconds_NonPositiveMultiplier_StaysFinite()
        {
            Assert.IsFalse(float.IsInfinity(AttackSpeedCalculator.GetAttackIntervalSeconds(150, 0f)));
        }

        [TestCase(160, 1f, 160)]
        [TestCase(160, 0.8f, 150)]
        [TestCase(160, 2f, 180)]
        [TestCase(195, 1f, 190)]
        public void GetEffectiveAspd_MatchesInterval(int aspd, float multiplier, int expected)
        {
            Assert.AreEqual(expected, AttackSpeedCalculator.GetEffectiveAspd(aspd, multiplier));
        }
    }
}
