using System;
using UnityEngine;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Pairs a weapon category with the basic-attack clip played while a weapon of that category
    /// is wielded.
    /// </summary>
    [Serializable]
    public sealed class WeaponAnimationEntry
    {
        [SerializeField] private WeaponSubtype subtype;
        [SerializeField] private AnimationClip attackClip;

        /// <summary>
        /// The weapon category this entry applies to.
        /// </summary>
        public WeaponSubtype Subtype => subtype;

        /// <summary>
        /// The basic-attack clip for the weapon category.
        /// </summary>
        public AnimationClip AttackClip => attackClip;
    }
}
