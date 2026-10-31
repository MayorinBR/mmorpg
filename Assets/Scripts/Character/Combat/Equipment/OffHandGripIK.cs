using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Relays the off-hand target computed by <see cref="WeaponHoldPoseController"/> to the character's Animator
    /// inverse kinematics. It must sit on the same GameObject as the Animator for Unity to call
    /// <see cref="OnAnimatorIK"/>, and the Animator layer needs its IK Pass enabled.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class OffHandGripIK : MonoBehaviour
    {
        private Animator animator;
        private WeaponHoldPoseController source;

        /// <summary>
        /// Sets the controller that supplies the grip target.
        /// </summary>
        /// <param name="controller">The hold pose controller of the wielded weapon.</param>
        public void Bind(WeaponHoldPoseController controller)
        {
            source = controller;
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            var weight = 0f;
            var position = Vector3.zero;

            if (source != null)
            {
                source.TryGetOffHandTarget(out position, out weight);
            }

            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, weight);

            if (weight > 0f)
            {
                animator.SetIKPosition(AvatarIKGoal.LeftHand, position);
            }
        }
    }
}
