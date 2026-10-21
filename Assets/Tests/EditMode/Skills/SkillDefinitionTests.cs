using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Project.Skills.Tests
{
    /// <summary>
    /// Tests for the damage and health cost formulas of <see cref="SkillDefinition"/>.
    /// </summary>
    public class SkillDefinitionTests
    {
        private SkillDefinition skill;

        [SetUp]
        public void SetUp()
        {
            skill = ScriptableObject.CreateInstance<SkillDefinition>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(skill);
        }

        [TestCase(1, 120)]
        [TestCase(5, 200)]
        [TestCase(10, 300)]
        public void CalculateDamage_AddsBaseToPerLevelMultiplier(int level, int expected)
        {
            SetPhysicalDamageType();
            SetField("damageBaseMultiplier", 1f);
            SetField("damageMultiplierPerLevel", 0.2f);

            Assert.AreEqual(expected, skill.CalculateDamage(100, 0, level));
        }

        [Test]
        public void CalculateDamage_ZeroBase_KeepsPlainPerLevelScaling()
        {
            SetPhysicalDamageType();
            SetField("damageBaseMultiplier", 0f);
            SetField("damageMultiplierPerLevel", 0.5f);

            Assert.AreEqual(150, skill.CalculateDamage(100, 0, 3));
        }

        [TestCase(1, 20)]
        [TestCase(2, 20)]
        [TestCase(3, 19)]
        [TestCase(4, 19)]
        [TestCase(9, 16)]
        [TestCase(10, 16)]
        public void GetHealthCost_DropsOneEveryTwoLevels(int level, int expected)
        {
            SetField("healthCost", 20);
            SetField("healthCostReductionPerTwoLevels", 1);

            Assert.AreEqual(expected, skill.GetHealthCost(level));
        }

        [Test]
        public void GetHealthCost_NeverNegative_AndZeroWithoutCost()
        {
            Assert.AreEqual(0, skill.GetHealthCost(10));

            SetField("healthCost", 2);
            SetField("healthCostReductionPerTwoLevels", 5);

            Assert.AreEqual(0, skill.GetHealthCost(10));
        }

        /// <summary>
        /// The zone hit limit is zero (duration only) unless a base or per-level cap is authored,
        /// and is never lower than one once a cap exists.
        /// </summary>
        [TestCase(0, 0, 5, 0)]
        [TestCase(0, 2, 3, 6)]
        [TestCase(4, 0, 9, 4)]
        [TestCase(0, -5, 1, 1)]
        public void GetZoneMaxHits_FollowsAuthoredCaps(int baseHits, int perLevel, int level, int expected)
        {
            SetField("zoneMaxHits", baseHits);
            SetField("zoneMaxHitsPerLevel", perLevel);

            Assert.AreEqual(expected, skill.GetZoneMaxHits(level));
        }

        /// <summary>
        /// Fixed and variable cast times scale per level and are clamped at zero.
        /// </summary>
        [Test]
        public void CastTimes_ScalePerLevel_AndNeverGoNegative()
        {
            SetField("fixedCastSeconds", 0.5f);
            SetField("fixedCastPerLevel", 0.1f);
            SetField("variableCastSeconds", 1f);
            SetField("variableCastPerLevel", -0.5f);

            Assert.AreEqual(1.0f, skill.GetFixedCastTime(5), 0.0001f);
            Assert.AreEqual(0.5f, skill.GetVariableCastTime(1), 0.0001f);
            Assert.AreEqual(0f, skill.GetVariableCastTime(4), 0.0001f);
        }

        /// <summary>
        /// Accuracy bonus, status duration, poison tick damage and success chance grow linearly with level.
        /// </summary>
        [Test]
        public void PerLevelScaling_IsLinear()
        {
            SetField("accuracyBonusPerLevel", 3);
            SetField("inflictedStatusDurationSeconds", 2f);
            SetField("inflictedStatusDurationPerLevel", 1f);
            SetField("poisonDamagePerTickPerLevel", 2.5f);
            SetField("successChancePerLevel", 0.1f);

            Assert.AreEqual(12, skill.GetAccuracyBonus(4));
            Assert.AreEqual(6f, skill.GetInflictedStatusDuration(4), 0.0001f);
            Assert.AreEqual(10, skill.GetPoisonDamagePerTick(4));
            Assert.AreEqual(0.4f, skill.GetSuccessChance(4), 0.0001f);
        }

        private void SetPhysicalDamageType()
        {
            var field = typeof(SkillDefinition).GetField("damageType", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field);
            field.SetValue(skill, System.Enum.Parse(field.FieldType, "Physical"));
        }

        private void SetField(string name, object value)
        {
            var field = typeof(SkillDefinition).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name);
            field.SetValue(skill, value);
        }
    }
}
