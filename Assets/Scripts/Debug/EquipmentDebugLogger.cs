using System;
using UnityEngine;
using Project.Items;

namespace Project.DebugTools
{
    /// <summary>
    /// Temporary debug helper that logs every equipment slot's contents to
    /// the Console whenever <see cref="EquipmentManager.EquipmentChanged"/>
    /// fires. Intended to validate multi-slot equip/evict behavior before a
    /// real equipment UI exists; remove once that UI is in place.
    /// </summary>
    public class EquipmentDebugLogger : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipment;

        private void OnEnable()
        {
            equipment.EquipmentChanged += LogAllSlots;
        }

        private void OnDisable()
        {
            equipment.EquipmentChanged -= LogAllSlots;
        }

        private void LogAllSlots()
        {
            foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
            {
                var items = equipment.GetEquippedItems(slot);

                if (items.Count == 0)
                {
                    continue;
                }

                foreach (var item in items)
                {
                    UnityEngine.Debug.Log($"[Equipment] {slot}: {item.ItemName}");
                }
            }
        }
    }
}