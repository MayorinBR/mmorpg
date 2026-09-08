using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Project.Items;
using Project.Character.Stats;
using Project.Combat;
using Project.Persistence;

namespace Project.Items.Tests
{
    /// <summary>
    /// Covers <see cref="EquipmentManager"/>'s multi-slot and eviction
    /// logic — the case FUTURE_IMPROVEMENTS.md called out alongside
    /// <c>Inventory.TryAddItem</c> as a good candidate for automated
    /// coverage, since it has caused subtle bugs before. Unlike
    /// <see cref="InventoryTests"/>, <see cref="EquipmentManager"/> and
    /// <see cref="PlayerInventory"/> are <see cref="MonoBehaviour"/>s, and
    /// EditMode tests never enter Play Mode, so Unity never calls their
    /// private <c>Awake()</c> automatically — <see cref="ItemTestFactory.InvokePrivateMethod"/>
    /// runs it directly after each component's fields are configured,
    /// exactly mirroring what Unity does on scene load.
    /// </summary>
    public class EquipmentManagerTests
    {
        private readonly List<UnityEngine.Object> createdObjects = new List<UnityEngine.Object>();

        private PlayerInventory playerInventory;
        private EquipmentManager equipmentManager;

        [SetUp]
        public void SetUp()
        {
            var inventoryObject = new GameObject("PlayerInventory");
            playerInventory = inventoryObject.AddComponent<PlayerInventory>();
            ItemTestFactory.InvokePrivateMethod(playerInventory, "Awake");

            var equipmentObject = new GameObject("EquipmentManager");
            equipmentManager = equipmentObject.AddComponent<EquipmentManager>();
            ItemTestFactory.SetField(equipmentManager, "inventory", playerInventory);
            ItemTestFactory.InvokePrivateMethod(equipmentManager, "Awake");

            createdObjects.Add(inventoryObject);
            createdObjects.Add(equipmentObject);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var createdObject in createdObjects)
            {
                if (createdObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(createdObject);
                }
            }

            createdObjects.Clear();
        }

        [Test]
        public void TryEquipFromInventory_ArmorItem_MovesFromInventoryToEquippedSlot()
        {
            var armor = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, weight: 0f);
            playerInventory.Items.TryAddItem(armor, 1);

            var result = equipmentManager.TryEquipFromInventory(0);

            Assert.IsTrue(result);
            Assert.AreEqual(1, equipmentManager.GetEquippedItems(EquipmentSlot.Body).Count);
            Assert.AreEqual(armor, equipmentManager.GetEquippedItems(EquipmentSlot.Body)[0]);
            Assert.IsTrue(playerInventory.Items.GetSlot(0).IsEmpty);
        }

        [Test]
        public void TryEquipFromInventory_ReplacingItemInSameSlot_ReturnsPreviousItemToInventory()
        {
            var firstArmor = ItemTestFactory.CreateItem(name: "FirstArmor", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, weight: 0f);
            var secondArmor = ItemTestFactory.CreateItem(name: "SecondArmor", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, weight: 0f);
            playerInventory.Items.TryAddItem(firstArmor, 1);
            playerInventory.Items.TryAddItem(secondArmor, 1);
            equipmentManager.TryEquipFromInventory(0);

            var result = equipmentManager.TryEquipFromInventory(1);

            Assert.IsTrue(result);
            Assert.AreEqual(secondArmor, equipmentManager.GetEquippedItems(EquipmentSlot.Body)[0]);
            Assert.IsTrue(InventoryContains(firstArmor));
        }

        [Test]
        public void TryEquipFromInventory_TwoHandedWeapon_OccupiesBothHandSlots()
        {
            var twoHandedSword = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand, EquipmentSlot.RightHand }, weight: 0f);
            playerInventory.Items.TryAddItem(twoHandedSword, 1);

            equipmentManager.TryEquipFromInventory(0);

            Assert.AreEqual(twoHandedSword, equipmentManager.GetEquippedItems(EquipmentSlot.LeftHand)[0]);
            Assert.AreEqual(twoHandedSword, equipmentManager.GetEquippedItems(EquipmentSlot.RightHand)[0]);
        }

        [Test]
        public void TryEquipFromInventory_OneHandedWeapon_GoesToLeftHandWhenBothHandsAreEmpty()
        {
            var dagger = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: true, weight: 0f);
            playerInventory.Items.TryAddItem(dagger, 1);

            equipmentManager.TryEquipFromInventory(0);

            Assert.AreEqual(dagger, equipmentManager.GetEquippedItems(EquipmentSlot.LeftHand)[0]);
            Assert.AreEqual(0, equipmentManager.GetEquippedItems(EquipmentSlot.RightHand).Count);
        }

        [Test]
        public void TryEquipFromInventory_SecondDualWieldCapableWeapon_GoesToOffHand()
        {
            var mainDagger = ItemTestFactory.CreateItem(name: "MainDagger", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: true, weight: 0f);
            var offDagger = ItemTestFactory.CreateItem(name: "OffDagger", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: true, weight: 0f);
            playerInventory.Items.TryAddItem(mainDagger, 1);
            playerInventory.Items.TryAddItem(offDagger, 1);
            equipmentManager.TryEquipFromInventory(0);

            equipmentManager.TryEquipFromInventory(1);

            Assert.AreEqual(mainDagger, equipmentManager.GetEquippedItems(EquipmentSlot.LeftHand)[0]);
            Assert.AreEqual(offDagger, equipmentManager.GetEquippedItems(EquipmentSlot.RightHand)[0]);
        }

        [Test]
        public void TryEquipFromInventory_MainHandWeaponIncompatibleWithCurrentOffHand_AlsoEvictsOffHandWeapon()
        {
            var mainDagger = ItemTestFactory.CreateItem(name: "MainDagger", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: true, weight: 0f);
            var offDagger = ItemTestFactory.CreateItem(name: "OffDagger", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: true, weight: 0f);
            var heavySword = ItemTestFactory.CreateItem(name: "HeavySword", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, canBeOffHand: false, weight: 0f);
            playerInventory.Items.TryAddItem(mainDagger, 1);
            playerInventory.Items.TryAddItem(offDagger, 1);
            playerInventory.Items.TryAddItem(heavySword, 1);
            equipmentManager.TryEquipFromInventory(0);
            equipmentManager.TryEquipFromInventory(1);

            var result = equipmentManager.TryEquipFromInventory(2);

            Assert.IsTrue(result);
            Assert.AreEqual(heavySword, equipmentManager.GetEquippedItems(EquipmentSlot.LeftHand)[0]);
            Assert.AreEqual(0, equipmentManager.GetEquippedItems(EquipmentSlot.RightHand).Count);
            Assert.IsTrue(InventoryContains(mainDagger));
            Assert.IsTrue(InventoryContains(offDagger));
        }

        [Test]
        public void TryEquipFromInventory_BelowRequiredLevel_FailsAndPublishesFeedback()
        {
            var levelProviderObject = new GameObject("LevelProvider");
            var levelProvider = levelProviderObject.AddComponent<FakeLevelProvider>();
            levelProvider.BaseLevel = 5;
            createdObjects.Add(levelProviderObject);
            ItemTestFactory.SetField(equipmentManager, "playerStatsSource", levelProvider);
            ItemTestFactory.InvokePrivateMethod(equipmentManager, "Awake");

            var highLevelArmor = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, requiredLevel: 10, weight: 0f);
            playerInventory.Items.TryAddItem(highLevelArmor, 1);

            string publishedMessage = null;
            void Handler(string message) => publishedMessage = message;
            PlayerFeedbackChannel.MessagePublished += Handler;

            try
            {
                var result = equipmentManager.TryEquipFromInventory(0);

                Assert.IsFalse(result);
                Assert.AreEqual(0, equipmentManager.GetEquippedItems(EquipmentSlot.Body).Count);
                Assert.IsNotNull(publishedMessage);
                StringAssert.Contains("Level 10", publishedMessage);
            }
            finally
            {
                PlayerFeedbackChannel.MessagePublished -= Handler;
            }
        }

        [Test]
        public void TryEquipFromInventory_ClassNotAllowed_FailsAndPublishesFeedback()
        {
            var classProviderObject = new GameObject("ClassProvider");
            var classProvider = classProviderObject.AddComponent<FakeClassProvider>();
            classProvider.CurrentClass = CharacterClass.Swordman;
            createdObjects.Add(classProviderObject);
            ItemTestFactory.SetField(equipmentManager, "playerClassSource", classProvider);
            ItemTestFactory.InvokePrivateMethod(equipmentManager, "Awake");

            var mageOnlyRobe = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, allowedClasses: new[] { CharacterClass.Mage }, weight: 0f);
            playerInventory.Items.TryAddItem(mageOnlyRobe, 1);

            string publishedMessage = null;
            void Handler(string message) => publishedMessage = message;
            PlayerFeedbackChannel.MessagePublished += Handler;

            try
            {
                var result = equipmentManager.TryEquipFromInventory(0);

                Assert.IsFalse(result);
                Assert.IsNotNull(publishedMessage);
                StringAssert.Contains("class cannot equip", publishedMessage);
            }
            finally
            {
                PlayerFeedbackChannel.MessagePublished -= Handler;
            }
        }

        [Test]
        public void TryEquipFromInventory_SameAmmoTypeAlreadyEquipped_StacksOntoExisting()
        {
            var arrows = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: true, maxStackSize: 99, requiredSlots: new[] { EquipmentSlot.Ammo }, weight: 0f);
            playerInventory.Items.TryAddItem(arrows, 20);
            equipmentManager.TryEquipFromInventory(0);
            playerInventory.Items.TryAddItem(arrows, 10);

            equipmentManager.TryEquipFromInventory(0);

            Assert.AreEqual(30, equipmentManager.EquippedAmmoCount);
            Assert.IsTrue(playerInventory.Items.GetSlot(0).IsEmpty);
        }

        [Test]
        public void TryEquipFromInventory_DifferentAmmoTypeAlreadyEquipped_SwapsWithInventorySlot()
        {
            var arrows = ItemTestFactory.CreateItem(name: "Arrows", itemType: ItemType.Equipment, isStackable: true, maxStackSize: 99, requiredSlots: new[] { EquipmentSlot.Ammo }, weight: 0f);
            var bolts = ItemTestFactory.CreateItem(name: "Bolts", itemType: ItemType.Equipment, isStackable: true, maxStackSize: 99, requiredSlots: new[] { EquipmentSlot.Ammo }, weight: 0f);
            playerInventory.Items.TryAddItem(arrows, 20);
            equipmentManager.TryEquipFromInventory(0);
            playerInventory.Items.TryAddItem(bolts, 5);

            var result = equipmentManager.TryEquipFromInventory(0);

            Assert.IsTrue(result);
            Assert.AreEqual(5, equipmentManager.EquippedAmmoCount);
            Assert.AreEqual(bolts, equipmentManager.GetEquippedItems(EquipmentSlot.Ammo)[0]);
            Assert.AreEqual(arrows, playerInventory.Items.GetSlot(0).Item);
            Assert.AreEqual(20, playerInventory.Items.GetSlot(0).Quantity);
        }

        [Test]
        public void Unequip_ReturnsItemToInventoryAndRaisesEquipmentChanged()
        {
            var armor = ItemTestFactory.CreateItem(itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, weight: 0f);
            playerInventory.Items.TryAddItem(armor, 1);
            equipmentManager.TryEquipFromInventory(0);
            var raised = false;
            equipmentManager.EquipmentChanged += () => raised = true;

            equipmentManager.Unequip(EquipmentSlot.Body, 0);

            Assert.AreEqual(0, equipmentManager.GetEquippedItems(EquipmentSlot.Body).Count);
            Assert.IsTrue(InventoryContains(armor));
            Assert.IsTrue(raised);
        }

        [Test]
        public void GetBonus_SumsStatBonusesAcrossAllEquippedItems()
        {
            var strengthItem = ItemTestFactory.CreateItem(name: "StrengthItem", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, statBonuses: ItemTestFactory.CreateStatModifiers(strength: 5), weight: 0f);
            var mixedItem = ItemTestFactory.CreateItem(name: "MixedItem", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Pants }, statBonuses: ItemTestFactory.CreateStatModifiers(strength: 2, vitality: 3), weight: 0f);
            playerInventory.Items.TryAddItem(strengthItem, 1);
            playerInventory.Items.TryAddItem(mixedItem, 1);
            equipmentManager.TryEquipFromInventory(0);
            equipmentManager.TryEquipFromInventory(1);

            Assert.AreEqual(7, equipmentManager.GetBonus(StatType.Strength));
            Assert.AreEqual(3, equipmentManager.GetBonus(StatType.Vitality));
        }

        [Test]
        public void GetBonus_AmmoStatBonus_OnlyCountsWhileRangedWeaponEquipped()
        {
            var arrows = ItemTestFactory.CreateItem(name: "Arrows", itemType: ItemType.Equipment, isStackable: true, maxStackSize: 99, requiredSlots: new[] { EquipmentSlot.Ammo }, statBonuses: ItemTestFactory.CreateStatModifiers(dexterity: 4), weight: 0f);
            var bow = ItemTestFactory.CreateItem(name: "Bow", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.LeftHand }, weaponType: WeaponType.Ranged, weight: 0f);
            playerInventory.Items.TryAddItem(arrows, 10);
            equipmentManager.TryEquipFromInventory(0);

            Assert.AreEqual(0, equipmentManager.GetBonus(StatType.Dexterity));

            playerInventory.Items.TryAddItem(bow, 1);
            equipmentManager.TryEquipFromInventory(0);

            Assert.AreEqual(4, equipmentManager.GetBonus(StatType.Dexterity));
        }

        [Test]
        public void CaptureState_ThenRestoreState_RoundTripsEquippedItemsAndAmmoCount()
        {
            var itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
            var armor = ItemTestFactory.CreateItem(name: "Armor", itemType: ItemType.Equipment, isStackable: false, requiredSlots: new[] { EquipmentSlot.Body }, weight: 0f);
            var arrows = ItemTestFactory.CreateItem(name: "Arrows", itemType: ItemType.Equipment, isStackable: true, maxStackSize: 99, requiredSlots: new[] { EquipmentSlot.Ammo }, weight: 0f);
            ItemTestFactory.SetField(itemDatabase, "allItems", new[] { armor, arrows });
            ItemTestFactory.SetField(equipmentManager, "itemDatabase", itemDatabase);

            playerInventory.Items.TryAddItem(armor, 1);
            playerInventory.Items.TryAddItem(arrows, 30);
            equipmentManager.TryEquipFromInventory(0);
            equipmentManager.TryEquipFromInventory(1);

            var savedData = new PlayerSaveData();
            equipmentManager.CaptureState(savedData);

            var freshInventoryObject = new GameObject("FreshPlayerInventory");
            var freshInventory = freshInventoryObject.AddComponent<PlayerInventory>();
            ItemTestFactory.InvokePrivateMethod(freshInventory, "Awake");

            var freshEquipmentObject = new GameObject("FreshEquipmentManager");
            var freshEquipmentManager = freshEquipmentObject.AddComponent<EquipmentManager>();
            ItemTestFactory.SetField(freshEquipmentManager, "inventory", freshInventory);
            ItemTestFactory.SetField(freshEquipmentManager, "itemDatabase", itemDatabase);
            ItemTestFactory.InvokePrivateMethod(freshEquipmentManager, "Awake");

            createdObjects.Add(freshInventoryObject);
            createdObjects.Add(freshEquipmentObject);

            freshEquipmentManager.RestoreState(savedData);

            Assert.AreEqual(armor, freshEquipmentManager.GetEquippedItems(EquipmentSlot.Body)[0]);
            Assert.AreEqual(arrows, freshEquipmentManager.GetEquippedItems(EquipmentSlot.Ammo)[0]);
            Assert.AreEqual(30, freshEquipmentManager.EquippedAmmoCount);
        }

        private bool InventoryContains(ItemDefinition item)
        {
            for (var i = 0; i < playerInventory.Items.SlotCount; i++)
            {
                var slot = playerInventory.Items.GetSlot(i);

                if (!slot.IsEmpty && slot.Item == item)
                {
                    return true;
                }
            }

            return false;
        }

        private class FakeLevelProvider : MonoBehaviour, IPlayerLevelProvider
        {
            public int BaseLevel { get; set; }
        }

        private class FakeClassProvider : MonoBehaviour, IPlayerClassProvider
        {
            public CharacterClass CurrentClass { get; set; }
        }
    }
}
