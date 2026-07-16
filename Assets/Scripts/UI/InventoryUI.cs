using UnityEngine;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Builds a grid of <see cref="InventorySlotUI"/> matching the player's
    /// inventory capacity, and keeps it in sync with
    /// <see cref="Inventory.InventoryChanged"/>. Slot views are built and
    /// the initial sync happens in <see cref="Start"/> to guarantee
    /// <see cref="PlayerInventory.Items"/> has already been created by
    /// <see cref="PlayerInventory.Awake"/>.
    /// </summary>
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private PlayerInventory playerInventory;
        [SerializeField] private InventorySlotUI slotPrefab;
        [SerializeField] private Transform slotsParent;

        private InventorySlotUI[] slotViews;

        private void OnEnable()
        {
            if (playerInventory != null && playerInventory.Items != null)
            {
                playerInventory.Items.InventoryChanged -= RefreshAll;
                playerInventory.Items.InventoryChanged += RefreshAll;

                if (slotViews != null)
                {
                    RefreshAll();
                }
            }
        }

        private void Start()
        {
            BuildSlotViews();

            // Defensive: avoids a duplicate subscription in case OnEnable
            // already subscribed successfully before this ran.
            playerInventory.Items.InventoryChanged -= RefreshAll;
            playerInventory.Items.InventoryChanged += RefreshAll;

            RefreshAll();
        }

        private void OnDisable()
        {
            if (playerInventory != null && playerInventory.Items != null)
            {
                playerInventory.Items.InventoryChanged -= RefreshAll;
            }
        }

        private void BuildSlotViews()
        {
            var capacity = playerInventory.Items.Capacity;
            slotViews = new InventorySlotUI[capacity];

            for (var i = 0; i < capacity; i++)
            {
                slotViews[i] = Instantiate(slotPrefab, slotsParent);
            }
        }

        private void RefreshAll()
        {
            for (var i = 0; i < slotViews.Length; i++)
            {
                slotViews[i].SetSlot(playerInventory.Items.GetSlot(i));
            }
        }
    }
}