using UnityEngine;
using Project.Combat;

namespace Project.Items
{
    /// <summary>
    /// A dropped item sitting in the world. Collected explicitly via
    /// <see cref="TryCollect"/> (click-based, with range checked by the
    /// caller), not automatically on contact.
    /// </summary>
    public class ItemPickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int quantity = 1;
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private DroppedItemIconSet iconSet;

        /// <summary>Gets the item this pickup represents, read-only for external observers like tooltip triggers.</summary>
        public ItemDefinition Item => item;

        /// <summary>Gets the quantity this pickup grants.</summary>
        public int Quantity => quantity;

        /// <summary>Gets whether this pickup was already collected.</summary>
        public bool IsCollected { get; private set; }

        private Camera mainCamera;

        private void Awake()
        {
            mainCamera = Camera.main;
            UpdateIconSprite();
        }

        private void LateUpdate()
        {
            if (mainCamera != null)
            {
                transform.forward = mainCamera.transform.forward;
            }
        }

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
            UpdateIconSprite();
        }

        /// <summary>
        /// Shows or hides the icon without disabling the pickup, which stays collectable. Used to blink the icon
        /// as the pickup is about to expire.
        /// </summary>
        /// <param name="visible">True to show the icon.</param>
        public void SetIconVisible(bool visible)
        {
            if (iconRenderer != null)
            {
                iconRenderer.enabled = visible;
            }
        }

        private void UpdateIconSprite()
        {
            if (iconRenderer != null && item != null)
            {
                iconRenderer.sprite = iconSet != null ? iconSet.GetIcon(item) : item.Icon;
            }
        }

        /// <summary>
        /// Attempts to add this pickup's item to the given inventory,
        /// destroying this pickup on success. The caller is responsible for
        /// range checking before calling this. Publishes a
        /// <see cref="PlayerFeedbackChannel"/> message on failure (in
        /// practice, always the carried-weight cap) so the attempt doesn't
        /// fail with no explanation.
        /// </summary>
        /// <param name="inventory">The inventory to add the item to.</param>
        /// <returns>True if the item was fully added and the pickup was collected.</returns>
        public bool TryCollect(PlayerInventory inventory)
        {
            if (!inventory.Items.TryAddItem(item, quantity))
            {
                PlayerFeedbackChannel.Publish($"Maximum weight reached: couldn't pick up {item.ItemName}.");
                return false;
            }

            IsCollected = true;
            Destroy(gameObject);
            return true;
        }
    }
}