using NUnit.Framework;

namespace Project.Character.Stats.Tests
{
    /// <summary>
    /// Tests for <see cref="WeightCapacityCalculator"/>.
    /// </summary>
    public class WeightCapacityCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        /// <summary>
        /// The limit is 2000 plus 30 per STR plus the job modifier.
        /// </summary>
        [TestCase(1, 0f, 2030f)]
        [TestCase(50, 800f, 4300f)]
        [TestCase(99, 200f, 5170f)]
        public void Calculate_AppliesFormula(int strength, float jobModifier, float expected)
        {
            Assert.AreEqual(expected, WeightCapacityCalculator.Calculate(strength, jobModifier), Tolerance);
        }

        /// <summary>
        /// Each class grants its own carry weight bonus.
        /// </summary>
        [TestCase(CharacterClass.Swordman, 800f)]
        [TestCase(CharacterClass.Merchant, 800f)]
        [TestCase(CharacterClass.Archer, 600f)]
        [TestCase(CharacterClass.Acolyte, 400f)]
        [TestCase(CharacterClass.Thief, 400f)]
        [TestCase(CharacterClass.Mage, 200f)]
        [TestCase(CharacterClass.Novice, 0f)]
        public void GetJobModifier_ReturnsClassBonus(CharacterClass characterClass, float expected)
        {
            Assert.AreEqual(expected, WeightCapacityCalculator.GetJobModifier(characterClass), Tolerance);
        }
    }
}
