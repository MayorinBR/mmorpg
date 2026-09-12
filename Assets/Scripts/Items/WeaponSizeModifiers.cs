using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Items
{
    /// <summary>
    /// Real Ragnarok Online Classic's weapon-vs-monster-size damage table
    /// (source: iRO Wiki Classic — Size, consulted September 2026): most
    /// weapon types deal reduced damage to at least one monster size.
    /// Weapon types not listed here (<see cref="WeaponSubtype.Unarmed"/>,
    /// <see cref="WeaponSubtype.Rod"/>, <see cref="WeaponSubtype.Gun"/>,
    /// <see cref="WeaponSubtype.HuumaShuriken"/>, <see cref="WeaponSubtype.Shield"/>)
    /// deal full damage to every size, matching the wiki's own table.
    /// </summary>
    public static class WeaponSizeModifiers
    {
        private static readonly Dictionary<WeaponSubtype, SizeModifierRow> Table = new Dictionary<WeaponSubtype, SizeModifierRow>
        {
            { WeaponSubtype.Dagger, new SizeModifierRow(small: 1f, medium: 0.75f, large: 0.5f) },
            { WeaponSubtype.OneHandSword, new SizeModifierRow(small: 0.75f, medium: 1f, large: 0.75f) },
            { WeaponSubtype.TwoHandSword, new SizeModifierRow(small: 0.75f, medium: 0.75f, large: 1f) },
            { WeaponSubtype.OneHandAxe, new SizeModifierRow(small: 0.5f, medium: 0.75f, large: 1f) },
            { WeaponSubtype.TwoHandAxe, new SizeModifierRow(small: 0.5f, medium: 0.75f, large: 1f) },
            { WeaponSubtype.Mace, new SizeModifierRow(small: 0.75f, medium: 1f, large: 1f) },
            { WeaponSubtype.Spear, new SizeModifierRow(small: 0.75f, medium: 0.75f, large: 1f) },
            { WeaponSubtype.Bow, new SizeModifierRow(small: 1f, medium: 1f, large: 0.75f) },
            { WeaponSubtype.Book, new SizeModifierRow(small: 1f, medium: 1f, large: 0.5f) },
            { WeaponSubtype.Instrument, new SizeModifierRow(small: 0.75f, medium: 1f, large: 0.75f) },
            { WeaponSubtype.Whip, new SizeModifierRow(small: 0.75f, medium: 1f, large: 0.5f) },
            { WeaponSubtype.Knuckle, new SizeModifierRow(small: 1f, medium: 0.75f, large: 0.5f) },
            { WeaponSubtype.Katar, new SizeModifierRow(small: 0.75f, medium: 1f, large: 0.75f) },
        };

        /// <summary>
        /// Applies the weapon-vs-size damage modifier to a physical hit,
        /// leaving magical damage untouched — matching Ragnarok Online,
        /// where the size table only affects weapon-delivered physical
        /// damage. The single choke point every physical damage source
        /// (basic attacks, physical skills) should call before
        /// <see cref="IDamageable.TakeDamage"/>, instead of each
        /// caller repeating its own "only if Physical" check.
        /// </summary>
        /// <param name="damage">The damage amount before the size modifier.</param>
        /// <param name="category">Whether this damage is physical or magical.</param>
        /// <param name="weapon">The attacker's equipped main-hand weapon subtype.</param>
        /// <param name="targetSize">The target's size class.</param>
        /// <returns>The adjusted damage amount.</returns>
        public static int Apply(int damage, DamageCategory category, WeaponSubtype weapon, MonsterSize targetSize)
        {
            return category == DamageCategory.Physical
                ? Mathf.RoundToInt(damage * GetMultiplier(weapon, targetSize))
                : damage;
        }

        /// <summary>
        /// Gets the raw damage multiplier for a weapon type against a
        /// monster size, with no size penalty (1) for any weapon type not
        /// in the table.
        /// </summary>
        /// <param name="weapon">The weapon subtype.</param>
        /// <param name="size">The target's size class.</param>
        /// <returns>The damage multiplier, e.g. 0.5 for a dagger vs. a Large monster.</returns>
        public static float GetMultiplier(WeaponSubtype weapon, MonsterSize size)
        {
            if (!Table.TryGetValue(weapon, out var row))
            {
                return 1f;
            }

            switch (size)
            {
                case MonsterSize.Small: return row.Small;
                case MonsterSize.Large: return row.Large;
                default: return row.Medium;
            }
        }

        private readonly struct SizeModifierRow
        {
            public SizeModifierRow(float small, float medium, float large)
            {
                Small = small;
                Medium = medium;
                Large = large;
            }

            public float Small { get; }
            public float Medium { get; }
            public float Large { get; }
        }
    }
}
