using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Project.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="ActiveEffectRules"/>, <see cref="BuffPayloadDescription"/> and the
    /// active effect reporting of <see cref="BuffController"/>.
    /// </summary>
    public class ActiveEffectTests
    {
        private GameObject host;
        private BuffController buffs;
        private float now;

        [SetUp]
        public void SetUp()
        {
            now = 100f;
            host = new GameObject("BuffHost");
            buffs = host.AddComponent<BuffController>();
            buffs.SetClock(() => now);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
        }

        private static EffectDescriptor Descriptor(string id, EffectCategory category = EffectCategory.Buff)
        {
            return new EffectDescriptor(id, id, "description", null, category);
        }

        private static ActiveEffect Effect(string id, EffectCategory category, float remaining, float total)
        {
            return new ActiveEffect(Descriptor(id, category), remaining, total);
        }

        [TestCase(0f, 0f, false)]
        [TestCase(10f, 60f, false)]
        [TestCase(3f, 60f, true)]
        [TestCase(0.5f, 60f, true)]
        public void IsExpiring_OnlyForTimedEffectsNearTheEnd(float remaining, float total, bool expected)
        {
            Assert.AreEqual(expected, ActiveEffectRules.IsExpiring(Effect("x", EffectCategory.Buff, remaining, total)));
        }

        [Test]
        public void SortForDisplay_PutsHarmfulFirstThenTimedByRemainingTime()
        {
            var list = new List<ActiveEffect>
            {
                Effect("Passive", EffectCategory.Passive, 0f, 0f),
                Effect("Toggle", EffectCategory.Toggle, 0f, 0f),
                Effect("BuffLong", EffectCategory.Buff, 50f, 60f),
                Effect("BuffShort", EffectCategory.Buff, 5f, 60f),
                Effect("Poison", EffectCategory.Status, 8f, 10f),
                Effect("Provoke", EffectCategory.Debuff, 20f, 30f)
            };

            ActiveEffectRules.SortForDisplay(list);

            CollectionAssert.AreEqual(
                new[] { "Provoke", "Poison", "BuffShort", "BuffLong", "Toggle", "Passive" },
                list.ConvertAll(effect => effect.Descriptor.Id));
        }

        [TestCase(0f, "0s")]
        [TestCase(-3f, "0s")]
        [TestCase(0.2f, "1s")]
        [TestCase(45f, "45s")]
        [TestCase(125f, "2m 05s")]
        [TestCase(3780f, "1h 03m")]
        public void FormatRemaining_UsesTheLargestUnit(float seconds, string expected)
        {
            Assert.AreEqual(expected, ActiveEffectRules.FormatRemaining(seconds));
        }

        [Test]
        public void RemainingFraction_IsOneWithoutTimerAndShrinksWithTime()
        {
            Assert.AreEqual(1f, Effect("a", EffectCategory.Toggle, 0f, 0f).RemainingFraction, 0.0001f);
            Assert.AreEqual(0.25f, Effect("b", EffectCategory.Buff, 15f, 60f).RemainingFraction, 0.0001f);
        }

        [Test]
        public void Describe_ListsOnlyNonZeroComponentsWithSigns()
        {
            var payload = new BuffPayload(atkPercent: 0.32f, defPercent: -0.55f, agility: 3, incomingDamageReductionPercent: 0.3f, preventsFlinch: true);

            Assert.AreEqual("ATK +32%\nDEF -55%\nAGI +3\nDamage taken -30%\nCannot be staggered by hits", BuffPayloadDescription.Describe(payload));
            Assert.AreEqual(string.Empty, BuffPayloadDescription.Describe(default));
        }

        [Test]
        public void ApplyBuff_SameId_RefreshesInsteadOfStacking()
        {
            buffs.ApplyBuff(new BuffPayload(strength: 5), 60f, Descriptor("Blessing"));
            now += 40f;
            buffs.ApplyBuff(new BuffPayload(strength: 5), 60f, Descriptor("Blessing"));

            var effects = new List<ActiveEffect>();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(1, effects.Count);
            Assert.AreEqual(60f, effects[0].RemainingSeconds, 0.001f);
            Assert.AreEqual(5, buffs.Total.Strength);
        }

        [Test]
        public void ApplyBuff_DifferentIds_Coexist()
        {
            buffs.ApplyBuff(new BuffPayload(strength: 5), 60f, Descriptor("Blessing"));
            buffs.ApplyBuff(new BuffPayload(agility: 3), 60f, Descriptor("IncreaseAgi"));

            var effects = new List<ActiveEffect>();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(2, effects.Count);
            Assert.AreEqual(5, buffs.Total.Strength);
            Assert.AreEqual(3, buffs.Total.Agility);
        }

        [Test]
        public void CollectActiveEffects_ReportsRemainingTimeAndDropsExpiredBuffs()
        {
            buffs.ApplyBuff(new BuffPayload(strength: 5), 60f, Descriptor("Blessing"));
            now += 15f;

            var effects = new List<ActiveEffect>();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(45f, effects[0].RemainingSeconds, 0.001f);
            Assert.AreEqual(60f, effects[0].TotalSeconds, 0.001f);

            now += 50f;
            effects.Clear();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(0, effects.Count);
            Assert.AreEqual(0, buffs.Total.Strength);
        }

        [Test]
        public void AnonymousBuffs_StillApplyButAreNotReported()
        {
            buffs.ApplyBuff(new BuffPayload(strength: 5), 60f);

            var effects = new List<ActiveEffect>();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(0, effects.Count);
            Assert.AreEqual(5, buffs.Total.Strength);
        }

        [Test]
        public void PersistentModifier_IsReportedWithoutTimerAndClearedWhenEmpty()
        {
            var source = new object();
            buffs.SetPersistentModifier(source, new BuffPayload(incomingDamageReductionPercent: 0.3f), Descriptor("EnergyCoat", EffectCategory.Toggle));

            var effects = new List<ActiveEffect>();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(1, effects.Count);
            Assert.IsFalse(effects[0].HasTimer);

            buffs.SetPersistentModifier(source, default, default);
            effects.Clear();
            buffs.CollectActiveEffects(effects);

            Assert.AreEqual(0, effects.Count);
        }
    }
}
