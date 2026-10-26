using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Displays a single inventory slot's icon and quantity, raises
    /// <see cref="Clicked"/> with its own index when clicked, shows the
    /// shared item tooltip on hover, and acts as a drag source (e.g. for
    /// dropping onto a <see cref="ShopSellEntryUI"/> tray slot). Purely
    /// presentational otherwise — it doesn't decide what clicking or
    /// dragging should do.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Button button;

        private int slotIndex;
        private ItemDefinition currentItem;
        private int currentQuantity;
        private RectTransform dragIcon;

        /// <summary>Raised when this slot is clicked, carrying its inventory index.</summary>
        public event Action<int> Clicked;

        /// <summary>Gets the inventory index this slot currently represents.</summary>
        public int SlotIndex => slotIndex;

        /// <summary>Gets the item currently displayed, or null if this slot is empty.</summary>
        public ItemDefinition CurrentItem => currentItem;

        /// <summary>Gets the quantity currently displayed. Meaningless if <see cref="CurrentItem"/> is null.</summary>
        public int CurrentQuantity => currentQuantity;

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
            currentItem = slot.IsEmpty ? null : slot.Item;
            currentQuantity = slot.IsEmpty ? 0 : slot.Quantity;

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

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentItem == null)
            {
                return;
            }

            var rootCanvas = iconImage.canvas.rootCanvas;
            var dragIconObject = new GameObject("DragIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            dragIconObject.transform.SetParent(rootCanvas.transform, false);
            dragIconObject.transform.SetAsLastSibling();

            var dragImage = dragIconObject.GetComponent<Image>();
            dragImage.sprite = currentItem.Icon;
            dragImage.raycastTarget = false;
            dragIconObject.GetComponent<CanvasGroup>().blocksRaycasts = false;

            dragIcon = (RectTransform)dragIconObject.transform;
            dragIcon.sizeDelta = iconImage.rectTransform.sizeDelta;
            dragIcon.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                dragIcon.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                Destroy(dragIcon.gameObject);
                dragIcon = null;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentItem != null && ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Show(currentItem, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Hide();
            }
        }
    }
}