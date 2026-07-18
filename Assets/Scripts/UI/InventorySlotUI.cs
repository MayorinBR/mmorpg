using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Displays a single inventory slot's icon and quantity, and raises
    /// <see cref="Clicked"/> with its own index when clicked. Purely
    /// presentational otherwise — it doesn't decide what clicking should do.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Button button;

        private int slotIndex;

        /// <summary>Raised when this slot is clicked, carrying its inventory index.</summary>
        public event Action<int> Clicked;

        private void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(slotIndex));
        }

        /// <summary>
        /// Sets which inventory index this view represents, used when
        /// raising <see cref="Clicked"/>.
        /// </summary>
        /// <param name="index">The inventory slot index.</param>
        public void SetIndex(int index)
        {
            slotIndex = index;
        }

        /// <summary>
        /// Updates this slot's visuals to reflect the given inventory slot contents.
        /// </summary>
        /// <param name="slot">The slot data to display.</param>
        public void SetSlot(InventorySlot slot)
        {
            if (slot.IsEmpty)
            {
                iconImage.enabled = false;
                quantityText.text = string.Empty;
                return;
            }

            iconImage.enabled = true;
            iconImage.sprite = slot.Item.Icon;
            quantityText.text = slot.Item.IsStackable && slot.Quantity > 1
                ? slot.Quantity.ToString()
                : string.Empty;
        }
    }
}