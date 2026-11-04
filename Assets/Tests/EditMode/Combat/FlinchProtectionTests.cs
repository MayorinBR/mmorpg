using NUnit.Framework;
using UnityEngine;

namespace Project.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="BuffController.TryConsumeFlinchProtection"/>, including the hit limit used by Endure.
    /// </summary>
    public class FlinchProtectionTests
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

        private static EffectDescriptor Descriptor()
        {
            return new EffectDescriptor("Endure", "Endure", "description", null, EffectCategory.Buff);
        }

        [Test]
        public void WithoutBuff_DoesNotProtect()
        {
            Assert.IsFalse(buffs.TryConsumeFlinchProtection());
        }

        [Test]
        public void LimitedBuff_AbsorbsExactlyTheLimit_ThenEnds()
        {
            buffs.ApplyBuff(new BuffPayload(preventsFlinch: true), 60f, Descriptor(), 3);

            Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            Assert.IsFalse(buffs.TryConsumeFlinchProtection());
            Assert.IsFalse(buffs.PreventsFlinch);
        }

        [Test]
        public void UnlimitedBuff_NeverRunsOut_UntilItExpires()
        {
            buffs.ApplyBuff(new BuffPayload(preventsFlinch: true), 10f, Descriptor());

            for (var i = 0; i < 20; i++)
            {
                Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            }

            now += 11f;
            Assert.IsFalse(buffs.TryConsumeFlinchProtection());
        }

        [Test]
        public void LimitedBuff_StopsProtecting_WhenItExpiresBeforeTheLimit()
        {
            buffs.ApplyBuff(new BuffPayload(preventsFlinch: true), 10f, Descriptor(), 7);
            now += 11f;

            Assert.IsFalse(buffs.TryConsumeFlinchProtection());
        }

        [Test]
        public void ReapplyingTheBuff_RestoresTheHitLimit()
        {
            buffs.ApplyBuff(new BuffPayload(preventsFlinch: true), 60f, Descriptor(), 1);
            Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            Assert.IsFalse(buffs.TryConsumeFlinchProtection());

            buffs.ApplyBuff(new BuffPayload(preventsFlinch: true), 60f, Descriptor(), 1);
            Assert.IsTrue(buffs.TryConsumeFlinchProtection());
        }

        [Test]
        public void PersistentModifier_ProtectsWithoutLimit()
        {
            buffs.SetPersistentModifier(this, new BuffPayload(preventsFlinch: true));

            for (var i = 0; i < 20; i++)
            {
                Assert.IsTrue(buffs.TryConsumeFlinchProtection());
            }
        }
    }
}
