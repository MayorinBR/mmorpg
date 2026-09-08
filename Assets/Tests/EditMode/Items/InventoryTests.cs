using NUnit.Framework;
using Project.Items;

namespace Project.Items.Tests
{
    /// <summary>
    /// Covers <see cref="Inventory"/> in isolation — it is a plain C# class
    /// by design (see its own doc comment), so these run as EditMode tests
    /// with no scene or Play Mode needed. Closes the <c>Inventory.TryAddItem</c>
    /// gap the project's FUTURE_IMPROVEMENTS.md called out as a good
    /// candidate, since stacking and weight-capacity edge cases have caused
    /// subtle bugs before.
    /// </summary>
    public class InventoryTests
    {
        [Test]
        public void TryAddItem_IntoEmptyInventory_FillsFirstSlot()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 1f);

            var result = inventory.TryAddItem(item, 5);

            Assert.IsTrue(result);
            Assert.AreEqual(item, inventory.GetSlot(0).Item);
            Assert.AreEqual(5, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(5f, inventory.CurrentWeight);
        }

        [Test]
        public void TryAddItem_StackableItemAlreadyPresent_FillsExistingStackFirst()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(isStackable: true, maxStackSize: 99, weight: 1f);
            inventory.TryAddItem(item, 10);

            inventory.TryAddItem(item, 5);

            Assert.AreEqual(15, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(0, inventory.GetSlot(1).Quantity);
        }

        [Test]
        public void TryAddItem_ExceedsMaxStackSize_SpillsRemainderIntoNextSlot()
        {
            var inventory = new Inventory(maxCarryWeight: 1000f);
            var item = ItemTestFactory.CreateItem(isStackable: true, maxStackSize: 10, weight: 0f);

            inventory.TryAddItem(item, 15);

            Assert.AreEqual(10, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(5, inventory.GetSlot(1).Quantity);
        }

        [Test]
        public void TryAddItem_NonStackableItem_UsesOneSlotPerUnit()
        {
            var inventory = new Inventory(maxCarryWeight: 1000f);
            var item = ItemTestFactory.CreateItem(isStackable: false, weight: 0f);

            inventory.TryAddItem(item, 3);

            Assert.AreEqual(1, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(1, inventory.GetSlot(1).Quantity);
            Assert.AreEqual(1, inventory.GetSlot(2).Quantity);
        }

        [Test]
        public void TryAddItem_BeyondWeightCapacity_AddsPartialAmountAndReturnsFalse()
        {
            var inventory = new Inventory(maxCarryWeight: 10f);
            var item = ItemTestFactory.CreateItem(weight: 2f, maxStackSize: 99);

            var result = inventory.TryAddItem(item, 10);

            Assert.IsFalse(result);
            Assert.AreEqual(5, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(10f, inventory.CurrentWeight);
        }

        [Test]
        public void TryAddItem_AtFullWeightCapacity_AddsNothingAndReturnsFalse()
        {
            var inventory = new Inventory(maxCarryWeight: 5f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            inventory.TryAddItem(item, 5);

            var result = inventory.TryAddItem(item, 1);

            Assert.IsFalse(result);
            Assert.AreEqual(5, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(5f, inventory.CurrentWeight);
        }

        [Test]
        public void TryAddItem_MoreUnitsThanExistingSlots_GrowsANewPage()
        {
            var inventory = new Inventory(maxCarryWeight: 1000f);
            var item = ItemTestFactory.CreateItem(isStackable: false, weight: 0f);
            var startingSlotCount = inventory.SlotCount;

            inventory.TryAddItem(item, startingSlotCount + 1);

            Assert.Greater(inventory.SlotCount, startingSlotCount);
        }

        [Test]
        public void GetAddableQuantity_ClampsToWhatWeightCapacityAllows()
        {
            var inventory = new Inventory(maxCarryWeight: 10f);
            var item = ItemTestFactory.CreateItem(weight: 3f);

            var addable = inventory.GetAddableQuantity(item, 10);

            Assert.AreEqual(3, addable);
        }

        [Test]
        public void GetAddableQuantity_WithNullItem_ReturnsZero()
        {
            var inventory = new Inventory(maxCarryWeight: 10f);

            var addable = inventory.GetAddableQuantity(null, 5);

            Assert.AreEqual(0, addable);
        }

        [Test]
        public void RemoveAt_ClearsSlotAndReducesCurrentWeight()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 2f);
            inventory.TryAddItem(item, 4);

            inventory.RemoveAt(0);

            Assert.IsTrue(inventory.GetSlot(0).IsEmpty);
            Assert.AreEqual(0f, inventory.CurrentWeight);
        }

        [Test]
        public void TryRemoveFromSlot_PartialQuantity_KeepsRemainderInSlot()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            inventory.TryAddItem(item, 10);

            var result = inventory.TryRemoveFromSlot(0, 4);

            Assert.IsTrue(result);
            Assert.AreEqual(6, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(6f, inventory.CurrentWeight);
        }

        [Test]
        public void TryRemoveFromSlot_FullQuantity_EmptiesTheSlot()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            inventory.TryAddItem(item, 3);

            inventory.TryRemoveFromSlot(0, 3);

            Assert.IsTrue(inventory.GetSlot(0).IsEmpty);
        }

        [Test]
        public void TryRemoveFromSlot_MoreThanAvailable_ReturnsFalseAndLeavesSlotUnchanged()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            inventory.TryAddItem(item, 2);

            var result = inventory.TryRemoveFromSlot(0, 3);

            Assert.IsFalse(result);
            Assert.AreEqual(2, inventory.GetSlot(0).Quantity);
        }

        [Test]
        public void SetSlot_ReplacesExistingContentsAndUpdatesWeight()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var originalItem = ItemTestFactory.CreateItem(name: "Original", weight: 1f);
            var replacementItem = ItemTestFactory.CreateItem(name: "Replacement", weight: 3f);
            inventory.TryAddItem(originalItem, 2);

            inventory.SetSlot(0, replacementItem, 4);

            Assert.AreEqual(replacementItem, inventory.GetSlot(0).Item);
            Assert.AreEqual(4, inventory.GetSlot(0).Quantity);
            Assert.AreEqual(12f, inventory.CurrentWeight);
        }

        [Test]
        public void EnsureSlotCount_GrowsInWholePagesUntilAtLeastTheRequestedCount()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var startingSlotCount = inventory.SlotCount;

            inventory.EnsureSlotCount(startingSlotCount + 1);

            Assert.GreaterOrEqual(inventory.SlotCount, startingSlotCount + 1);
        }

        [Test]
        public void EnsureSlotCount_WhenAlreadyEnoughSlots_DoesNothing()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var startingSlotCount = inventory.SlotCount;

            inventory.EnsureSlotCount(startingSlotCount - 1);

            Assert.AreEqual(startingSlotCount, inventory.SlotCount);
        }

        [Test]
        public void InventoryChanged_FiresOnSuccessfulAdd()
        {
            var inventory = new Inventory(maxCarryWeight: 100f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            var raised = false;
            inventory.InventoryChanged += () => raised = true;

            inventory.TryAddItem(item, 1);

            Assert.IsTrue(raised);
        }

        [Test]
        public void InventoryChanged_DoesNotFireWhenNothingCouldBeAdded()
        {
            var inventory = new Inventory(maxCarryWeight: 0f);
            var item = ItemTestFactory.CreateItem(weight: 1f);
            var raised = false;
            inventory.InventoryChanged += () => raised = true;

            inventory.TryAddItem(item, 1);

            Assert.IsFalse(raised);
        }
    }
}
