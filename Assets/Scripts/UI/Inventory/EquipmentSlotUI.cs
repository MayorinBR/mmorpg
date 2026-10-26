using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Displays a single equipment slot instance's icon. While empty it shows
    /// the placeholder sprite authored on <see cref="iconImage"/> in the
    /// scene or prefab (e.g. a helmet silhouette); while occupied it shows the
    /// equipped item's icon, and the placeholder returns on unequip.
    /// raises <see cref="Clicked"/> with its slot and index when clicked,
    /// and shows the shared item tooltip on hover. For
    /// <see cref="EquipmentSlot.Accessory"/>, two separate instances of
    /// this component (index 0 and 1) represent the two concurrent
    /// accessory slots.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private EquipmentSlot slot;
        [SerializeField] private int indexWithinSlot;
        [SerializeField] private Image iconImage;
        [SerializeField] private Button button;

        private ItemDefinition currentItem;
        private Sprite placeholderSprite;
        private bool placeholderCaptured;

        private Sprite PlaceholderSprite
        {
            get
            {
                if (!placeholderCaptured)
                {
                    placeholderSprite = iconImage.sprite;
                    placeholderCaptured = true;
                }

                return placeholderSprite;
            }
        }

        /// <summary>Raised when this slot is clicked, carrying its slot type and index within that slot.</summary>
        public event Action<EquipmentSlot, int> Clicked;

        /// <summary>Gets the equipment slot type this view represents.</summary>
        public EquipmentSlot Slot => slot;

        /// <summary>Gets which instance within the slot type this view represents (0 or 1 for Accessory, always 0 otherwise).</summary>
        public int IndexWithinSlot => indexWithinSlot;

        private void Awake()
        {
            button.onClick.AddListener(() => Clicked?.Invoke(slot, indexWithinSlot));
        }

        /// <summary>
        /// Updates the displayed icon to reflect the currently equipped item,
        /// falling back to the slot's placeholder sprite when empty.
        /// </summary>
        /// <param name="item">The equipped item, or null if the slot is empty.</param>
        public void SetItem(ItemDefinition item)
        {
            var placeholder = PlaceholderSprite;
            currentItem = item;
            iconImage.sprite = item != null && item.Icon != null ? item.Icon : placeholder;
            iconImage.enabled = iconImage.sprite != null;
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