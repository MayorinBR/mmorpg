using NUnit.Framework;
using UnityEngine;
using Project.Combat;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="SkillCastContext.IsCastInterrupted"/>, the rule that cancels a delayed skill impact.
    /// </summary>
    public class SkillCastInterruptionTests
    {
        private GameObject host;
        private StatusEffectController statusEffects;

        [SetUp]
        public void SetUp()
        {
            host = new GameObject("CasterHost");
            statusEffects = host.AddComponent<StatusEffectController>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
        }

        [Test]
        public void WithoutAnyInterruption_TheImpactLands()
        {
            Assert.IsFalse(SkillCastContext.IsCastInterrupted(statusEffects, null));
            Assert.IsFalse(SkillCastContext.IsCastInterrupted(null, null));
        }

        [Test]
        public void AStunnedCaster_CancelsTheImpact()
        {
            statusEffects.ApplyStun(5f);

            Assert.IsTrue(statusEffects.IsStunned);
            Assert.IsTrue(SkillCastContext.IsCastInterrupted(statusEffects, null));
        }
    }
}
