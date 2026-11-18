using NUnit.Framework;

namespace Project.Items.Tests
{
    /// <summary>
    /// Verifies the save-file bit mask conversion in <see cref="EquipmentSlotMask"/>.
    /// </summary>
    public class EquipmentSlotMaskTests
    {
        /// <summary>An empty list packs into zero and unpacks into an empty list.</summary>
        [Test]
        public void EmptySlots_RoundTripToEmpty()
        {
            Assert.AreEqual(0, EquipmentSlotMask.ToMask(new EquipmentSlot[0]));
            Assert.AreEqual(0, EquipmentSlotMask.FromMask(0).Count);
        }

        /// <summary>A two-handed weapon's slots survive a round trip in ascending order.</summary>
        [Test]
        public void TwoHandedSlots_RoundTrip()
        {
            var slots = new[] { EquipmentSlot.LeftHand, EquipmentSlot.RightHand };

            var restored = EquipmentSlotMask.FromMask(EquipmentSlotMask.ToMask(slots));

            CollectionAssert.AreEquivalent(slots, restored);
        }

        /// <summary>Each slot maps to its own bit.</summary>
        [Test]
        public void EverySlot_UsesItsOwnBit()
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                var mask = EquipmentSlotMask.ToMask(new[] { slot });

                Assert.AreEqual(1 << (int)slot, mask);
                Assert.AreEqual(slot, EquipmentSlotMask.FromMask(mask)[0]);
            }
        }
    }
}
