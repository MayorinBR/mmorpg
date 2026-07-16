using System;

namespace Project.Items
{
    /// <summary>
    /// Manages a fixed-capacity collection of <see cref="InventorySlot"/>s,
    /// handling stacking automatically when adding stackable items. Kept as
    /// a plain C# class (not a MonoBehaviour) so it can be unit tested and
    /// reused independently of the Unity component that owns it.
    /// </summary>
    public class Inventory
    {
        private readonly InventorySlot[] slots;

        /// <summary>Raised whenever any slot's contents change.</summary>
        public event Action InventoryChanged;

        /// <summary>Gets the total number of slots.</summary>
        public int Capacity => slots.Length;

        /// <summary>
        /// Initializes an empty inventory with the given number of slots.
        /// </summary>
        /// <param name="capacity">The total number of slots available.</param>
        public Inventory(int capacity)
        {
            slots = new InventorySlot[capacity];

            for (var i = 0; i < capacity; i++)
            {
                slots[i] = InventorySlot.Empty;
            }
        }

        /// <summary>
        /// Gets the contents of the slot at the given index.
        /// </summary>
        /// <param name="index">The slot index.</param>
        /// <returns>The slot's current contents.</returns>
        public InventorySlot GetSlot(int index)
        {
            return slots[index];
        }

        /// <summary>
        /// Attempts to add a quantity of an item, filling existing stacks
        /// before using empty slots.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The number of units to add.</param>
        /// <returns>True if the full quantity was added; false if there wasn't enough space for all of it.</returns>
        public bool TryAddItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            var remaining = quantity;
            remaining = FillExistingStacks(item, remaining);
            remaining = FillEmptySlots(item, remaining);

            if (remaining < quantity)
            {
                InventoryChanged?.Invoke();
            }

            return remaining == 0;
        }

        /// <summary>
        /// Clears the slot at the given index.
        /// </summary>
        /// <param name="index">The slot index to clear.</param>
        public void RemoveAt(int index)
        {
            slots[index] = InventorySlot.Empty;
            InventoryChanged?.Invoke();
        }

        private int FillExistingStacks(ItemDefinition item, int remaining)
        {
            if (!item.IsStackable)
            {
                return remaining;
            }

            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                var slot = slots[i];

                if (slot.IsEmpty || slot.Item != item || slot.Quantity >= item.MaxStackSize)
                {
                    continue;
                }

                var spaceInStack = item.MaxStackSize - slot.Quantity;
                var amountToAdd = Math.Min(spaceInStack, remaining);
                slots[i] = new InventorySlot(item, slot.Quantity + amountToAdd);
                remaining -= amountToAdd;
            }

            return remaining;
        }

        private int FillEmptySlots(ItemDefinition item, int remaining)
        {
            for (var i = 0; i < slots.Length && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    continue;
                }

                var stackLimit = item.IsStackable ? item.MaxStackSize : 1;
                var amountToAdd = Math.Min(stackLimit, remaining);
                slots[i] = new InventorySlot(item, amountToAdd);
                remaining -= amountToAdd;
            }

            return remaining;
        }
    }
}