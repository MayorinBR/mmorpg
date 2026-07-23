using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Items
{
    /// <summary>
    /// Tracks currently equipped items as records, each occupying one or
    /// more <see cref="EquipmentSlot"/>s simultaneously (e.g. a two-handed
    /// weapon occupies both LeftHand and RightHand). Exposes the combined
    /// stat bonus across all equipped items. Equipping and unequipping move
    /// items to and from the player's inventory.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        private const int AccessoryCapacity = 2;

        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private MonoBehaviour playerStatsSource;
        [SerializeField] private MonoBehaviour playerClassSource;

        private IPlayerLevelProvider levelProvider;
        private IPlayerClassProvider classProvider;

        private void Awake()
        {
            levelProvider = playerStatsSource as IPlayerLevelProvider;
            classProvider = playerClassSource as IPlayerClassProvider;
        }

        private readonly List<EquippedRecord> equippedRecords = new List<EquippedRecord>();

        /// <summary>Raised whenever an item is equipped or unequipped.</summary>
        public event Action EquipmentChanged;

        /// <summary>
        /// Gets the items currently occupying the given slot (more than one
        /// only possible for <see cref="EquipmentSlot.Accessory"/>).
        /// </summary>
        /// <param name="slot">The equipment slot to check.</param>
        /// <returns>The items currently occupying that slot.</returns>
        public IReadOnlyList<ItemDefinition> GetEquippedItems(EquipmentSlot slot)
        {
            return equippedRecords
                .Where(record => record.OccupiedSlots.Contains(slot))
                .Select(record => record.Item)
                .ToList();
        }

        /// <summary>
        /// Gets the combined bonus for a single stat across all equipped items.
        /// </summary>
        /// <param name="stat">The stat to sum bonuses for.</param>
        /// <returns>The total bonus from all equipped items.</returns>
        public int GetBonus(StatType stat)
        {
            var total = default(StatModifiers);

            foreach (var record in equippedRecords)
            {
                total += record.Item.StatBonuses;
            }

            return ReadStat(total, stat);
        }

        /// <summary>
        /// Equips the item currently in the given inventory slot. Evicts
        /// whatever currently occupies any of the item's required slots to
        /// make room, returning evicted items to the inventory.
        /// </summary>
        /// <param name="inventorySlotIndex">The inventory slot index holding the item to equip.</param>
        /// <returns>True if the item was equipped.</returns>
        public bool TryEquipFromInventory(int inventorySlotIndex)
        {
            var slotContents = inventory.Items.GetSlot(inventorySlotIndex);

            if (slotContents.IsEmpty || slotContents.Item.ItemType != ItemType.Equipment)
            {
                return false;
            }

            var itemToEquip = slotContents.Item;

            if (!MeetsRequirements(itemToEquip))
            {
                return false;
            }

            var requiredSlots = itemToEquip.RequiredSlots;

            inventory.Items.RemoveAt(inventorySlotIndex);
            MakeRoomFor(requiredSlots);

            equippedRecords.Add(new EquippedRecord(itemToEquip, requiredSlots));
            EquipmentChanged?.Invoke();
            return true;
        }

        private bool MeetsRequirements(ItemDefinition item)
        {
            if (levelProvider != null && levelProvider.BaseLevel < item.RequiredLevel)
            {
                //Debug.Log($"Cannot equip {item.ItemName}: requires level {item.RequiredLevel}, current is {levelProvider.BaseLevel}");
                return false;
            }

            if (classProvider != null && item.AllowedClasses.Count > 0 && !item.AllowedClasses.Contains(classProvider.CurrentClass))
            {
                //Debug.Log($"Cannot equip {item.ItemName}: class {classProvider.CurrentClass} not allowed");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unequips one item instance occupying the given slot, returning
        /// it (and any other slots it occupied) to the inventory.
        /// </summary>
        /// <param name="slot">The equipment slot to unequip from.</param>
        /// <param name="indexWithinSlot">Which of the (possibly multiple) items in that slot to remove.</param>
        public void Unequip(EquipmentSlot slot, int indexWithinSlot)
        {
            var recordsInSlot = equippedRecords.Where(record => record.OccupiedSlots.Contains(slot)).ToList();

            if (indexWithinSlot < 0 || indexWithinSlot >= recordsInSlot.Count)
            {
                return;
            }

            EvictRecord(recordsInSlot[indexWithinSlot]);
            EquipmentChanged?.Invoke();
        }

        private void MakeRoomFor(IReadOnlyList<EquipmentSlot> requiredSlots)
        {
            foreach (var slot in requiredSlots)
            {
                while (CountInSlot(slot) >= GetCapacity(slot))
                {
                    var recordToEvict = equippedRecords.FirstOrDefault(record => record.OccupiedSlots.Contains(slot));

                    if (recordToEvict == null)
                    {
                        break;
                    }

                    EvictRecord(recordToEvict);
                }
            }
        }

        private void EvictRecord(EquippedRecord record)
        {
            equippedRecords.Remove(record);
            inventory.Items.TryAddItem(record.Item, 1);
        }

        private int CountInSlot(EquipmentSlot slot)
        {
            return equippedRecords.Count(record => record.OccupiedSlots.Contains(slot));
        }

        private int GetCapacity(EquipmentSlot slot)
        {
            return slot == EquipmentSlot.Accessory ? AccessoryCapacity : 1;
        }

        private int ReadStat(StatModifiers modifiers, StatType stat)
        {
            switch (stat)
            {
                case StatType.Strength: return modifiers.Strength;
                case StatType.Agility: return modifiers.Agility;
                case StatType.Vitality: return modifiers.Vitality;
                case StatType.Intelligence: return modifiers.Intelligence;
                case StatType.Dexterity: return modifiers.Dexterity;
                case StatType.Luck: return modifiers.Luck;
                default: return 0;
            }
        }

        private class EquippedRecord
        {
            public EquippedRecord(ItemDefinition item, IReadOnlyList<EquipmentSlot> occupiedSlots)
            {
                Item = item;
                OccupiedSlots = occupiedSlots;
            }

            public ItemDefinition Item { get; }
            public IReadOnlyList<EquipmentSlot> OccupiedSlots { get; }
        }
    }
}