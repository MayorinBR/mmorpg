using System;
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
        [Tooltip("Detailed icon showing the item's real appearance. Used by the shop, tooltip, inventory and equipment UI. The small icon shown on the ground comes from the Dropped Item Icon Set, by category.")]
        [SerializeField] private Sprite icon;

        [Tooltip("Ground icon category. Auto derives it from the item type, slot and weapon subtype; set it manually for cases like food.")]
        [SerializeField] private DropIconCategory dropIconCategory = DropIconCategory.Auto;
        [SerializeField] private bool isStackable = true;
        [SerializeField] private int maxStackSize = 99;
        [SerializeField] private ItemType itemType = ItemType.Material;
        [SerializeField] private EquipmentSlot[] requiredSlots;
        [SerializeField] private StatModifiers statBonuses;
        [SerializeField] private int requiredLevel = 1;
        [SerializeField] private CharacterClass[] allowedClasses;
        [SerializeField] private float weight = 1f;
        [SerializeField] private bool canBeOffHand;
        [SerializeField] private WeaponType weaponType = WeaponType.Melee;
        [SerializeField] private WeaponSubtype weaponSubtype = WeaponSubtype.Unarmed;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private int buyPrice;
        [SerializeField] private int sellPrice;
        [SerializeField] private ConsumableEffectType consumableEffectType = ConsumableEffectType.Instant;
        [SerializeField] private int healthRestore;
        [SerializeField] private int manaRestore;
        [SerializeField] private float effectDurationSeconds = 1f;
        [SerializeField] private float tickIntervalSeconds = 1f;

        [Tooltip("If true, using this consumable clears Poison (see Project.Combat.StatusEffectController.ApplyPoison) — e.g. an Antidote. Only meaningful when ItemType is Consumable.")]
        [SerializeField] private bool curesPoison;

        [Tooltip("Model shown on the character while this item is equipped. Its root transform is the offset from the attachment bone, so author the pivot at the grip (weapons) or the head origin (headgear). Only meaningful when ItemType is Equipment.")]
        [SerializeField] private GameObject equippedModel;

        [Tooltip("Optional. Used instead of Equipped Model on a female character, for items that need a different scale or offset.")]
        [SerializeField] private GameObject equippedModelFemale;

        [Tooltip("If true, the character's hair is hidden while this item is equipped (e.g. a closed helmet).")]
        [SerializeField] private bool hidesHair;

        /// <summary>Gets the display name of the item.</summary>
        public string ItemName => itemName;

        /// <summary>Gets the item's description text.</summary>
        public string Description => description;

        /// <summary>Gets the detailed icon shown in the shop, tooltip and inventory UI.</summary>
        public Sprite Icon => icon;

        /// <summary>Gets the category that selects this item's small ground icon, resolved automatically unless overridden on the asset.</summary>
        public DropIconCategory DropCategory => dropIconCategory != DropIconCategory.Auto ? dropIconCategory : ResolveDropCategory();

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

        /// <summary>Gets the weight of a single unit of this item, used by the carry-weight inventory system.</summary>
        public float Weight => weight;

        /// <summary>Gets whether this one-handed weapon can be equipped in the off hand (Right Hand) alongside a main-hand weapon.</summary>
        public bool CanBeOffHand => canBeOffHand;

        /// <summary>Gets whether this weapon is melee or ranged. Only meaningful for hand-slot equipment.</summary>
        public WeaponType WeaponType => weaponType;

        /// <summary>Gets the specific weapon category (dagger, one-hand sword, bow, etc.), used by weapon-mastery passive skills. Only meaningful for hand-slot equipment; <see cref="Project.Items.WeaponSubtype.Unarmed"/> for anything else.</summary>
        public WeaponSubtype WeaponSubtype => weaponSubtype;

        /// <summary>Gets the basic-attack range this weapon grants while equipped. Only meaningful for hand-slot weapons.</summary>
        public float AttackRange => attackRange;

        /// <summary>Gets the Zeny cost to buy this item from a shop.</summary>
        public int BuyPrice => buyPrice;

        /// <summary>Gets the Zeny a shop pays the player when selling this item.</summary>
        public int SellPrice => sellPrice;

        /// <summary>Gets whether this consumable restores instantly or gradually over time. Only meaningful when <see cref="ItemType"/> is Consumable.</summary>
        public ConsumableEffectType ConsumableEffectType => consumableEffectType;

        /// <summary>Gets the health restored on use — the full amount for an instant effect, or the amount per tick for an over-time effect. Only meaningful when <see cref="ItemType"/> is Consumable.</summary>
        public int HealthRestore => healthRestore;

        /// <summary>Gets the mana restored on use — the full amount for an instant effect, or the amount per tick for an over-time effect. Only meaningful when <see cref="ItemType"/> is Consumable.</summary>
        public int ManaRestore => manaRestore;

        /// <summary>Gets the total duration, in seconds, an over-time effect lasts. Only meaningful when <see cref="ConsumableEffectType"/> is OverTime.</summary>
        public float EffectDurationSeconds => effectDurationSeconds;

        /// <summary>Gets the interval, in seconds, between each tick of an over-time effect. Only meaningful when <see cref="ConsumableEffectType"/> is OverTime.</summary>
        public float TickIntervalSeconds => tickIntervalSeconds;

        /// <summary>
        /// Gets whether using this consumable clears every active status
        /// effect (see <see cref="Project.Combat.StatusEffectController.ClearAllDebuffs"/>)
        /// — e.g. an Antidote. Read by
        /// <see cref="Project.Character.Combat.PlayerConsumableUser"/>.
        /// </summary>
        public bool CuresPoison => curesPoison;

        /// <summary>Gets whether the character's hair is hidden while this item is equipped.</summary>
        public bool HidesHair => hidesHair;

        /// <summary>
        /// Gets the model shown on a character of the given gender while this
        /// item is equipped.
        /// </summary>
        /// <param name="gender">The gender of the character wearing the item.</param>
        /// <returns>The gender-specific model when one is authored for a female character, otherwise the shared model; null when the item has no model.</returns>
        public GameObject GetEquippedModel(CharacterGender gender)
        {
            return gender == CharacterGender.Female && equippedModelFemale != null ? equippedModelFemale : equippedModel;
        }

        private DropIconCategory ResolveDropCategory()
        {
            switch (itemType)
            {
                case ItemType.Consumable:
                    return DropIconCategory.Potion;
                case ItemType.Material:
                    return DropIconCategory.Material;
                case ItemType.KeyItem:
                    return DropIconCategory.KeyItem;
                case ItemType.Equipment:
                    return ResolveEquipmentDropCategory();
                default:
                    return DropIconCategory.Generic;
            }
        }

        private DropIconCategory ResolveEquipmentDropCategory()
        {
            switch (weaponSubtype)
            {
                case WeaponSubtype.Dagger: return DropIconCategory.Dagger;
                case WeaponSubtype.OneHandSword: return DropIconCategory.OneHandSword;
                case WeaponSubtype.TwoHandSword: return DropIconCategory.TwoHandSword;
                case WeaponSubtype.OneHandAxe:
                case WeaponSubtype.TwoHandAxe: return DropIconCategory.Axe;
                case WeaponSubtype.Mace: return DropIconCategory.Mace;
                case WeaponSubtype.Spear: return DropIconCategory.Spear;
                case WeaponSubtype.Bow: return DropIconCategory.Bow;
                case WeaponSubtype.Book: return DropIconCategory.Book;
                case WeaponSubtype.Instrument: return DropIconCategory.Instrument;
                case WeaponSubtype.Whip: return DropIconCategory.Whip;
                case WeaponSubtype.Rod: return DropIconCategory.Rod;
                case WeaponSubtype.Knuckle: return DropIconCategory.Knuckle;
                case WeaponSubtype.Katar: return DropIconCategory.Katar;
                case WeaponSubtype.Gun: return DropIconCategory.Gun;
                case WeaponSubtype.HuumaShuriken: return DropIconCategory.Shuriken;
                case WeaponSubtype.Shield: return DropIconCategory.Shield;
            }

            if (requiredSlots == null || requiredSlots.Length == 0)
            {
                return DropIconCategory.Generic;
            }

            switch (requiredSlots[0])
            {
                case EquipmentSlot.HeadTop:
                case EquipmentSlot.HeadFace: return DropIconCategory.Headgear;
                case EquipmentSlot.Body: return DropIconCategory.Armor;
                case EquipmentSlot.Pants: return DropIconCategory.Legs;
                case EquipmentSlot.Boots: return DropIconCategory.Boots;
                case EquipmentSlot.Gloves: return DropIconCategory.Gloves;
                case EquipmentSlot.Accessory: return DropIconCategory.Accessory;
                case EquipmentSlot.Ammo: return DropIconCategory.Ammo;
                default: return DropIconCategory.Generic;
            }
        }

        private void OnValidate()
        {
            if (itemType == ItemType.Equipment)
            {
                ValidateEquipment();
            }
            else if (itemType == ItemType.Consumable)
            {
                ValidateConsumable();
            }
        }

        private void ValidateEquipment()
        {
            var isAmmo = requiredSlots != null && Array.Exists(requiredSlots, slot => slot == EquipmentSlot.Ammo);

            if (isStackable && !isAmmo)
            {
                Debug.LogWarning($"{name}: Equipment item is marked Is Stackable, which equipment should never be.", this);
            }

            if (requiredSlots == null || requiredSlots.Length == 0)
            {
                Debug.LogWarning($"{name}: Equipment item has no Required Slots assigned, so it can never actually be equipped.", this);
            }
        }

        private void ValidateConsumable()
        {
            if (healthRestore <= 0 && manaRestore <= 0 && !curesPoison)
            {
                Debug.LogWarning($"{name}: Consumable item restores no health or mana.", this);
            }

            if (consumableEffectType == ConsumableEffectType.OverTime && (effectDurationSeconds <= 0f || tickIntervalSeconds <= 0f))
            {
                Debug.LogWarning($"{name}: Over-time consumable needs a positive Effect Duration and Tick Interval.", this);
            }
        }
    }
}