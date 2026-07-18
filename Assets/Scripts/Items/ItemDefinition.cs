using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Items
{
    /// <summary>
    /// Defines a single item type. Instances are authored as assets and
    /// referenced by inventory slots and loot tables, so all systems read
    /// identical item data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "Project/Items/Item")]
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string itemName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private ItemType itemType = ItemType.Material;
        [SerializeField] private EquipmentSlot[] requiredSlots;
        [SerializeField] private StatModifiers statBonuses;
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private CharacterClass[] allowedClasses;

        /// <summary>Gets the display name of the item.</summary>
        public string ItemName => itemName;

        /// <summary>Gets the item's description text.</summary>
        public string Description => description;

        /// <summary>Gets the icon shown in inventory UI.</summary>
        public Sprite Icon => icon;

        /// <summary>Gets a value indicating whether multiple units of this item can share a single slot.</summary>
        public bool IsStackable => isStackable;

        /// <summary>Gets the maximum quantity allowed in a single stack.</summary>
        public int MaxStackSize => maxStackSize;

        /// <summary>Gets the broad category of this item.</summary>
        public ItemType ItemType => itemType;

        /// <summary>Gets the equipment slot this item occupies. Only meaningful when <see cref="ItemType"/> is Equipment.</summary>
        public IReadOnlyList<EquipmentSlot> RequiredSlots => requiredSlots;

        /// <summary>Gets the stat bonuses this item grants while equipped. Only meaningful when <see cref="ItemType"/> is Equipment.</summary>
        public StatModifiers StatBonuses => statBonuses;

        /// <summary>Gets the minimum character level required to equip this item. Only meaningful when <see cref="ItemType"/> is Equipment.</summary>
        public int RequiredLevel => requiredLevel;

        /// <summary>Gets the classes allowed to equip this item. An empty array means any class can use it. Only meaningful when <see cref="ItemType"/> is Equipment.</summary>
        public IReadOnlyList<CharacterClass> AllowedClasses => allowedClasses;
    }
}