using System;
using UnityEngine;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Pairs a weapon category with the basic-attack, idle and run clips, the attack impact moment and the hold
    /// poses used while a weapon of that category is wielded.
    /// </summary>
    [Serializable]
    public sealed class WeaponAnimationEntry
    {
        [SerializeField] private WeaponSubtype subtype;
        [SerializeField] private AnimationClip attackClip;
        [Tooltip("Optional. Idle clip for this weapon category; empty keeps the default idle.")]
        [SerializeField] private AnimationClip idleClip;
        [Tooltip("Optional. Run clip for this weapon category; empty keeps the default run.")]
        [SerializeField] private AnimationClip runClip;
        [Tooltip("Where in the attack clip the blow lands, from 0 (first frame) to 1 (last frame). The hit is applied at that moment; 0 applies it as the attack starts.")]
        [SerializeField, Range(0f, 1f)] private float attackImpactNormalizedTime;
        [SerializeField] private WeaponHoldPoseSet holdPoses = new WeaponHoldPoseSet();

        /// <summary>
        /// The weapon category this entry applies to.
        /// </summary>
        public WeaponSubtype Subtype => subtype;

        /// <summary>
        /// The basic-attack clip for the weapon category.
        /// </summary>
        public AnimationClip AttackClip => attackClip;

        /// <summary>
        /// The idle clip for the weapon category, or null to keep the default idle.
        /// </summary>
        public AnimationClip IdleClip => idleClip;

        /// <summary>
        /// The run clip for the weapon category, or null to keep the default run.
        /// </summary>
        public AnimationClip RunClip => runClip;

        /// <summary>
        /// The moment the attack blow lands, as a fraction of the attack clip.
        /// </summary>
        public float AttackImpactNormalizedTime => attackImpactNormalizedTime;

        /// <summary>
        /// How the weapon is held in the idle, run and attack states.
        /// </summary>
        public WeaponHoldPoseSet HoldPoses => holdPoses;
    }
}
