using System.Collections.Generic;

namespace Project.Items
{
    /// <summary>
    /// Decides which slots an item occupies when equipped, so the equipment manager only has to apply the result.
    /// </summary>
    public static class EquipmentSlotResolver
    {
        /// <summary>
        /// Resolves the slot or slots an item should occupy. Most items (armor, two-handed weapons) use their authored
        /// slots. A one-handed weapon goes to whichever hand is free, landing in the off hand only when both it and the
        /// current main-hand weapon are flagged <see cref="ItemDefinition.CanBeOffHand"/>. Replacing the main-hand weapon
        /// with one that cannot pair with the current off-hand weapon also evicts that off-hand weapon.
        /// </summary>
        /// <param name="item">The item being equipped.</param>
        /// <param name="mainHandItem">The item currently in the main hand, or null.</param>
        /// <param name="offHandItem">The item currently in the off hand, or null.</param>
        /// <param name="alsoEvictOffHand">Set to true if the current off-hand weapon must be evicted too.</param>
        /// <returns>The slot or slots the item should occupy.</returns>
        public static IReadOnlyList<EquipmentSlot> Resolve(ItemDefinition item, ItemDefinition mainHandItem, ItemDefinition offHandItem, out bool alsoEvictOffHand)
        {
            alsoEvictOffHand = false;
            var authoredSlots = item.RequiredSlots;
            var isOneHandedWeapon = authoredSlots.Count == 1 && authoredSlots[0] == EquipmentSlot.LeftHand;

            if (!isOneHandedWeapon)
            {
                return authoredSlots;
            }

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
    }
}
