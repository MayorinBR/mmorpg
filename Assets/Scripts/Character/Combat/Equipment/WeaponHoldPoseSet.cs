using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// The hold poses of one weapon category for each animation state that changes how the weapon must sit in the
    /// hand: the idle stance, the run cycle and the attack swing. While the set is not defined, the weapon keeps the
    /// pose baked into its prefab.
    /// </summary>
    [Serializable]
    public sealed class WeaponHoldPoseSet
    {
        [SerializeField] private bool defined;
        [SerializeField] private WeaponHoldPose idle;
        [SerializeField] private WeaponHoldPose run;
        [SerializeField] private WeaponHoldPose attack;

        [Tooltip("Point on the weapon root, in the weapon's own space, that the off hand holds.")]
        [SerializeField] private Vector3 offHandGrip;

        /// <summary>
        /// True when the poses below are authored and should drive the weapon.
        /// </summary>
        public bool IsDefined => defined;

        /// <summary>
        /// Pose used while idle, and for every state that has no pose of its own.
        /// </summary>
        public WeaponHoldPose Idle => idle;

        /// <summary>
        /// Pose used while running.
        /// </summary>
        public WeaponHoldPose Run => run;

        /// <summary>
        /// Pose used during the basic attack swing.
        /// </summary>
        public WeaponHoldPose Attack => attack;

        /// <summary>
        /// Point on the weapon root, in the weapon's own space, that the off hand holds while a pose asks for it.
        /// </summary>
        public Vector3 OffHandGrip => offHandGrip;
    }
}
