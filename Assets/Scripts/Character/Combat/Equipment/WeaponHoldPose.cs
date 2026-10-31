using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Position and rotation of a weapon relative to the hand bone it is attached to, in the bone's local space,
    /// plus how strongly the off hand is pulled onto the weapon's grip while this pose is active.
    /// </summary>
    [Serializable]
    public struct WeaponHoldPose
    {
        [SerializeField] private Vector3 localPosition;
        [SerializeField] private Vector3 localEulerAngles;
        [SerializeField, Range(0f, 1f)] private float offHandGripWeight;

        /// <summary>
        /// Creates a hold pose.
        /// </summary>
        /// <param name="position">Local position of the weapon root in the hand bone's space.</param>
        /// <param name="eulerAngles">Local rotation of the weapon root in the hand bone's space, in degrees.</param>
        /// <param name="offHandWeight">How strongly the off hand is pulled onto the weapon grip, from 0 (clip only) to 1 (on the grip).</param>
        public WeaponHoldPose(Vector3 position, Vector3 eulerAngles, float offHandWeight = 0f)
        {
            localPosition = position;
            localEulerAngles = eulerAngles;
            offHandGripWeight = offHandWeight;
        }

        /// <summary>
        /// Local position of the weapon root in the hand bone's space.
        /// </summary>
        public Vector3 LocalPosition => localPosition;

        /// <summary>
        /// Local rotation of the weapon root in the hand bone's space.
        /// </summary>
        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);

        /// <summary>
        /// How strongly the off hand is pulled onto the weapon grip while this pose is active.
        /// </summary>
        public float OffHandGripWeight => offHandGripWeight;
    }
}
