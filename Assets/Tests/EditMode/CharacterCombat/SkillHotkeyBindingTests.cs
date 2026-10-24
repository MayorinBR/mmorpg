using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Guards the hotbar shortcuts: each CastSkill action must sit on its own key (1-9, then 0), and every
    /// PlayerInput event in the Bootstrap scene must point at the action it is named after.
    /// </summary>
    public class SkillHotkeyBindingTests
    {
        private const string InputAssetPath = "Assets/Input/PlayerControls.inputactions";
        private const string ScenePath = "Assets/Scenes/Bootstrap.unity";
        private const string ActionNamePrefix = "m_ActionName: 'Character/CastSkill";
        private const string ActionIdPrefix = "m_ActionId: ";

        private InputActionAsset asset;

        [SetUp]
        public void SetUp()
        {
            asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputAssetPath);
        }

        /// <summary>Slot 1 is the 1 key, slot 9 the 9 key and slot 10 the 0 key.</summary>
        [TestCase("CastSkill1", "<Keyboard>/1")]
        [TestCase("CastSkill2", "<Keyboard>/2")]
        [TestCase("CastSkill3", "<Keyboard>/3")]
        [TestCase("CastSkill4", "<Keyboard>/4")]
        [TestCase("CastSkill5", "<Keyboard>/5")]
        [TestCase("CastSkill6", "<Keyboard>/6")]
        [TestCase("CastSkill7", "<Keyboard>/7")]
        [TestCase("CastSkill8", "<Keyboard>/8")]
        [TestCase("CastSkill9", "<Keyboard>/9")]
        [TestCase("CastSkill0", "<Keyboard>/0")]
        public void CastAction_IsBoundToItsOwnKey(string actionName, string expectedPath)
        {
            var action = asset.FindAction("Character/" + actionName, true);

            Assert.AreEqual(1, action.bindings.Count);
            Assert.AreEqual(expectedPath, action.bindings[0].path);
        }

        /// <summary>Every CastSkill event in the Bootstrap scene references the id of the action it is named after.</summary>
        [Test]
        public void BootstrapScene_CastSkillEvents_ReferenceTheirOwnAction()
        {
            var lines = File.ReadAllLines(ScenePath);
            var checkedEvents = new List<string>();

            for (var i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                if (!line.StartsWith(ActionNamePrefix))
                {
                    continue;
                }

                var nameStart = line.IndexOf("CastSkill") + 9;
                var actionName = "CastSkill" + line[nameStart];
                var sceneId = lines[i - 1].Trim().Substring(ActionIdPrefix.Length);

                Assert.AreEqual(asset.FindAction("Character/" + actionName, true).id.ToString(), sceneId, actionName);
                checkedEvents.Add(actionName);
            }

            Assert.AreEqual(10, checkedEvents.Count);
        }
    }
}
