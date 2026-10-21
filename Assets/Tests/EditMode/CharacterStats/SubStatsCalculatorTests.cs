using System.Collections.Generic;
using NUnit.Framework;

namespace Project.Character.Stats.Tests
{
    /// <summary>
    /// Tests for <see cref="SubStatsCalculator"/>.
    /// </summary>
    public class SubStatsCalculatorTests
    {
        private const float Tolerance = 0.0001f;

        private readonly SubStatsCalculator calculator = new SubStatsCalculator();

        /// <summary>
        /// Minimal stat source so the calculator can be tested without a character.
        /// </summary>
        private sealed class FakeStats : IStatProvider
        {
            private readonly Dictionary<StatType, int> values = new Dictionary<StatType, int>();

            public FakeStats Set(StatType stat, int value)
            {
                values[stat] = value;
                return this;
            }

            public int GetValue(StatType stat)
            {
                return values.TryGetValue(stat, out var value) ? value : 1;
            }
        }

        /// <summary>
        /// Melee attackers scale status ATK from STR, plus a small LUK bonus.
        /// </summary>
        [Test]
        public void StatusAtk_Melee_UsesStrengthAndLuck()
        {
            var stats = new FakeStats().Set(StatType.Strength, 50).Set(StatType.Luck, 10);

            Assert.AreEqual(77, calculator.Calculate(stats, 1, false, 150).StatusAtk);
        }

        /// <summary>
        /// A ranged weapon switches the main attack stat from STR to DEX.
        /// </summary>
        [Test]
        public void StatusAtk_Ranged_UsesDexterity()
        {
            var stats = new FakeStats().Set(StatType.Strength, 1).Set(StatType.Dexterity, 40).Set(StatType.Luck, 0);

            Assert.AreEqual(56, calculator.Calculate(stats, 1, true, 150).StatusAtk);
        }

        /// <summary>
        /// MATK is the midpoint of the INT-derived minimum and maximum.
        /// </summary>
        [Test]
        public void StatusMatk_IsMidpointOfMinAndMax()
        {
            var stats = new FakeStats().Set(StatType.Intelligence, 49);

            Assert.AreEqual(114, calculator.Calculate(stats, 1, false, 150).StatusMatk);
        }

        /// <summary>
        /// Status DEF is 0.8 per VIT, truncated.
        /// </summary>
        [TestCase(10, 8)]
        [TestCase(33, 26)]
        [TestCase(99, 79)]
        public void StatusDef_IsEightyPercentOfVitality(int vitality, int expected)
        {
            var stats = new FakeStats().Set(StatType.Vitality, vitality);

            Assert.AreEqual(expected, calculator.Calculate(stats, 1, false, 150).StatusDef);
        }

        /// <summary>
        /// Status MDEF combines INT, VIT/5, DEX/5 and level/4.
        /// </summary>
        [Test]
        public void StatusMDef_CombinesIntVitDexAndLevel()
        {
            var stats = new FakeStats()
                .Set(StatType.Intelligence, 20)
                .Set(StatType.Vitality, 10)
                .Set(StatType.Dexterity, 15);

            Assert.AreEqual(35, calculator.Calculate(stats, 40, false, 150).StatusMDef);
        }

        /// <summary>
        /// HIT is level plus DEX, and FLEE is level plus AGI.
        /// </summary>
        [Test]
        public void HitAndFlee_AddBaseLevel()
        {
            var stats = new FakeStats().Set(StatType.Dexterity, 30).Set(StatType.Agility, 20);
            var result = calculator.Calculate(stats, 25, false, 150);

            Assert.AreEqual(55, result.Hit);
            Assert.AreEqual(45, result.Flee);
        }

        /// <summary>
        /// Critical rate is 0.3 per LUK plus 1.
        /// </summary>
        [Test]
        public void CriticalRate_ScalesWithLuck()
        {
            var stats = new FakeStats().Set(StatType.Luck, 10);

            Assert.AreEqual(4f, calculator.Calculate(stats, 1, false, 150).CriticalRate, Tolerance);
        }

        /// <summary>
        /// ASPD adds a flat AGI and DEX contribution to the supplied base.
        /// </summary>
        [Test]
        public void Aspd_AddsAgilityAndDexterityToBase()
        {
            var stats = new FakeStats().Set(StatType.Agility, 40).Set(StatType.Dexterity, 20);

            Assert.AreEqual(162, calculator.Calculate(stats, 1, false, 150).Aspd);
        }

        /// <summary>
        /// ASPD stays inside the 0-190 display range.
        /// </summary>
        [TestCase(185, 99, 99, 190)]
        [TestCase(-10, 1, 1, 0)]
        public void Aspd_IsClampedToDisplayRange(int baseAspd, int agility, int dexterity, int expected)
        {
            var stats = new FakeStats().Set(StatType.Agility, agility).Set(StatType.Dexterity, dexterity);

            Assert.AreEqual(expected, calculator.Calculate(stats, 1, false, baseAspd).Aspd);
        }
    }
}
