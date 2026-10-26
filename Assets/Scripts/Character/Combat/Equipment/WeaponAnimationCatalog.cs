using System.Collections.Generic;
using UnityEngine;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Maps each <see cref="WeaponSubtype"/> to the basic-attack clip the player plays while
    /// wielding it. A subtype without an entry uses the fallback clip, so a new weapon category
    /// works before its own animation exists.
    /// </summary>
    [CreateAssetMenu(menuName = "Character/Weapon Animation Catalog", fileName = "WeaponAnimationCatalog")]
    public sealed class WeaponAnimationCatalog : ScriptableObject
    {
        [Tooltip("Clip used for any weapon category without an entry below.")]
        [SerializeField] private AnimationClip fallbackAttackClip;

        [SerializeField] private List<WeaponAnimationEntry> entries = new List<WeaponAnimationEntry>();

        /// <summary>
        /// Finds the basic-attack clip for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The clip of the matching entry, or the fallback clip when there is no usable entry.</returns>
        public AnimationClip GetAttackClip(WeaponSubtype subtype)
        {
            foreach (var entry in entries)
            {
                if (entry.Subtype == subtype && entry.AttackClip != null)
                {
                    return entry.AttackClip;
                }
            }

            return fallbackAttackClip;
        }
    }
}
