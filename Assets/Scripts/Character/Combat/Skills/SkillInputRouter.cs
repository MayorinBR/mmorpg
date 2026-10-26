using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Character.Combat
{
    /// <summary>
    /// Reads Input System shortcuts for the 10 hotbar slots (keys 1-9,
    /// then 0 for the 10th) and casts whichever skill
    /// <see cref="PlayerSkillHotbar"/> currently has assigned to that
    /// slot through <see cref="PlayerSkillCaster"/>.
    /// </summary>
    public class SkillInputRouter : MonoBehaviour
    {
        [SerializeField] private PlayerSkillCaster caster;
        [SerializeField] private PlayerSkillHotbar hotbar;

        /// <summary>Called by the Input System when the Cast Skill 1 shortcut is pressed.</summary>
        public void OnCastSkill1(InputAction.CallbackContext context) => TryCast(context, 0);

        /// <summary>Called by the Input System when the Cast Skill 2 shortcut is pressed.</summary>
        public void OnCastSkill2(InputAction.CallbackContext context) => TryCast(context, 1);

        /// <summary>Called by the Input System when the Cast Skill 3 shortcut is pressed.</summary>
        public void OnCastSkill3(InputAction.CallbackContext context) => TryCast(context, 2);

        /// <summary>Called by the Input System when the Cast Skill 4 shortcut is pressed.</summary>
        public void OnCastSkill4(InputAction.CallbackContext context) => TryCast(context, 3);

        /// <summary>Called by the Input System when the Cast Skill 5 shortcut is pressed.</summary>
        public void OnCastSkill5(InputAction.CallbackContext context) => TryCast(context, 4);

        /// <summary>Called by the Input System when the Cast Skill 6 shortcut is pressed.</summary>
        public void OnCastSkill6(InputAction.CallbackContext context) => TryCast(context, 5);

        /// <summary>Called by the Input System when the Cast Skill 7 shortcut is pressed.</summary>
        public void OnCastSkill7(InputAction.CallbackContext context) => TryCast(context, 6);

        /// <summary>Called by the Input System when the Cast Skill 8 shortcut is pressed.</summary>
        public void OnCastSkill8(InputAction.CallbackContext context) => TryCast(context, 7);

        /// <summary>Called by the Input System when the Cast Skill 9 shortcut is pressed.</summary>
        public void OnCastSkill9(InputAction.CallbackContext context) => TryCast(context, 8);

        /// <summary>Called by the Input System when the Cast Skill 0 shortcut (10th slot) is pressed.</summary>
        public void OnCastSkill0(InputAction.CallbackContext context) => TryCast(context, 9);

        private void TryCast(InputAction.CallbackContext context, int slotIndex)
        {
            if (!context.performed)
            {
                return;
            }

            var skill = hotbar.GetSkill(slotIndex);

            if (skill != null)
            {
                caster.TryCastSkill(skill);
            }
        }
    }
}