using NUnit.Framework;

namespace Project.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="ElementalBonusDamage"/>.
    /// </summary>
    public class ElementalBonusDamageTests
    {
        [TestCase(100, 0.2f, 20)]
        [TestCase(7, 0.2f, 1)]
        [TestCase(1, 0.01f, 1)]
        public void CalculateBonus_ReturnsFractionOfHitWithMinimumOfOne(int hit, float percent, int expected)
        {
            Assert.AreEqual(expected, ElementalBonusDamage.CalculateBonus(hit, percent));
        }

        [TestCase(0, 0.2f)]
        [TestCase(-5, 0.2f)]
        [TestCase(100, 0f)]
        [TestCase(100, -0.2f)]
        public void CalculateBonus_NoBonusWhenHitOrPercentNotPositive(int hit, float percent)
        {
            Assert.AreEqual(0, ElementalBonusDamage.CalculateBonus(hit, percent));
        }
    }
}
