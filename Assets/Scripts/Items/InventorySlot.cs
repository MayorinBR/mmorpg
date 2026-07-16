namespace Project.Items
{
    /// <summary>
    /// Represents the contents of a single inventory slot: which item it
    /// holds and how many units. An empty slot has a null <see cref="Item"/>.
    /// </summary>
    public readonly struct InventorySlot
    {
        /// <summary>
        /// Initializes a slot holding the given item and quantity.
        /// </summary>
        /// <param name="item">The item occupying the slot, or null for an empty slot.</param>
        /// <param name="quantity">The number of units in this slot.</param>
        public InventorySlot(ItemDefinition item, int quantity)
        {
            Item = item;
            Quantity = quantity;
        }

        /// <summary>Gets the item occupying this slot, or null if empty.</summary>
        public ItemDefinition Item { get; }

        /// <summary>Gets the number of units in this slot.</summary>
        public int Quantity { get; }

        /// <summary>Gets a value indicating whether this slot holds no item.</summary>
        public bool IsEmpty => Item == null;

        /// <summary>Gets a reusable empty slot value.</summary>
        public static InventorySlot Empty => new InventorySlot(null, 0);
    }
}