using System.Collections.Generic;
using UnityEngine;
using Project.Character.Animation;
using Project.Character.Movement;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Owns the player's inverse kinematics: the head and aim follow the selected target, the free hand
    /// meets the bow, and the feet follow steps and slopes. The passes run from an <see cref="AnimatorIkRelay"/> on the model, which is destroyed and
    /// recreated on every gender swap, so this component attaches the relay again after each swap. The two-handed
    /// weapon grip is handled separately by <see cref="OffHandGripIK"/>.
    /// </summary>
    public sealed class PlayerIkController : MonoBehaviour, ILookTargetSource
    {
        [SerializeField] private PlayerGenderController genderController;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private EquipmentManager equipment;

        [Header("Look at and aim")]
        [SerializeField] private bool lookAtEnabled = true;
        [SerializeField] private LookAtIkSettings lookAt = new LookAtIkSettings();

        [Tooltip("Height above the target's position that the head looks at, in metres.")]
        [SerializeField, Min(0f)] private float lookTargetHeight = 1.2f;

        [Header("Bow hand")]
        [SerializeField] private bool bowHandEnabled = true;
        [SerializeField] private EquipmentVisualController equipmentVisuals;
        [SerializeField] private BowHandIkSettings bowHand = new BowHandIkSettings();

        [Header("Feet")]
        [SerializeField] private bool feetEnabled = true;
        [SerializeField] private FootIkSettings feet = new FootIkSettings();

        private void Reset()
        {
            feet.GroundMask = LayerMask.GetMask("Ground");
        }

        private void OnEnable()
        {
            genderController.GenderChanged += HandleGenderChanged;
        }

        private void OnDisable()
        {
            genderController.GenderChanged -= HandleGenderChanged;
        }

        private void Start()
        {
            Attach();
        }

        /// <inheritdoc />
        public bool TryGetLookTarget(out LookTarget target)
        {
            var current = targetSelector != null ? targetSelector.CurrentTarget : null;

            if (current == null)
            {
                target = default;
                return false;
            }

            target = new LookTarget(current.position + Vector3.up * lookTargetHeight, equipment != null && equipment.IsMainHandWeaponRanged());
            return true;
        }

        private void HandleGenderChanged(Stats.CharacterGender gender)
        {
            Attach();
        }

        private void Attach()
        {
            var geometry = genderController.CurrentGeometry;
            var animator = geometry != null ? geometry.GetComponentInChildren<Animator>() : null;

            if (animator == null)
            {
                return;
            }

            if (!animator.TryGetComponent(out AnimatorIkRelay relay))
            {
                relay = animator.gameObject.AddComponent<AnimatorIkRelay>();
            }

            var passes = new List<IAnimatorIkPass>(3);

            if (lookAtEnabled)
            {
                passes.Add(new LookAtIkPass(lookAt, this));
            }

            if (bowHandEnabled && equipmentVisuals != null)
            {
                passes.Add(new BowHandIkPass(bowHand, equipmentVisuals, equipment));
            }

            if (feetEnabled)
            {
                passes.Add(new FootIkPass(feet));
            }

            relay.SetPasses(passes);
        }
    }
}
