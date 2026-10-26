using UnityEngine;
using Project.Character.Animation;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Keeps the player's basic-attack animation in step with the equipped main-hand weapon: on
    /// every equipment change it looks the weapon's category up in a <see cref="WeaponAnimationCatalog"/>
    /// and hands the resulting clip to <see cref="PlayerAnimatorController"/>.
    /// </summary>
    public sealed class PlayerWeaponAnimationBinder : MonoBehaviour
    {
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private PlayerAnimatorController animatorController;
        [SerializeField] private WeaponAnimationCatalog catalog;

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
            animatorController.SetAttackClip(catalog.GetAttackClip(equipment.GetMainHandWeaponSubtype()));
        }
    }
}
