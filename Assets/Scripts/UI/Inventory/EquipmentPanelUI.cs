using UnityEngine;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Orchestrates the fixed set of <see cref="EquipmentSlotUI"/> views
    /// (one per slot, two for Accessory), keeping them in sync with
    /// <see cref="EquipmentManager.EquipmentChanged"/> and unequipping the
    /// clicked slot's item.
    /// </summary>
    public class EquipmentPanelUI : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private EquipmentSlotUI[] slotViews;

        private void OnEnable()
        {
            foreach (var view in slotViews)
            {
                view.Clicked += HandleSlotClicked;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged -= RefreshAll;
                equipment.EquipmentChanged += RefreshAll;
                RefreshAll();
            }
        }

        private void OnDisable()
        {
            foreach (var view in slotViews)
            {
                view.Clicked -= HandleSlotClicked;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged -= RefreshAll;
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void HandleSlotClicked(EquipmentSlot slot, int indexWithinSlot)
        {
            equipment.Unequip(slot, indexWithinSlot);
        }

        private void RefreshAll()
        {
            foreach (var view in slotViews)
            {
                var items = equipment.GetEquippedItems(view.Slot);
                var item = view.IndexWithinSlot < items.Count ? items[view.IndexWithinSlot] : null;
                view.SetItem(item);
            }
        }
    }
}