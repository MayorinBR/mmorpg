using NUnit.Framework;
using UnityEngine;

namespace Project.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="HitChanceCalculator"/>. The roll is random, so each case
    /// samples many rolls with a fixed seed and checks the observed hit rate.
    /// </summary>
    public class HitChanceCalculatorTests
    {
        private const int Samples = 20000;
        private const float Tolerance = 0.02f;

        /// <summary>
        /// The hit rate follows 100 + HIT - FLEE, clamped between 5% and 95%.
        /// </summary>
        [TestCase(0, 50, 0.5f)]
        [TestCase(0, 70, 0.3f)]
        [TestCase(50, 50, 0.95f)]
        [TestCase(200, 0, 0.95f)]
        [TestCase(0, 200, 0.05f)]
        public void RollHit_MatchesClampedHitChance(int attackerHit, int targetFlee, float expectedRate)
        {
            Random.InitState(1234);
            var hits = 0;

            for (var i = 0; i < Samples; i++)
            {
                if (HitChanceCalculator.RollHit(attackerHit, targetFlee))
                {
                    hits++;
                }
            }

            Assert.AreEqual(expectedRate, hits / (float)Samples, Tolerance);
        }
    }
}
