using Project.Character.Animation;
using Project.Items;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Brings the free right hand to the bow while the bow is held at rest. The bow sits in the left hand, so the
    /// right hand is the free one. The run and attack states keep the hand authored in their clips, which keeps the
    /// draw and release animation untouched.
    /// </summary>
    public sealed class BowHandIkPass : IAnimatorIkPass
    {
        private static readonly int RunState = Animator.StringToHash("Run");
        private static readonly int AttackState = Animator.StringToHash("Attack");

        private readonly BowHandIkSettings settings;
        private readonly EquipmentVisualController visuals;
        private readonly EquipmentManager equipment;
        private float weight;

        /// <summary>
        /// Creates the pass.
        /// </summary>
        /// <param name="settings">The hand placement and blend tuning.</param>
        /// <param name="visuals">Source of the bow instance held by the character.</param>
        /// <param name="equipment">Tells whether the main-hand weapon is a ranged weapon.</param>
        public BowHandIkPass(BowHandIkSettings settings, EquipmentVisualController visuals, EquipmentManager equipment)
        {
            this.settings = settings;
            this.visuals = visuals;
            this.equipment = equipment;
        }

        /// <inheritdoc />
        public void Apply(Animator animator, float deltaTime, float masterWeight)
        {
            var bow = visuals.MainHandWeapon;
            var active = bow != null
                && bow.gameObject.activeInHierarchy
                && equipment.IsMainHandWeaponRanged()
                && IsRestState(animator);

            var target = active ? settings.HoldWeight * masterWeight : 0f;
            weight = Mathf.MoveTowards(weight, target, settings.FadeSpeed * deltaTime);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, weight);

            if (weight > 0f && bow != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, bow.position + bow.rotation * settings.GripOffset);
            }
        }

        private static bool IsRestState(Animator animator)
        {
            var hash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;
            return hash != RunState && hash != AttackState && !animator.IsInTransition(0);
        }
    }
}
