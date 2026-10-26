using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Project.Skills;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for the skill tree prerequisites enforced by <see cref="PlayerSkillBook"/>
    /// and for the prerequisites authored on the Mage and Archer skill assets.
    /// </summary>
    public class SkillPrerequisiteTests
    {
        private const string MageFolder = "Assets/Data/Skills/Mage/";
        private const string ArcherFolder = "Assets/Data/Skills/Archer/";

        private GameObject host;
        private PlayerSkillBook book;
        private SkillDefinition prerequisiteSkill;
        private SkillDefinition dependentSkill;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("SkillBookTestHost");
            book = host.AddComponent<PlayerSkillBook>();
            prerequisiteSkill = ScriptableObject.CreateInstance<SkillDefinition>();
            dependentSkill = ScriptableObject.CreateInstance<SkillDefinition>();
            SetPrerequisites(dependentSkill, new SkillPrerequisite(prerequisiteSkill, 3));
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(prerequisiteSkill);
            Object.DestroyImmediate(dependentSkill);
        }

        /// <summary>
        /// A skill without prerequisites is always available.
        /// </summary>
        [Test]
        public void MeetsPrerequisites_NoPrerequisites_ReturnsTrue()
        {
            Assert.IsTrue(book.MeetsPrerequisites(prerequisiteSkill));
        }

        /// <summary>
        /// The prerequisite must reach its required level, not merely be learned.
        /// </summary>
        [TestCase(0, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(5, true)]
        public void MeetsPrerequisites_RequiresMinimumLevel(int learnedLevel, bool expected)
        {
            SetLearnedLevel(prerequisiteSkill, learnedLevel);

            Assert.AreEqual(expected, book.MeetsPrerequisites(dependentSkill));
        }

        /// <summary>
        /// Every Mage prerequisite must point at a Mage skill that can actually reach the required level.
        /// </summary>
        [Test]
        public void MageAssets_PrerequisitesAreReachable()
        {
            var checkedSkills = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:SkillDefinition", new[] { MageFolder }))
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (var prerequisite in skill.Prerequisites)
                {
                    Assert.IsNotNull(prerequisite.Skill, skill.name);
                    Assert.AreNotSame(skill, prerequisite.Skill, skill.name);
                    Assert.LessOrEqual(prerequisite.Level, prerequisite.Skill.MaxLevel, skill.name);
                    checkedSkills++;
                }
            }

            Assert.Greater(checkedSkills, 0);
        }

        /// <summary>
        /// The authored Mage tree matches the Ragnarok Online skill tree (levels compressed to a maximum of 5).
        /// </summary>
        [TestCase("SoulStrike", "NapalmBeat:2")]
        [TestCase("SafetyWall", "NapalmBeat:4,SoulStrike:3")]
        [TestCase("FrostDiver", "ColdBolt:3")]
        [TestCase("FireBall", "FireBolt:2")]
        [TestCase("FireWall", "FireBall:3,Sight:1")]
        [TestCase("Thunderstorm", "LightningBolt:2")]
        [TestCase("FireBolt", "")]
        [TestCase("StoneCurse", "")]
        public void MageAssets_MatchExpectedTree(string skillName, string expected)
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(MageFolder + skillName + ".asset");
            Assert.IsNotNull(skill, skillName);

            var actual = new List<string>();
            foreach (var prerequisite in skill.Prerequisites)
            {
                actual.Add(prerequisite.Skill.name + ":" + prerequisite.Level);
            }

            Assert.AreEqual(expected, string.Join(",", actual));
        }

        /// <summary>
        /// The authored Archer tree matches the Ragnarok Online skill tree (levels compressed to a maximum of 5).
        /// </summary>
        [TestCase("ArrowShower", "Snipe:3")]
        [TestCase("VulturesEye", "OwlsEye:2")]
        [TestCase("ImproveConcentration", "VulturesEye:1")]
        [TestCase("Snipe", "")]
        [TestCase("OwlsEye", "")]
        public void ArcherAssets_MatchExpectedTree(string skillName, string expected)
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(ArcherFolder + skillName + ".asset");
            Assert.IsNotNull(skill, skillName);

            var actual = new List<string>();
            foreach (var prerequisite in skill.Prerequisites)
            {
                actual.Add(prerequisite.Skill.name + ":" + prerequisite.Level);
            }

            Assert.AreEqual(expected, string.Join(",", actual));
        }

        /// <summary>
        /// Every Archer prerequisite must point at an Archer skill that can actually reach the required level.
        /// </summary>
        [Test]
        public void ArcherAssets_PrerequisitesAreReachable()
        {
            var checkedSkills = 0;

            foreach (var guid in AssetDatabase.FindAssets("t:SkillDefinition", new[] { ArcherFolder }))
            {
                var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(AssetDatabase.GUIDToAssetPath(guid));

                foreach (var prerequisite in skill.Prerequisites)
                {
                    Assert.IsNotNull(prerequisite.Skill, skill.name);
                    Assert.LessOrEqual(prerequisite.Level, prerequisite.Skill.MaxLevel, skill.name);
                    checkedSkills++;
                }
            }

            Assert.Greater(checkedSkills, 0);
        }

        /// <summary>
        /// Safety Wall follows the wiki maximum level of 10, compressed to 5 like the other Mage skills.
        /// </summary>
        [Test]
        public void SafetyWall_HasCompressedMaxLevel()
        {
            var skill = AssetDatabase.LoadAssetAtPath<SkillDefinition>(MageFolder + "SafetyWall.asset");
            Assert.IsNotNull(skill);
            Assert.AreEqual(5, skill.MaxLevel);
        }

        private void SetLearnedLevel(SkillDefinition skill, int level)
        {
            var field = typeof(PlayerSkillBook).GetField("skillLevels", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field);
            ((Dictionary<SkillDefinition, int>)field.GetValue(book))[skill] = level;
        }

        private static void SetPrerequisites(SkillDefinition skill, params SkillPrerequisite[] prerequisites)
        {
            var field = typeof(SkillDefinition).GetField("prerequisites", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field);
            field.SetValue(skill, prerequisites);
        }
    }
}
