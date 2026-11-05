using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for the pure parts of the skill effect pipeline: <see cref="SkillEffectHandlerRegistry"/>,
    /// <see cref="SkillBuffFactory"/> and <see cref="HeadMarkerMath"/>.
    /// </summary>
    public class SkillEffectHandlerTests
    {
        private sealed class StubHandler : ISkillEffectHandler
        {
            public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
            {
                return true;
            }
        }

        private SkillDefinition skill;

        [SetUp]
        public void SetUp()
        {
            skill = ScriptableObject.CreateInstance<SkillDefinition>();
            skill.name = "TestSkill";
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(skill);
        }

        private void SetField(string name, object value)
        {
            var field = typeof(SkillDefinition).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(field, name);
            field.SetValue(skill, value);
        }

        [Test]
        public void Registry_ReturnsFallback_ForUnregisteredType()
        {
            var fallback = new StubHandler();
            var registry = new SkillEffectHandlerRegistry(fallback);

            Assert.AreSame(fallback, registry.Get(SkillEffectType.Heal));
        }

        [Test]
        public void Registry_ReturnsRegisteredHandler_AndReplacesPreviousOne()
        {
            var registry = new SkillEffectHandlerRegistry(new StubHandler());
            var first = new StubHandler();
            var second = new StubHandler();

            registry.Register(SkillEffectType.Heal, first);
            Assert.AreSame(first, registry.Get(SkillEffectType.Heal));

            registry.Register(SkillEffectType.Heal, second);
            Assert.AreSame(second, registry.Get(SkillEffectType.Heal));
        }

        [TestCase(SkillEffectType.Heal, typeof(HealEffectHandler))]
        [TestCase(SkillEffectType.Buff, typeof(BuffEffectHandler))]
        [TestCase(SkillEffectType.Toggle, typeof(ToggleEffectHandler))]
        [TestCase(SkillEffectType.ToggleDrain, typeof(ToggleDrainEffectHandler))]
        [TestCase(SkillEffectType.Reveal, typeof(RevealEffectHandler))]
        [TestCase(SkillEffectType.Cleanse, typeof(CleanseEffectHandler))]
        [TestCase(SkillEffectType.Displacement, typeof(DisplacementEffectHandler))]
        [TestCase(SkillEffectType.Craft, typeof(CraftEffectHandler))]
        [TestCase(SkillEffectType.Damage, typeof(DamageEffectHandler))]
        public void DefaultRegistry_ResolvesEachEffectType_ToItsHandler(SkillEffectType type, System.Type expected)
        {
            var registry = SkillEffectHandlerRegistry.CreateDefault();

            Assert.IsInstanceOf(expected, registry.Get(type));
        }

        [Test]
        public void BuildPayload_CarriesPercentsAndFlinchProtection()
        {
            SetField("buffAtkPercentBase", 0.02f);
            SetField("buffAtkPercentPerLevel", 0.03f);
            SetField("buffDefPercentBase", -0.05f);
            SetField("buffDefPercentPerLevel", -0.05f);
            SetField("buffPreventsFlinch", true);

            var payload = SkillBuffFactory.BuildPayload(skill, 4);

            Assert.AreEqual(0.14f, payload.AtkPercent, 0.0001f);
            Assert.AreEqual(-0.25f, payload.DefPercent, 0.0001f);
            Assert.IsTrue(payload.PreventsFlinch);
        }

        [Test]
        public void BuildDescriptor_UsesSkillNameAndSuffix_ToKeepEffectsSeparate()
        {
            var main = SkillBuffFactory.BuildDescriptor(skill, EffectCategory.Buff);
            var after = SkillBuffFactory.BuildDescriptor(skill, EffectCategory.Buff, ".afterEffect");

            Assert.AreEqual("skill.TestSkill", main.Id);
            Assert.AreEqual("skill.TestSkill.afterEffect", after.Id);
            Assert.AreEqual(EffectCategory.Buff, main.Category);
            Assert.AreEqual(EffectCategory.Debuff, SkillBuffFactory.BuildDescriptor(skill, EffectCategory.Debuff).Category);
        }

        [TestCase(2.0f, 0.0f, 2.1f, 0.2f)]
        [TestCase(1.0f, 5.0f, 2.1f, -5.8f)]
        public void ComputeShift_PlacesAuthoredPointAboveTargetTop(float targetTop, float instanceY, float authoredHeight, float expected)
        {
            Assert.AreEqual(expected, HeadMarkerMath.ComputeShift(targetTop, instanceY, authoredHeight), 0.0001f);
        }
    }
}
