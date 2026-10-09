using NUnit.Framework;

namespace Project.AI.Tests
{
    /// <summary>
    /// Tests for <see cref="SpawnRules"/>.
    /// </summary>
    public class SpawnRulesTests
    {
        [TestCase(0.0f, 0)]
        [TestCase(0.59f, 0)]
        [TestCase(0.6f, 1)]
        [TestCase(0.99f, 1)]
        public void PickWeightedIndex_SplitsRangeByWeight(float roll, int expected)
        {
            Assert.AreEqual(expected, SpawnRules.PickWeightedIndex(new[] { 60f, 40f }, roll));
        }

        [Test]
        public void PickWeightedIndex_SkipsNonPositiveWeights()
        {
            Assert.AreEqual(1, SpawnRules.PickWeightedIndex(new[] { 0f, 5f, 0f }, 0.9f));
        }

        [Test]
        public void PickWeightedIndex_NoPositiveWeight_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SpawnRules.PickWeightedIndex(new[] { 0f, 0f }, 0.5f));
            Assert.AreEqual(-1, SpawnRules.PickWeightedIndex(new float[0], 0.5f));
        }

        [TestCase(0.5f, 0.4f, 0, 1, true)]
        [TestCase(0.5f, 0.6f, 0, 1, false)]
        [TestCase(0.5f, 0.1f, 1, 1, false)]
        [TestCase(0f, 0f, 0, 1, false)]
        public void ShouldSpawnBoss_RespectsChanceAndLimit(float chance, float roll, int spawned, int max, bool expected)
        {
            Assert.AreEqual(expected, SpawnRules.ShouldSpawnBoss(chance, roll, spawned, max));
        }

        [Test]
        public void StableHash_SameTextGivesSameValue_DifferentTextDiffers()
        {
            Assert.AreEqual(SpawnRules.StableHash("Map/Area"), SpawnRules.StableHash("Map/Area"));
            Assert.AreNotEqual(SpawnRules.StableHash("Map/AreaA"), SpawnRules.StableHash("Map/AreaB"));
        }
    }
}
