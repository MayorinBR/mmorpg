using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// A dropped item sitting in the world. Adds itself to the first
    /// <see cref="PlayerInventory"/> that touches its trigger collider, then
    /// destroys itself once fully picked up.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;

        /// <summary>
        /// Configures which item and quantity this pickup represents,
        /// typically called right after instantiating it from a loot drop.
        /// </summary>
        /// <param name="droppedItem">The item to grant on pickup.</param>
        /// <param name="droppedQuantity">The quantity to grant on pickup.</param>
        public void Initialize(ItemDefinition droppedItem, int droppedQuantity)
        {
            item = droppedItem;
            quantity = droppedQuantity;
        }

        private void OnTriggerEnter(Collider other)
        {
            var inventory = other.GetComponentInParent<PlayerInventory>();

            if (inventory == null)
            {
                return;
            }

            if (inventory.Items.TryAddItem(item, quantity))
            {
                Destroy(gameObject);
            }
        }
    }
}