using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Quests.Tests
{
    /// <summary>
    /// Tests for abandoning a quest and for the completion event the class trial relies on.
    /// </summary>
    public class QuestManagerAbandonTests
    {
        private GameObject host;
        private QuestManager manager;
        private CharacterStatsDefinition species;
        private KillCreatureRequirement requirement;
        private QuestDefinition quest;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("QuestManagerTest");
            manager = host.AddComponent<QuestManager>();

            species = ScriptableObject.CreateInstance<CharacterStatsDefinition>();
            requirement = ScriptableObject.CreateInstance<KillCreatureRequirement>();
            SetField(requirement, "targetSpecies", species);
            SetField(requirement, "requiredCount", 2);

            quest = ScriptableObject.CreateInstance<QuestDefinition>();
            SetField(quest, "requirements", new QuestRequirement[] { requirement });
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(quest);
            Object.DestroyImmediate(requirement);
            Object.DestroyImmediate(species);
        }

        [Test]
        public void AbandonQuest_DiscardsProgress_SoTheNextAcceptStartsAtZero()
        {
            manager.AcceptQuest(quest);
            EnemyDeathEvents.RaiseEnemyKilled(species);
            Assert.AreEqual(1, manager.ActiveQuests[0].GetProgress(0));

            Assert.IsTrue(manager.AbandonQuest(quest));
            Assert.IsFalse(manager.IsActive(quest));

            manager.AcceptQuest(quest);
            Assert.AreEqual(0, manager.ActiveQuests[0].GetProgress(0));
        }

        [Test]
        public void AbandonQuest_NotActive_ReturnsFalse()
        {
            Assert.IsFalse(manager.AbandonQuest(quest));
        }

        [Test]
        public void QuestCompleted_IsRaisedOnceWhenTheLastObjectiveIsMet()
        {
            QuestDefinition completed = null;
            var raised = 0;
            manager.QuestCompleted += finished =>
            {
                completed = finished;
                raised++;
            };

            manager.AcceptQuest(quest);
            EnemyDeathEvents.RaiseEnemyKilled(species);
            Assert.AreEqual(0, raised);

            EnemyDeathEvents.RaiseEnemyKilled(species);

            Assert.AreEqual(1, raised);
            Assert.AreSame(quest, completed);
            Assert.IsTrue(manager.IsCompleted(quest));
        }

        private static void SetField(object target, string name, object value)
        {
            var field = target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name);
            field.SetValue(target, value);
        }
    }
}
