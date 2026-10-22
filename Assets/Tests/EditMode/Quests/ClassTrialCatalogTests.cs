using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Quests.Tests
{
    /// <summary>
    /// Tests for looking up class trials in a <see cref="ClassTrialCatalog"/>.
    /// </summary>
    public class ClassTrialCatalogTests
    {
        private ClassTrialCatalog catalog;
        private QuestDefinition swordmanQuest;
        private QuestDefinition mageQuest;

        [SetUp]
        public void SetUp()
        {
            swordmanQuest = ScriptableObject.CreateInstance<QuestDefinition>();
            mageQuest = ScriptableObject.CreateInstance<QuestDefinition>();
            catalog = ScriptableObject.CreateInstance<ClassTrialCatalog>();

            var trials = new[]
            {
                CreateTrial(CharacterClass.Swordman, swordmanQuest),
                CreateTrial(CharacterClass.Mage, mageQuest)
            };

            typeof(ClassTrialCatalog).GetField("trials", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(catalog, trials);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(swordmanQuest);
            Object.DestroyImmediate(mageQuest);
        }

        [Test]
        public void TryGetTargetClass_KnownQuest_ReturnsItsClass()
        {
            Assert.IsTrue(catalog.TryGetTargetClass(mageQuest, out var target));
            Assert.AreEqual(CharacterClass.Mage, target);
        }

        [Test]
        public void TryGetTargetClass_UnrelatedQuest_ReturnsFalse()
        {
            var other = ScriptableObject.CreateInstance<QuestDefinition>();

            Assert.IsFalse(catalog.TryGetTargetClass(other, out _));

            Object.DestroyImmediate(other);
        }

        [Test]
        public void FindTrial_ReturnsFirstMatchOrNull()
        {
            Assert.AreEqual(CharacterClass.Swordman, catalog.FindTrial(quest => quest == swordmanQuest).TargetClass);
            Assert.IsNull(catalog.FindTrial(quest => false));
        }

        private static ClassTrial CreateTrial(CharacterClass targetClass, QuestDefinition quest)
        {
            var trial = new ClassTrial();
            typeof(ClassTrial).GetField("targetClass", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(trial, targetClass);
            typeof(ClassTrial).GetField("quest", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(trial, quest);
            return trial;
        }
    }
}
