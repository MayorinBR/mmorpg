using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Snapshot of a dropped item taken when its map unloads: what it was, where it lay and the
    /// absolute <see cref="Time.time"/> at which it expires, enough to recreate it later with the
    /// time it had left.
    /// </summary>
    public readonly struct DroppedItemState
    {
        /// <summary>
        /// Creates a snapshot of a dropped item.
        /// </summary>
        /// <param name="item">The item the pickup grants.</param>
        /// <param name="quantity">The quantity the pickup grants.</param>
        /// <param name="position">World position where the pickup lay.</param>
        /// <param name="expiresAtTime">Absolute <see cref="Time.time"/> at which the pickup disappears.</param>
        public DroppedItemState(ItemDefinition item, int quantity, Vector3 position, float expiresAtTime)
        {
            Item = item;
            Quantity = quantity;
            Position = position;
            ExpiresAtTime = expiresAtTime;
        }

        /// <summary>Gets the item the pickup grants.</summary>
        public ItemDefinition Item { get; }

        /// <summary>Gets the quantity the pickup grants.</summary>
        public int Quantity { get; }

        /// <summary>Gets the world position where the pickup lay.</summary>
        public Vector3 Position { get; }

        /// <summary>Gets the absolute <see cref="Time.time"/> at which the pickup disappears.</summary>
        public float ExpiresAtTime { get; }
    }
}
