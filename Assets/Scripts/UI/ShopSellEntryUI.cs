using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// A single sell-tray slot. Starts empty and accepts an item dragged
    /// from an <see cref="InventorySlotUI"/>, reporting the drop to
    /// whoever called <see cref="Init"/> so it can decide whether to
    /// accept it. Once occupied, lets the player edit how many to sell
    /// and empties itself when its own button (relabeled "X") is
    /// clicked. Tracks the dragged item type rather than the specific
    /// inventory slot it was dragged from, so its quantity can go up to
    /// however many units of that item the player holds in total — even
    /// non-stackable items, which occupy one inventory slot per unit;
    /// <see cref="ShopWindowUI"/> is the one that sums that total and, at
    /// sell time, gathers the requested amount from as many slots as it
    /// takes. This component is purely a view over one queued sale.
    /// </summary>
    public class ShopSellEntryUI : MonoBehaviour, IDropHandler
    {
        private const int MinQuantity = 1;
        private const string EmptyPlaceholder = "Drop item here";

        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button sellButton;

        private Action<ShopSellEntryUI, ItemDefinition> onItemDropped;
        private int availableQuantity;

        /// <summary>Gets a value indicating whether this slot currently holds an item.</summary>
        public bool HasItem => Item != null;

        /// <summary>Gets the item currently queued in this slot, or null if empty.</summary>
        public ItemDefinition Item { get; private set; }

        /// <summary>Gets the quantity currently typed into this slot, clamped to what's available. Zero if empty.</summary>
        public int Quantity => GetQuantity();

        private void Awake()
        {
            var sellButtonLabel = sellButton != null ? sellButton.GetComponentInChildren<TMP_Text>() : null;

            if (sellButtonLabel != null)
            {
                sellButtonLabel.text = "X";
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(Clear);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Registers the callback invoked when an inventory item is
        /// dropped onto this slot. Called once by <see cref="ShopWindowUI"/>
        /// when the sell tray is built.
        /// </summary>
        /// <param name="callback">Invoked with this slot and the dragged item's <see cref="ItemDefinition"/>.</param>
        public void Init(Action<ShopSellEntryUI, ItemDefinition> callback)
        {
            onItemDropped = callback;
        }

        /// <summary>Assigns an item to this slot, defaulting its sell quantity to one.</summary>
        /// <param name="sourceItem">The item to display.</param>
        /// <param name="sourceAvailableQuantity">How many units of the item the player holds in total, across every inventory slot.</param>
        public void Assign(ItemDefinition sourceItem, int sourceAvailableQuantity)
        {
            Item = sourceItem;
            availableQuantity = Mathf.Max(MinQuantity, sourceAvailableQuantity);

            UpdateVisuals();
        }

        /// <summary>Empties this slot, ready to accept another dropped item.</summary>
        public void Clear()
        {
            Item = null;
            availableQuantity = 0;

            UpdateVisuals();
        }

        /// <inheritdoc />
        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null)
            {
                return;
            }

            var sourceSlot = eventData.pointerDrag.GetComponent<InventorySlotUI>();

            if (sourceSlot == null || sourceSlot.CurrentItem == null)
            {
                return;
            }

            onItemDropped?.Invoke(this, sourceSlot.CurrentItem);
        }

        private void UpdateVisuals()
        {
            var hasItem = HasItem;

            if (iconImage != null)
            {
                iconImage.enabled = hasItem && Item.Icon != null;
                iconImage.sprite = hasItem ? Item.Icon : null;
            }

            if (nameText != null)
            {
                nameText.text = hasItem ? Item.ItemName : EmptyPlaceholder;
            }

            if (priceText != null)
            {
                priceText.text = hasItem ? $"{Item.SellPrice} Zeny" : string.Empty;
            }

            if (quantityInput != null)
            {
                quantityInput.text = hasItem ? MinQuantity.ToString() : string.Empty;
                quantityInput.interactable = hasItem;
            }

            if (sellButton != null)
            {
                sellButton.gameObject.SetActive(hasItem);
            }
        }

        private int GetQuantity()
        {
            if (!HasItem)
            {
                return 0;
            }

            if (quantityInput == null || !int.TryParse(quantityInput.text, out var quantity))
            {
                return Mathf.Min(MinQuantity, availableQuantity);
            }

            return Mathf.Clamp(quantity, MinQuantity, availableQuantity);
        }
    }
}
