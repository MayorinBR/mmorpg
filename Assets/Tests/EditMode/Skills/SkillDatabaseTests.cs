using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Project.Skills.Tests
{
    /// <summary>
    /// Tests for <see cref="SkillDatabase.Contains"/>, which decides whether a skill belongs to a class tier.
    /// </summary>
    public class SkillDatabaseTests
    {
        private SkillDatabase database;
        private SkillDefinition listed;
        private SkillDefinition unlisted;

        [SetUp]
        public void SetUp()
        {
            listed = ScriptableObject.CreateInstance<SkillDefinition>();
            unlisted = ScriptableObject.CreateInstance<SkillDefinition>();
            database = ScriptableObject.CreateInstance<SkillDatabase>();
            typeof(SkillDatabase)
                .GetField("allSkills", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(database, new[] { listed });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(database);
            Object.DestroyImmediate(listed);
            Object.DestroyImmediate(unlisted);
        }

        /// <summary>A skill listed in the database belongs to it.</summary>
        [Test]
        public void Contains_ListedSkill_ReturnsTrue()
        {
            Assert.IsTrue(database.Contains(listed));
        }

        /// <summary>A skill from another class tier does not belong to it.</summary>
        [Test]
        public void Contains_UnlistedSkill_ReturnsFalse()
        {
            Assert.IsFalse(database.Contains(unlisted));
        }

        /// <summary>A null skill never belongs to a database.</summary>
        [Test]
        public void Contains_Null_ReturnsFalse()
        {
            Assert.IsFalse(database.Contains(null));
        }
    }
}
