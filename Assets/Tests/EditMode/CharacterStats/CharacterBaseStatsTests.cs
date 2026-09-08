using NUnit.Framework;
using Project.Character.Stats;

namespace Project.Character.Stats.Tests
{
    /// <summary>
    /// Covers <see cref="CharacterBaseStats"/> in isolation, with no Unity
    /// runtime dependency — the class is plain C#, so these run as EditMode
    /// tests with no scene or Play Mode needed. Closes part of the
    /// "no automated tests" gap noted in FUTURE_IMPROVEMENTS.md for the
    /// project's pure logic classes.
    /// </summary>
    public class CharacterBaseStatsTests
    {
        [Test]
        public void TryIncreaseStat_WhenEnoughPointsAvailable_RaisesValueAndSpendsCost()
        {
            var stats = new CharacterBaseStats(new FlatStatPointCostStrategy());
            stats.GrantPoints(1);

            var result = stats.TryIncreaseStat(StatType.Strength);

            Assert.IsTrue(result);
            Assert.AreEqual(2, stats.GetValue(StatType.Strength));
            Assert.AreEqual(0, stats.AvailablePoints);
        }

        [Test]
        public void TryIncreaseStat_WhenNotEnoughPointsAvailable_LeavesValueUnchanged()
        {
            var stats = new CharacterBaseStats(new FlatStatPointCostStrategy());

            var result = stats.TryIncreaseStat(StatType.Strength);

            Assert.IsFalse(result);
            Assert.AreEqual(1, stats.GetValue(StatType.Strength));
            Assert.AreEqual(0, stats.AvailablePoints);
        }

        [Test]
        public void TryIncreaseStat_AtMaximumValue_ReturnsFalseAndDoesNotSpendPoints()
        {
            var stats = new CharacterBaseStats(new FlatStatPointCostStrategy());
            stats.SetValue(StatType.Strength, 99);
            stats.GrantPoints(5);

            var result = stats.TryIncreaseStat(StatType.Strength);

            Assert.IsFalse(result);
            Assert.AreEqual(99, stats.GetValue(StatType.Strength));
            Assert.AreEqual(5, stats.AvailablePoints);
        }

        [Test]
        public void ResetToMinimum_WithNoPointsSpent_RefundsZeroAndKeepsExistingPoints()
        {
            var stats = new CharacterBaseStats(new FlatStatPointCostStrategy());
            stats.GrantPoints(10);

            var refunded = stats.ResetToMinimum();

            Assert.AreEqual(0, refunded);
            Assert.AreEqual(10, stats.AvailablePoints);
            Assert.AreEqual(1, stats.GetValue(StatType.Strength));
        }

        [Test]
        public void ResetToMinimum_WithFlatCostStrategy_RefundsExactlyWhatWasSpent()
        {
            var stats = new CharacterBaseStats(new FlatStatPointCostStrategy());
            stats.GrantPoints(4);
            stats.TryIncreaseStat(StatType.Strength);
            stats.TryIncreaseStat(StatType.Strength);
            stats.TryIncreaseStat(StatType.Vitality);

            var refunded = stats.ResetToMinimum();

            Assert.AreEqual(3, refunded);
            Assert.AreEqual(1, stats.GetValue(StatType.Strength));
            Assert.AreEqual(1, stats.GetValue(StatType.Vitality));
            Assert.AreEqual(4, stats.AvailablePoints);
        }

        [Test]
        public void ResetToMinimum_WithRagnarokCostStrategy_RefundsBandedCostNotFlatValue()
        {
            // Raising Strength from 1 to 12 (11 increments) crosses the
            // 1-10/11-20 cost band: the first 10 increments cost 2 points
            // each and the 11th costs 3, for 23 total - not 11, which a
            // naive "points spent = current value - 1" refund would assume.
            var stats = new CharacterBaseStats(new RagnarokStatPointCostStrategy());
            stats.GrantPoints(23);

            for (var i = 0; i < 11; i++)
            {
                Assert.IsTrue(stats.TryIncreaseStat(StatType.Dexterity));
            }

            Assert.AreEqual(12, stats.GetValue(StatType.Dexterity));
            Assert.AreEqual(0, stats.AvailablePoints);

            var refunded = stats.ResetToMinimum();

            Assert.AreEqual(23, refunded);
            Assert.AreEqual(1, stats.GetValue(StatType.Dexterity));
            Assert.AreEqual(23, stats.AvailablePoints);
        }

        [Test]
        public void ResetToMinimum_AfterReset_StatCanBeRaisedAgainAtOriginalCost()
        {
            var stats = new CharacterBaseStats(new RagnarokStatPointCostStrategy());
            stats.GrantPoints(2);
            stats.TryIncreaseStat(StatType.Luck);
            stats.ResetToMinimum();

            var result = stats.TryIncreaseStat(StatType.Luck);

            Assert.IsTrue(result);
            Assert.AreEqual(2, stats.GetValue(StatType.Luck));
            Assert.AreEqual(0, stats.AvailablePoints);
        }
    }
}
