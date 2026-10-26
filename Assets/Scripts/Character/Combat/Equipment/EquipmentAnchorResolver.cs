using UnityEngine;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Decides which bone of a Humanoid skeleton an equipped item's model is attached to.
    /// </summary>
    public static class EquipmentAnchorResolver
    {
        /// <summary>
        /// Resolves the attachment bone for an equipped item.
        /// </summary>
        /// <param name="slot">The slot the item occupies. The main hand is <see cref="EquipmentSlot.LeftHand"/> and the off hand is <see cref="EquipmentSlot.RightHand"/>.</param>
        /// <param name="subtype">The weapon category of the item, or <see cref="WeaponSubtype.Unarmed"/> for non-weapons.</param>
        /// <param name="bone">The bone to attach to when the slot has a visual anchor.</param>
        /// <returns>True when items in the slot are shown on the character; false for slots without a visual anchor yet.</returns>
        public static bool TryResolve(EquipmentSlot slot, WeaponSubtype subtype, out HumanBodyBones bone)
        {
            switch (slot)
            {
                case EquipmentSlot.LeftHand:
                    bone = subtype == WeaponSubtype.Bow ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand;
                    return true;
                case EquipmentSlot.RightHand:
                    bone = HumanBodyBones.LeftHand;
                    return true;
                case EquipmentSlot.HeadTop:
                case EquipmentSlot.HeadFace:
                    bone = HumanBodyBones.Head;
                    return true;
                case EquipmentSlot.Ammo:
                    bone = HumanBodyBones.UpperChest;
                    return true;
                default:
                    bone = HumanBodyBones.LastBone;
                    return false;
            }
        }
    }
}
