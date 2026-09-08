using System;
using System.Reflection;
using UnityEngine;
using Project.Items;
using Project.Character.Stats;

namespace Project.Items.Tests
{
    /// <summary>
    /// Builds <see cref="ItemDefinition"/> and <see cref="StatModifiers"/>
    /// test fixtures, and provides the small reflection helpers the tests
    /// in this assembly need to configure private serialized fields and
    /// invoke private Unity lifecycle methods (<c>Awake</c>) outside of
    /// Play Mode. Kept in one place so <c>InventoryTests</c> and
    /// <c>EquipmentManagerTests</c> don't duplicate this setup — none of
    /// this changes production code, since <see cref="ItemDefinition"/>
    /// and the classes under test are deliberately authored (via the
    /// Unity Inspector, or by other systems) rather than constructed
    /// through a public API.
    /// </summary>
    internal static class ItemTestFactory
    {
        /// <summary>
        /// Creates an in-memory <see cref="ItemDefinition"/> asset with the
        /// given field values, bypassing the Inspector. Every parameter
        /// mirrors one of <see cref="ItemDefinition"/>'s private serialized
        /// fields and defaults to a harmless, non-equipment value.
        /// </summary>
        public static ItemDefinition CreateItem(
            string name = "TestItem",
            ItemType itemType = ItemType.Material,
            bool isStackable = true,
            int maxStackSize = 99,
            float weight = 1f,
            EquipmentSlot[] requiredSlots = null,
            StatModifiers statBonuses = default,
            int requiredLevel = 1,
            CharacterClass[] allowedClasses = null,
            bool canBeOffHand = false,
            WeaponType weaponType = WeaponType.Melee)
        {
            var item = ScriptableObject.CreateInstance<ItemDefinition>();
            item.name = name;

            SetField(item, "itemName", name);
            SetField(item, "itemType", itemType);
            SetField(item, "isStackable", isStackable);
            SetField(item, "maxStackSize", maxStackSize);
            SetField(item, "weight", weight);
            SetField(item, "requiredSlots", requiredSlots ?? Array.Empty<EquipmentSlot>());
            SetField(item, "statBonuses", statBonuses);
            SetField(item, "requiredLevel", requiredLevel);
            SetField(item, "allowedClasses", allowedClasses ?? Array.Empty<CharacterClass>());
            SetField(item, "canBeOffHand", canBeOffHand);
            SetField(item, "weaponType", weaponType);

            return item;
        }

        /// <summary>
        /// Builds a <see cref="StatModifiers"/> value with specific stat
        /// bonuses. The struct's own constructor is private (its only
        /// public construction path is the <c>+</c> operator starting from
        /// <c>default</c>), so this invokes it through reflection.
        /// </summary>
        public static StatModifiers CreateStatModifiers(int strength = 0, int agility = 0, int vitality = 0, int intelligence = 0, int dexterity = 0, int luck = 0)
        {
            var constructor = typeof(StatModifiers).GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Instance,
                null,
                new[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) },
                null);

            if (constructor == null)
            {
                throw new InvalidOperationException("StatModifiers no longer has the expected private constructor.");
            }

            return (StatModifiers)constructor.Invoke(new object[] { strength, agility, vitality, intelligence, dexterity, luck });
        }

        /// <summary>
        /// Sets a private instance field by name via reflection. Used to
        /// configure <c>[SerializeField]</c> fields that have no public
        /// setter, since production code never needs one outside the
        /// Inspector.
        /// </summary>
        public static void SetField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"Field '{fieldName}' not found on {target.GetType()}.");

            field.SetValue(target, value);
        }

        /// <summary>
        /// Invokes a private instance method by name via reflection.
        /// EditMode tests never enter Play Mode, so Unity never calls
        /// <c>Awake()</c> on a component automatically — this runs it
        /// directly after the component's fields are configured, exactly
        /// mirroring what Unity would do on scene load.
        /// </summary>
        public static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                ?? throw new InvalidOperationException($"Method '{methodName}' not found on {target.GetType()}.");

            method.Invoke(target, null);
        }
    }
}
