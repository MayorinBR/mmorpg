using NUnit.Framework;
using Project.Skills;
using UnityEngine;

namespace Project.Character.Combat.Tests
{
    /// <summary>EditMode tests for <see cref="SkillCooldownTracker"/>.</summary>
    public class SkillCooldownTrackerTests
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

        [Test]
        public void UnknownSkill_IsReady()
        {
            var tracker = new SkillCooldownTracker();

            Assert.IsFalse(tracker.IsOnCooldown(skill, 10f));
            Assert.AreEqual(0f, tracker.GetRemaining(skill, 10f));
        }

        [Test]
        public void StartedSkill_IsOnCooldownUntilDurationElapses()
        {
            var tracker = new SkillCooldownTracker();
            tracker.Start(skill, 10f, 3f);

            Assert.IsTrue(tracker.IsOnCooldown(skill, 12f));
            Assert.AreEqual(1f, tracker.GetRemaining(skill, 12f), 0.0001f);
            Assert.IsFalse(tracker.IsOnCooldown(skill, 13f));
            Assert.AreEqual(0f, tracker.GetRemaining(skill, 20f));
        }
    }
}
