using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// A single row in a shop's buy list: icon, name, price, a quantity
    /// input field, and a Buy button that reports back which item and how
    /// many units were bought. Purely presentational, mirroring
    /// <see cref="InventorySlotUI"/> — it doesn't decide whether the
    /// purchase can succeed.
    /// </summary>
    public class ShopBuyEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const int MinQuantity = 1;

        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private TMP_InputField quantityInput;
        [SerializeField] private Button buyButton;

        private ItemDefinition item;
        private Action<ItemDefinition, int> onBuy;

        /// <summary>
        /// Configures this row to display the given item and invoke the
        /// given callback with the chosen quantity when its Buy button is
        /// clicked.
        /// </summary>
        /// <param name="itemToSell">The item this row represents.</param>
        /// <param name="buyCallback">Invoked with this row's item and chosen quantity when Buy is clicked.</param>
        public void Setup(ItemDefinition itemToSell, Action<ItemDefinition, int> buyCallback)
        {
            if (itemToSell == null)
            {
                gameObject.SetActive(false);
                return;
            }

            item = itemToSell;
            onBuy = buyCallback;

            if (iconImage != null)
            {
                iconImage.enabled = item.Icon != null;
                iconImage.sprite = item.Icon;
            }

            if (nameText != null)
            {
                nameText.text = item.ItemName;
            }

            if (priceText != null)
            {
                priceText.text = $"{item.BuyPrice} Zeny";
            }

            if (quantityInput != null)
            {
                quantityInput.text = MinQuantity.ToString();
            }

            if (buyButton != null)
            {
                buyButton.onClick.RemoveAllListeners();
                buyButton.onClick.AddListener(() => onBuy?.Invoke(item, GetQuantity()));
            }
        }

        /// <summary>Shows the shared item tooltip for this row's item.</summary>
        /// <param name="eventData">The pointer event data.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (item != null && ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Show(item, eventData.position);
            }
        }

        /// <summary>Hides the shared item tooltip.</summary>
        /// <param name="eventData">The pointer event data.</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Hide();
            }
        }

        private int GetQuantity()
        {
            if (quantityInput == null || !int.TryParse(quantityInput.text, out var quantity))
            {
                return MinQuantity;
            }

            return Mathf.Max(MinQuantity, quantity);
        }
    }
}
