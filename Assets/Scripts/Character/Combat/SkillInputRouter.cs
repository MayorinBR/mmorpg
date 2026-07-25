using UnityEngine;
using UnityEngine.InputSystem;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Reads Input System shortcuts for up to four skill slots and casts
    /// the assigned skill through <see cref="PlayerSkillCaster"/>. Slots
    /// are fixed in the Inspector for now, ahead of a proper hotbar UI.
    /// </summary>
    public class SkillInputRouter : MonoBehaviour
    {
        [SerializeField] private PlayerSkillCaster caster;
        [SerializeField] private SkillDefinition skillSlot1;
        [SerializeField] private SkillDefinition skillSlot2;
        [SerializeField] private SkillDefinition skillSlot3;
        [SerializeField] private SkillDefinition skillSlot4;

        /// <summary>Called by the Input System when the Cast Skill 1 shortcut is pressed.</summary>
        public void OnCastSkill1(InputAction.CallbackContext context) => TryCast(context, skillSlot1);

        /// <summary>Called by the Input System when the Cast Skill 2 shortcut is pressed.</summary>
        public void OnCastSkill2(InputAction.CallbackContext context) => TryCast(context, skillSlot2);

        /// <summary>Called by the Input System when the Cast Skill 3 shortcut is pressed.</summary>
        public void OnCastSkill3(InputAction.CallbackContext context) => TryCast(context, skillSlot3);

        /// <summary>Called by the Input System when the Cast Skill 4 shortcut is pressed.</summary>
        public void OnCastSkill4(InputAction.CallbackContext context) => TryCast(context, skillSlot4);

        private void TryCast(InputAction.CallbackContext context, SkillDefinition skill)
        {
            if (context.performed && skill != null)
            {
                caster.TryCastSkill(skill);
            }
        }
    }
}