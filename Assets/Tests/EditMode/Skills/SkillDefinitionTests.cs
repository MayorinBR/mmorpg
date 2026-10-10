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
