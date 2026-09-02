using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Persistence;

namespace Project.Items
{
    /// <summary>
    /// Tracks currently equipped items as records, each occupying one or
    /// more <see cref="EquipmentSlot"/>s simultaneously (e.g. a two-handed
    /// weapon occupies both LeftHand and RightHand). One-handed weapons
    /// flagged <see cref="ItemDefinition.CanBeOffHand"/> can dual-wield
    /// with another off-hand-capable weapon; equipping any other weapon
    /// into the main hand evicts an incompatible off-hand weapon
    /// automatically. Ammo is tracked as a count rather than a single worn
    /// item: equipping more of the same ammo type stacks onto what's
    /// already equipped, while equipping a different type swaps places
    /// directly with the inventory slot it came from. Exposes the
    /// combined stat bonus across all equipped items, with ammo only
    /// contributing while a Ranged weapon is equipped. Equipping and
    /// unequipping move items to and from the player's inventory.
    /// </summary>
    public class EquipmentManager : MonoBehaviour, ISaveParticipant
    {
        private const int AccessoryCapacity = 2;
        private const int SlotCount = 9;

        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private MonoBehaviour playerStatsSource;
        [SerializeField] private MonoBehaviour playerClassSource;
        [SerializeField] private ItemDatabase itemDatabase;
        [SerializeField] private HealthComponent playerHealth;

        private IPlayerLevelProvider levelProvider;
        private IPlayerClassProvider classProvider;
        private int equippedAmmoCount;

        private void Awake()
        {
            levelProvider = playerStatsSource as IPlayerLevelProvider;
            classProvider = playerClassSource as IPlayerClassProvider;
        }

        private readonly List<EquippedRecord> equippedRecords = new List<EquippedRecord>();

        /// <summary>Raised whenever an item is equipped or unequipped.</summary>
        public event Action EquipmentChanged;

        /// <summary>Raised whenever the equipped ammo count changes (stacked, consumed, swapped, or unequipped).</summary>
        public event Action AmmoCountChanged;

        /// <summary>Gets how many units of ammo are currently equipped.</summary>
        public int EquippedAmmoCount => equippedAmmoCount;

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
        /// Gets the combined bonus for a single stat across all equipped
        /// items. Ammo only contributes while a Ranged weapon is equipped.
        /// </summary>
        /// <param name="stat">The stat to sum bonuses for.</param>
        /// <returns>The total bonus from all equipped items.</returns>
        public int GetBonus(StatType stat)
        {
            var total = default(StatModifiers);
            var rangedWeaponEquipped = IsMainHandWeaponRanged();

            foreach (var record in equippedRecords)
            {
                if (record.OccupiedSlots.Contains(EquipmentSlot.Ammo) && !rangedWeaponEquipped)
                {
                    continue;
                }

                total += record.Item.StatBonuses;
            }

            return ReadStat(total, stat);
        }

        /// <summary>
        /// Equips the item currently in the given inventory slot. For most
        /// equipment this evicts whatever currently occupies its required
        /// slot(s); for one-handed weapons, the target hand is resolved
        /// dynamically (see <see cref="ResolveTargetSlots"/>); for ammo,
        /// equipping the same type stacks onto what's already equipped,
        /// while equipping a different type swaps places with it.
        /// </summary>
        /// <param name="inventorySlotIndex">The inventory slot index holding the item to equip.</param>
        /// <returns>True if the item was equipped.</returns>
        public bool TryEquipFromInventory(int inventorySlotIndex)
        {
            if (playerHealth != null && playerHealth.IsDead)
            {
                return false;
            }

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

            if (IsAmmo(itemToEquip))
            {
                EquipAmmo(itemToEquip, slotContents.Quantity, inventorySlotIndex);
                EquipmentChanged?.Invoke();
                return true;
            }

            var requiredSlots = ResolveTargetSlots(itemToEquip, out var alsoEvictOffHand);

            inventory.Items.RemoveAt(inventorySlotIndex);

            if (alsoEvictOffHand)
            {
                var offHandRecord = equippedRecords.FirstOrDefault(record => record.OccupiedSlots.Contains(EquipmentSlot.RightHand));

                if (offHandRecord != null)
                {
                    EvictRecord(offHandRecord);
                }
            }

            MakeRoomFor(requiredSlots);

            equippedRecords.Add(new EquippedRecord(itemToEquip, requiredSlots));
            EquipmentChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Attempts to consume one unit of equipped ammo. Automatically
        /// unequips the ammo item once the count reaches zero.
        /// </summary>
        /// <returns>True if ammo was available and consumed; false if the Ammo slot is empty or already depleted.</returns>
        public bool TryConsumeAmmo()
        {
            if (equippedAmmoCount <= 0)
            {
                return false;
            }

            equippedAmmoCount--;
            AmmoCountChanged?.Invoke();

            if (equippedAmmoCount == 0)
            {
                var ammoRecord = equippedRecords.FirstOrDefault(record => record.OccupiedSlots.Contains(EquipmentSlot.Ammo));

                if (ammoRecord != null)
                {
                    equippedRecords.Remove(ammoRecord);
                    EquipmentChanged?.Invoke();
                }
            }

            return true;
        }

        private bool MeetsRequirements(ItemDefinition item)
        {
            if (levelProvider != null && levelProvider.BaseLevel < item.RequiredLevel)
            {
                return false;
            }

            if (classProvider != null && item.AllowedClasses.Count > 0 && !item.AllowedClasses.Contains(classProvider.CurrentClass))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Unequips one item instance occupying the given slot, returning
        /// it (and any other slots it occupied) to the inventory. For
        /// Ammo, returns the full remaining count as a single stack
        /// instead of a single unit.
        /// </summary>
        /// <param name="slot">The equipment slot to unequip from.</param>
        /// <param name="indexWithinSlot">Which of the (possibly multiple) items in that slot to remove.</param>
        public void Unequip(EquipmentSlot slot, int indexWithinSlot)
        {
            if (playerHealth != null && playerHealth.IsDead)
            {
                return;
            }

            var recordsInSlot = equippedRecords.Where(record => record.OccupiedSlots.Contains(slot)).ToList();

            if (indexWithinSlot < 0 || indexWithinSlot >= recordsInSlot.Count)
            {
                return;
            }

            var record = recordsInSlot[indexWithinSlot];

            if (slot == EquipmentSlot.Ammo)
            {
                equippedRecords.Remove(record);

                if (equippedAmmoCount > 0)
                {
                    inventory.Items.TryAddItem(record.Item, equippedAmmoCount);
                }

                equippedAmmoCount = 0;
                AmmoCountChanged?.Invoke();
                EquipmentChanged?.Invoke();
                return;
            }

            EvictRecord(record);
            EquipmentChanged?.Invoke();
        }

        /// <summary>
        /// Decides which slot(s) an item should occupy when equipped, and
        /// whether equipping it should also evict whatever's in the off
        /// hand. Most items (armor, two-handed weapons) just use their
        /// authored required slots. A one-handed weapon (authored as Left
        /// Hand) is special-cased: it goes to whichever hand is free,
        /// landing in the off hand (Right Hand) only if both it and
        /// whatever's already in the main hand are flagged
        /// <see cref="ItemDefinition.CanBeOffHand"/>. Replacing the
        /// main-hand weapon with one that can't validly dual-wield with
        /// the current off-hand weapon also evicts that off-hand weapon,
        /// since an off-hand weapon can only stay paired with another
        /// off-hand-capable weapon.
        /// </summary>
        /// <param name="item">The item being equipped.</param>
        /// <param name="alsoEvictOffHand">Set to true if the current off-hand weapon must be evicted too.</param>
        /// <returns>The slot(s) the item should occupy.</returns>
        private IReadOnlyList<EquipmentSlot> ResolveTargetSlots(ItemDefinition item, out bool alsoEvictOffHand)
        {
            alsoEvictOffHand = false;
            var authoredSlots = item.RequiredSlots;
            var isOneHandedWeapon = authoredSlots.Count == 1 && authoredSlots[0] == EquipmentSlot.LeftHand;

            if (!isOneHandedWeapon)
            {
                return authoredSlots;
            }

            var mainHandItem = GetEquippedItems(EquipmentSlot.LeftHand).FirstOrDefault();
            var offHandItem = GetEquippedItems(EquipmentSlot.RightHand).FirstOrDefault();

            if (mainHandItem == null)
            {
                return new[] { EquipmentSlot.LeftHand };
            }

            var canDualWield = offHandItem == null && item.CanBeOffHand && mainHandItem.CanBeOffHand;

            if (canDualWield)
            {
                return new[] { EquipmentSlot.RightHand };
            }

            alsoEvictOffHand = offHandItem != null && !(item.CanBeOffHand && offHandItem.CanBeOffHand);

            return new[] { EquipmentSlot.LeftHand };
        }

        private static bool IsAmmo(ItemDefinition item)
        {
            return item.RequiredSlots.Count == 1 && item.RequiredSlots[0] == EquipmentSlot.Ammo;
        }

        /// <summary>
        /// Equips ammo from the given inventory slot. If the same ammo
        /// type is already equipped, the incoming quantity stacks onto
        /// it and the inventory slot simply empties. If a different type
        /// is equipped, the two swap places directly: the inventory slot
        /// receives exactly what was equipped, and the new stack becomes
        /// equipped.
        /// </summary>
        /// <param name="item">The ammo item being equipped.</param>
        /// <param name="quantity">The quantity in the source inventory slot.</param>
        /// <param name="inventorySlotIndex">The inventory slot the ammo is coming from.</param>
        private void EquipAmmo(ItemDefinition item, int quantity, int inventorySlotIndex)
        {
            var existingAmmoRecord = equippedRecords.FirstOrDefault(record => record.OccupiedSlots.Contains(EquipmentSlot.Ammo));

            if (existingAmmoRecord != null && existingAmmoRecord.Item == item)
            {
                equippedAmmoCount += quantity;
                inventory.Items.RemoveAt(inventorySlotIndex);
                AmmoCountChanged?.Invoke();
                return;
            }

            if (existingAmmoRecord != null)
            {
                equippedRecords.Remove(existingAmmoRecord);
                inventory.Items.SetSlot(inventorySlotIndex, existingAmmoRecord.Item, equippedAmmoCount);
            }
            else
            {
                inventory.Items.RemoveAt(inventorySlotIndex);
            }

            equippedRecords.Add(new EquippedRecord(item, new[] { EquipmentSlot.Ammo }));
            equippedAmmoCount = quantity;
            AmmoCountChanged?.Invoke();
        }

        /// <summary>
        /// Gets whether the currently equipped main-hand weapon is Ranged.
        /// False if unarmed or wielding a Melee weapon.
        /// </summary>
        public bool IsMainHandWeaponRanged()
        {
            var mainHandWeapon = GetEquippedItems(EquipmentSlot.LeftHand).FirstOrDefault();
            return mainHandWeapon != null && mainHandWeapon.WeaponType == WeaponType.Ranged;
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

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.equippedItemIds.Clear();
            data.equippedSlotMasks.Clear();
            data.equippedAmmoCount = equippedAmmoCount;

            if (itemDatabase == null)
            {
                return;
            }

            foreach (var record in equippedRecords)
            {
                data.equippedItemIds.Add(itemDatabase.GetId(record.Item));
                data.equippedSlotMasks.Add(ToSlotMask(record.OccupiedSlots));
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            equippedRecords.Clear();
            equippedAmmoCount = 0;

            if (itemDatabase != null)
            {
                for (var i = 0; i < data.equippedItemIds.Count; i++)
                {
                    var item = itemDatabase.FindById(data.equippedItemIds[i]);

                    if (item != null)
                    {
                        equippedRecords.Add(new EquippedRecord(item, FromSlotMask(data.equippedSlotMasks[i])));
                    }
                }

                equippedAmmoCount = data.equippedAmmoCount;
            }

            EquipmentChanged?.Invoke();
            AmmoCountChanged?.Invoke();
        }

        private static int ToSlotMask(IReadOnlyList<EquipmentSlot> slots)
        {
            var mask = 0;

            foreach (var slot in slots)
            {
                mask |= 1 << (int)slot;
            }

            return mask;
        }

        private static IReadOnlyList<EquipmentSlot> FromSlotMask(int mask)
        {
            var slots = new List<EquipmentSlot>();

            for (var bit = 0; bit < SlotCount; bit++)
            {
                if ((mask & (1 << bit)) != 0)
                {
                    slots.Add((EquipmentSlot)bit);
                }
            }

            return slots;
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
