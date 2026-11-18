using System.Collections.Generic;

namespace Project.Items
{
    /// <summary>
    /// Converts between a list of <see cref="EquipmentSlot"/> values and the bit mask stored in the save file.
    /// </summary>
    public static class EquipmentSlotMask
    {
        private static readonly int SlotCount = System.Enum.GetValues(typeof(EquipmentSlot)).Length;

        /// <summary>Packs the given slots into a bit mask, one bit per slot index.</summary>
        /// <param name="slots">The slots an equipped item occupies.</param>
        /// <returns>The bit mask.</returns>
        public static int ToMask(IReadOnlyList<EquipmentSlot> slots)
        {
            var mask = 0;

            foreach (var slot in slots)
            {
                mask |= 1 << (int)slot;
            }

            return mask;
        }

        /// <summary>Unpacks a bit mask into the slots it contains, in ascending slot order.</summary>
        /// <param name="mask">The bit mask read from the save file.</param>
        /// <returns>The slots whose bit is set.</returns>
        public static IReadOnlyList<EquipmentSlot> FromMask(int mask)
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
    }
}
