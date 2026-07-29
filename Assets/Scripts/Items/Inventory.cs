using System;
using System.Collections.Generic;

namespace Project.Items
{
    /// <summary>
    /// Manages a weight-bounded, dynamically growing collection of
    /// <see cref="InventorySlot"/>s. Slots are added in pages as needed,
    /// limited only by <see cref="MaxCarryWeight"/> — there is no fixed
    /// slot count. Kept as a plain C# class (not a MonoBehaviour) so it can
    /// be unit tested and reused independently of the Unity component that
    /// owns it.
    /// </summary>
    public class Inventory
    {
        private const int SlotsPerPage = 20;

        private readonly List<InventorySlot> slots = new List<InventorySlot>();

        /// <summary>Raised whenever any slot's contents change.</summary>
        public event Action InventoryChanged;

        /// <summary>Gets the maximum total weight this inventory can carry.</summary>
        public float MaxCarryWeight { get; }

        /// <summary>Gets the combined weight of everything currently carried.</summary>
        public float CurrentWeight { get; private set; }

        /// <summary>Gets the number of slots currently allocated (grows in pages as needed).</summary>
        public int SlotCount => slots.Count;

        /// <summary>
        /// Initializes an inventory with one starting page of empty slots.
        /// </summary>
        /// <param name="maxCarryWeight">The maximum total weight this inventory can carry.</param>
        public Inventory(float maxCarryWeight)
        {
            MaxCarryWeight = maxCarryWeight;
            AddPage();
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
        /// before using or creating empty slots. New slot pages are added
        /// automatically if needed and weight capacity allows.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <param name="quantity">The number of units to add.</param>
        /// <returns>True only if the full requested quantity was added.</returns>
        public bool TryAddItem(ItemDefinition item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                return false;
            }

            var addable = ClampToWeightCapacity(item, quantity);

            if (addable <= 0)
            {
                return false;
            }

            var remaining = addable;
            remaining = FillExistingStacks(item, remaining);
            remaining = FillIntoSlots(item, remaining);

            var actuallyAdded = addable - remaining;

            if (actuallyAdded > 0)
            {
                CurrentWeight += actuallyAdded * item.Weight;
                InventoryChanged?.Invoke();
            }

            return actuallyAdded == quantity;
        }

        /// <summary>
        /// Clears the slot at the given index.
        /// </summary>
        /// <param name="index">The slot index to clear.</param>
        public void RemoveAt(int index)
        {
            var removedSlot = slots[index];

            if (!removedSlot.IsEmpty)
            {
                CurrentWeight -= removedSlot.Item.Weight * removedSlot.Quantity;
            }

            slots[index] = InventorySlot.Empty;
            InventoryChanged?.Invoke();
        }

        private int ClampToWeightCapacity(ItemDefinition item, int quantity)
        {
            if (item.Weight <= 0f)
            {
                return quantity;
            }

            var remainingWeightCapacity = MaxCarryWeight - CurrentWeight;
            var weightCapacityUnits = (int)(remainingWeightCapacity / item.Weight);
            return Math.Min(quantity, Math.Max(weightCapacityUnits, 0));
        }

        private void AddPage()
        {
            for (var i = 0; i < SlotsPerPage; i++)
            {
                slots.Add(InventorySlot.Empty);
            }
        }

        private int FillExistingStacks(ItemDefinition item, int remaining)
        {
            if (!item.IsStackable)
            {
                return remaining;
            }

            for (var i = 0; i < slots.Count && remaining > 0; i++)
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

        private int FillIntoSlots(ItemDefinition item, int remaining)
        {
            var index = 0;

            while (remaining > 0)
            {
                if (index >= slots.Count)
                {
                    AddPage();
                }

                var slot = slots[index];

                if (slot.IsEmpty)
                {
                    var stackLimit = item.IsStackable ? item.MaxStackSize : 1;
                    var amountToAdd = Math.Min(stackLimit, remaining);
                    slots[index] = new InventorySlot(item, amountToAdd);
                    remaining -= amountToAdd;
                }

                index++;
            }

            return remaining;
        }

        /// <summary>
        /// Directly replaces the contents of a slot, bypassing stacking and
        /// weight-capacity logic. Intended for swaps where the caller already
        /// knows exactly which slot should hold the item (e.g. exchanging
        /// equipped ammo with the stack that's replacing it).
        /// </summary>
        /// <param name="index">The slot index to overwrite.</param>
        /// <param name="item">The item to place in the slot.</param>
        /// <param name="quantity">The quantity to place in the slot.</param>
        public void SetSlot(int index, ItemDefinition item, int quantity)
        {
            var previousSlot = slots[index];

            if (!previousSlot.IsEmpty)
            {
                CurrentWeight -= previousSlot.Item.Weight * previousSlot.Quantity;
            }

            slots[index] = new InventorySlot(item, quantity);
            CurrentWeight += item.Weight * quantity;
            InventoryChanged?.Invoke();
        }
    }
}