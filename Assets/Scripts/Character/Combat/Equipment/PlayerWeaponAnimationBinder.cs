using UnityEngine;
using Project.Character.Animation;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Keeps the player's basic-attack and idle animations in step with the equipped main-hand weapon: on
    /// every equipment change it looks the weapon's category up in a <see cref="WeaponAnimationCatalog"/>
    /// and hands the resulting clips to <see cref="PlayerAnimatorController"/>.
    /// </summary>
    public sealed class PlayerWeaponAnimationBinder : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private PlayerAnimatorController animatorController;
        [SerializeField] private WeaponAnimationCatalog catalog;

        private WeaponHoldPoseController holdPoseController;

        private void Awake()
        {
            holdPoseController = GetComponent<WeaponHoldPoseController>();

            if (holdPoseController == null)
            {
                holdPoseController = gameObject.AddComponent<WeaponHoldPoseController>();
            }
        }

        private void OnEnable()
        {
            equipment.EquipmentChanged += Refresh;
        }

        private void OnDisable()
        {
            equipment.EquipmentChanged -= Refresh;
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            var subtype = equipment.GetMainHandWeaponSubtype();
            animatorController.SetAttackClip(catalog.GetAttackClip(subtype));
            animatorController.SetIdleClip(catalog.GetIdleClip(subtype));
            animatorController.SetRunClip(catalog.GetRunClip(subtype));
            animatorController.SetAttackImpact(catalog.GetAttackImpactNormalizedTime(subtype));
            holdPoseController.SetPoses(catalog.GetHoldPoses(subtype));
        }
    }
}
