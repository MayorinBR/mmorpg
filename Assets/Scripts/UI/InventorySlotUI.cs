using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Displays a single inventory slot's icon and quantity. Purely
    /// presentational — it doesn't know about the inventory as a whole,
    /// only how to render the <see cref="InventorySlot"/> it's given.
    /// </summary>
    public class InventorySlotUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text quantityText;

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