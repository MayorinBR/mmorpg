using System.Collections.Generic;
using UnityEngine;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Maps each <see cref="WeaponSubtype"/> to the basic-attack and idle clips the player plays while
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

        /// <summary>
        /// Finds the single cast clip for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The cast clip of the matching entry, or null when the category keeps the default channel and cast animations.</returns>
        public AnimationClip GetCastClip(WeaponSubtype subtype)
        {
            return Find(subtype)?.CastClip;
        }

        /// <summary>
        /// Finds the run clip for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The run clip of the matching entry, or null when the category keeps the default run.</returns>
        public AnimationClip GetRunClip(WeaponSubtype subtype)
        {
            var entry = Find(subtype);
            return entry?.RunClip;
        }

        /// <summary>
        /// Finds where the attack blow lands for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The impact moment as a fraction of the attack clip, or 0 when the blow lands as the attack starts.</returns>
        public float GetAttackImpactNormalizedTime(WeaponSubtype subtype)
        {
            var entry = Find(subtype);
            return entry != null ? entry.AttackImpactNormalizedTime : 0f;
        }

        /// <summary>
        /// Finds the hold poses for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The pose set of the matching entry, or null when the category keeps the pose baked into the weapon prefab.</returns>
        public WeaponHoldPoseSet GetHoldPoses(WeaponSubtype subtype)
        {
            var entry = Find(subtype);
            return entry != null && entry.HoldPoses.IsDefined ? entry.HoldPoses : null;
        }

        private WeaponAnimationEntry Find(WeaponSubtype subtype)
        {
            foreach (var entry in entries)
            {
                if (entry.Subtype == subtype)
                {
                    return entry;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the idle clip for a weapon category.
        /// </summary>
        /// <param name="subtype">The category of the wielded weapon, or <see cref="WeaponSubtype.Unarmed"/>.</param>
        /// <returns>The idle clip of the matching entry, or null when the category keeps the default idle.</returns>
        public AnimationClip GetIdleClip(WeaponSubtype subtype)
        {
            foreach (var entry in entries)
            {
                if (entry.Subtype == subtype && entry.IdleClip != null)
                {
                    return entry.IdleClip;
                }
            }

            return null;
        }
    }
}
