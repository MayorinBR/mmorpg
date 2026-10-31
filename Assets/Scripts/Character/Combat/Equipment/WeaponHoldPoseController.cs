using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Moves the equipped main-hand weapon between the hold poses of its weapon category as the player's Animator
    /// changes state, so a two-handed weapon sits on the hilt in the idle stance and in the swing, and is carried clear
    /// of the body while running. Does nothing for a weapon category without a defined pose set.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EquipmentVisualController))]
    public sealed class WeaponHoldPoseController : MonoBehaviour
    {
        private static readonly int RunState = Animator.StringToHash("Run");
        private static readonly int AttackState = Animator.StringToHash("Attack");

        private EquipmentVisualController visuals;
        private WeaponHoldPoseSet poses;
        private float offHandWeight;

        private void Awake()
        {
            visuals = GetComponent<EquipmentVisualController>();
        }

        /// <summary>
        /// Sets the poses applied to the main-hand weapon.
        /// </summary>
        /// <param name="value">The pose set of the wielded weapon category, or null to leave the weapon at its prefab pose.</param>
        public void SetPoses(WeaponHoldPoseSet value)
        {
            poses = value;
        }

        private void LateUpdate()
        {
            var weapon = visuals.MainHandWeapon;
            var animator = visuals.CurrentAnimator;

            if (weapon == null || animator == null || poses == null || !poses.IsDefined)
            {
                offHandWeight = 0f;
                return;
            }

            EnsureGripIk(animator);

            var current = PoseFor(animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
            var position = current.LocalPosition;
            var rotation = current.LocalRotation;
            offHandWeight = current.OffHandGripWeight;

            if (animator.IsInTransition(0))
            {
                var next = PoseFor(animator.GetNextAnimatorStateInfo(0).shortNameHash);
                var blend = animator.GetAnimatorTransitionInfo(0).normalizedTime;
                position = Vector3.Lerp(position, next.LocalPosition, blend);
                rotation = Quaternion.Slerp(rotation, next.LocalRotation, blend);
                offHandWeight = Mathf.Lerp(offHandWeight, next.OffHandGripWeight, blend);
            }

            weapon.SetLocalPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Gets where the off hand should hold the weapon and how strongly.
        /// </summary>
        /// <param name="worldPosition">The grip point on the weapon in world space.</param>
        /// <param name="weight">The IK weight from 0 (follow the animation) to 1 (on the grip).</param>
        /// <returns>True when there is a weapon with a grip to hold.</returns>
        public bool TryGetOffHandTarget(out Vector3 worldPosition, out float weight)
        {
            var weapon = visuals.MainHandWeapon;

            if (weapon == null || poses == null || !poses.IsDefined || offHandWeight <= 0f)
            {
                worldPosition = Vector3.zero;
                weight = 0f;
                return false;
            }

            worldPosition = weapon.TransformPoint(poses.OffHandGrip);
            weight = offHandWeight;
            return true;
        }

        private void EnsureGripIk(Animator animator)
        {
            if (!animator.TryGetComponent(out OffHandGripIK relay))
            {
                relay = animator.gameObject.AddComponent<OffHandGripIK>();
            }

            relay.Bind(this);
        }

        private WeaponHoldPose PoseFor(int stateHash)
        {
            if (stateHash == RunState)
            {
                return poses.Run;
            }

            return stateHash == AttackState ? poses.Attack : poses.Idle;
        }
    }
}
