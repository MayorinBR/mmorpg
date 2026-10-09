using NUnit.Framework;
using UnityEngine;
using Project.Character.Combat;
using Project.Items;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="EquipmentAnchorResolver"/>.
    /// </summary>
    public class EquipmentAnchorResolverTests
    {
        [TestCase(EquipmentSlot.LeftHand, WeaponSubtype.OneHandSword, HumanBodyBones.RightHand)]
        [TestCase(EquipmentSlot.LeftHand, WeaponSubtype.Bow, HumanBodyBones.LeftHand)]
        [TestCase(EquipmentSlot.RightHand, WeaponSubtype.Shield, HumanBodyBones.LeftHand)]
        [TestCase(EquipmentSlot.RightHand, WeaponSubtype.Dagger, HumanBodyBones.LeftHand)]
        [TestCase(EquipmentSlot.HeadTop, WeaponSubtype.Unarmed, HumanBodyBones.Head)]
        [TestCase(EquipmentSlot.HeadFace, WeaponSubtype.Unarmed, HumanBodyBones.Head)]
        [TestCase(EquipmentSlot.Ammo, WeaponSubtype.Unarmed, HumanBodyBones.UpperChest)]
        public void TryResolve_SlotWithAnchor_ReturnsExpectedBone(EquipmentSlot slot, WeaponSubtype subtype, HumanBodyBones expected)
        {
            var resolved = EquipmentAnchorResolver.TryResolve(slot, subtype, out var bone);

            Assert.IsTrue(resolved);
            Assert.AreEqual(expected, bone);
        }

        [TestCase(EquipmentSlot.Body)]
        [TestCase(EquipmentSlot.Pants)]
        [TestCase(EquipmentSlot.Boots)]
        [TestCase(EquipmentSlot.Accessory)]
        public void TryResolve_SlotWithoutAnchor_ReturnsFalse(EquipmentSlot slot)
        {
            Assert.IsFalse(EquipmentAnchorResolver.TryResolve(slot, WeaponSubtype.Unarmed, out _));
        }
    }
}
