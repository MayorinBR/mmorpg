using NUnit.Framework;
using UnityEngine;
using Project.Persistence;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests <see cref="PlayerTravelPoints"/>: the save point, the memorized locations and their persistence.
    /// </summary>
    public class PlayerTravelPointsTests
    {
        private GameObject host;
        private PlayerTravelPoints points;

        /// <summary>Creates a fresh component for each test.</summary>
        [SetUp]
        public void SetUp()
        {
            host = new GameObject("TravelPoints");
            points = host.AddComponent<PlayerTravelPoints>();
        }

        /// <summary>Destroys the component created for the test.</summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(host);
        }

        /// <summary>A new character has no save point.</summary>
        [Test]
        public void GetSavePoint_Initially_IsNull()
        {
            Assert.IsNull(points.GetSavePoint());
        }

        /// <summary>The save point always comes first, followed by the memos in order.</summary>
        [Test]
        public void GetDestinations_ListsSavePointThenMemos()
        {
            points.SetSavePoint("Town", new Vector3(1f, 0f, 1f));
            points.TryAddMemo("Field", new Vector3(2f, 0f, 2f));

            var destinations = points.GetDestinations(4);

            Assert.AreEqual(2, destinations.Count);
            Assert.AreEqual("Town", destinations[0].SceneName);
            Assert.AreEqual("Field", destinations[1].SceneName);
        }

        /// <summary>The skill level caps how many destinations are offered.</summary>
        [Test]
        public void GetDestinations_RespectsLimit()
        {
            points.SetSavePoint("Town", Vector3.zero);
            points.TryAddMemo("A", Vector3.zero);
            points.TryAddMemo("B", Vector3.zero);

            Assert.AreEqual(1, points.GetDestinations(1).Count);
            Assert.AreEqual(2, points.GetDestinations(2).Count);
        }

        /// <summary>Only <see cref="PlayerTravelPoints.MaxMemos"/> locations can be memorized.</summary>
        [Test]
        public void TryAddMemo_BeyondMax_IsRejected()
        {
            for (var i = 0; i < PlayerTravelPoints.MaxMemos; i++)
            {
                Assert.IsTrue(points.TryAddMemo("Map", Vector3.zero));
            }

            Assert.IsFalse(points.TryAddMemo("Map", Vector3.zero));
            Assert.AreEqual(PlayerTravelPoints.MaxMemos, points.MemoCount);
        }

        /// <summary>The save point and memos survive a save and load.</summary>
        [Test]
        public void CaptureAndRestore_RoundTripsPoints()
        {
            points.SetSavePoint("Town", new Vector3(1f, 2f, 3f));
            points.TryAddMemo("Field", new Vector3(4f, 5f, 6f));
            var data = new PlayerSaveData();
            points.CaptureState(data);

            var other = new GameObject("Other").AddComponent<PlayerTravelPoints>();
            other.RestoreState(data);

            var savePoint = other.GetSavePoint().Value;
            Assert.AreEqual("Town", savePoint.SceneName);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), savePoint.Position);
            Assert.AreEqual(1, other.MemoCount);
            Assert.AreEqual(new Vector3(4f, 5f, 6f), other.GetDestinations(4)[1].Position);
            Object.DestroyImmediate(other.gameObject);
        }

        /// <summary>A save from before this feature existed leaves the current save point untouched.</summary>
        [Test]
        public void RestoreState_WithoutSavedPoint_KeepsCurrentSavePoint()
        {
            points.SetSavePoint("Town", Vector3.zero);

            points.RestoreState(new PlayerSaveData());

            Assert.IsTrue(points.HasSavePoint);
        }
    }
}
