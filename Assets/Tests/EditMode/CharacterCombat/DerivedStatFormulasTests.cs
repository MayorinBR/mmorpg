using NUnit.Framework;
using Project.Character.Combat;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Verifies the Ragnarok Online stat-derived formulas in <see cref="DerivedStatFormulas"/>.
    /// </summary>
    public class DerivedStatFormulasTests
    {
        /// <summary>One percent of the base max health is added per point of VIT.</summary>
        [Test]
        public void MaxHealthBonus_AddsOnePercentPerVitality()
        {
            Assert.AreEqual(0, DerivedStatFormulas.MaxHealthBonus(1000, 0));
            Assert.AreEqual(100, DerivedStatFormulas.MaxHealthBonus(1000, 10));
            Assert.AreEqual(500, DerivedStatFormulas.MaxHealthBonus(1000, 50));
        }

        /// <summary>One percent of the base max mana is added per point of INT.</summary>
        [Test]
        public void MaxManaBonus_AddsOnePercentPerIntelligence()
        {
            Assert.AreEqual(0, DerivedStatFormulas.MaxManaBonus(200, 0));
            Assert.AreEqual(40, DerivedStatFormulas.MaxManaBonus(200, 20));
        }

        /// <summary>Without DEX or INT the cast time is untouched.</summary>
        [Test]
        public void CastTimeMultiplier_IsOneWithoutStats()
        {
            Assert.AreEqual(1f, DerivedStatFormulas.CastTimeMultiplier(0, 0), 0.0001f);
        }

        /// <summary>DEX counts double and the reduction is divided by 530.</summary>
        [Test]
        public void CastTimeMultiplier_ReducesByDexTimesTwoPlusIntOver530()
        {
            Assert.AreEqual(1f - 265f / 530f, DerivedStatFormulas.CastTimeMultiplier(100, 65), 0.0001f);
        }

        /// <summary>Enough DEX and INT make casting instant, never negative.</summary>
        [Test]
        public void CastTimeMultiplier_ClampsAtZero()
        {
            Assert.AreEqual(0f, DerivedStatFormulas.CastTimeMultiplier(999, 999), 0.0001f);
        }

        /// <summary>The resist chance is VIT plus three percent, capped at one.</summary>
        [Test]
        public void StatusResistChance_IsVitalityPlusThreePercentClamped()
        {
            Assert.AreEqual(0.03f, DerivedStatFormulas.StatusResistChance(0), 0.0001f);
            Assert.AreEqual(0.53f, DerivedStatFormulas.StatusResistChance(50), 0.0001f);
            Assert.AreEqual(1f, DerivedStatFormulas.StatusResistChance(200), 0.0001f);
        }
    }
}
