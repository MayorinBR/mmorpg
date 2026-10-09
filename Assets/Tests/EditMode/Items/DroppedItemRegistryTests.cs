using NUnit.Framework;
using UnityEngine;

namespace Project.Items.Tests
{
    /// <summary>
    /// Tests <see cref="DroppedItemRegistry"/>: items come back once, only while they have not expired.
    /// </summary>
    public class DroppedItemRegistryTests
    {
        private static DroppedItemState StateExpiringAt(float expiresAtTime)
        {
            return new DroppedItemState(ItemTestFactory.CreateItem(), 1, Vector3.zero, expiresAtTime);
        }

        [Test]
        public void TakeActive_ReturnsSavedItemsOnlyOnce()
        {
            DroppedItemRegistry.Save("MapOnce", StateExpiringAt(30f));

            Assert.AreEqual(1, DroppedItemRegistry.TakeActive("MapOnce", 10f).Count);
            Assert.AreEqual(0, DroppedItemRegistry.TakeActive("MapOnce", 10f).Count);
        }

        [Test]
        public void TakeActive_DiscardsExpiredItems()
        {
            DroppedItemRegistry.Save("MapExpired", StateExpiringAt(30f));
            DroppedItemRegistry.Save("MapExpired", StateExpiringAt(50f));

            var active = DroppedItemRegistry.TakeActive("MapExpired", 40f);

            Assert.AreEqual(1, active.Count);
            Assert.AreEqual(50f, active[0].ExpiresAtTime);
        }

        [Test]
        public void TakeActive_ReturnsNothingForUnknownScene()
        {
            Assert.AreEqual(0, DroppedItemRegistry.TakeActive("MapUnknown", 0f).Count);
        }
    }
}
