using NUnit.Framework;
using UnityEditor;

namespace Project.Skills.Tests
{
    /// <summary>
    /// Checks the authored Thief skills against the max levels and Job Level requirements of the irowiki Thief page.
    /// </summary>
    public class ThiefSkillDataTests
    {
        private const string Folder = "Assets/Data/Skills/Thief/";

        /// <summary>Checks one Thief skill's max level and Job Level requirement.</summary>
        /// <param name="assetName">The skill asset's file name without extension.</param>
        /// <param name="maxLevel">The expected max level.</param>
        /// <param name="jobLevel">The expected required Job Level.</param>
        [TestCase("DoubleAttack", 10, 0)]
        [TestCase("Envenom", 10, 0)]
        [TestCase("Hiding", 10, 0)]
        [TestCase("ImproveDodge", 10, 0)]
        [TestCase("Steal", 10, 0)]
        [TestCase("Detoxify", 1, 0)]
        [TestCase("FindStone", 1, 20)]
        [TestCase("StoneFling", 1, 15)]
        [TestCase("SandAttack", 1, 25)]
        [TestCase("BackSlide", 1, 35)]
        public void ThiefSkill_MatchesWikiLevelAndJobLevel(string assetName, int maxLevel, int jobLevel)
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(Folder + assetName + ".asset");

            Assert.NotNull(skill, assetName);
            Assert.AreEqual(maxLevel, skill.MaxLevel);
            Assert.AreEqual(jobLevel, skill.RequiredJobLevel);
        }

        /// <summary>Checks that Double Attack reaches 50 percent at its max level.</summary>
        [Test]
        public void DoubleAttack_ReachesHalfChanceAtMaxLevel()
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(Folder + "DoubleAttack.asset");

            Assert.AreEqual(0.5f, skill.GetPassiveDoubleAttackChance(10), 0.001f);
        }
    }
}
