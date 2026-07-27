using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.UI
{
    /// <summary>
    /// Reads Input System keyboard shortcuts (e.g. I for Inventory) and
    /// forwards them to <see cref="PlayerUIController.ToggleWindow"/>.
    /// </summary>
    public class UIInputRouter : MonoBehaviour
    {
        [SerializeField] private PlayerUIController uiController;

        /// <summary>Called by the Input System when the Toggle Inventory shortcut is pressed.</summary>
        /// <param name="context">Callback context for the action.</param>
        public void OnToggleInventory(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiController.ToggleWindow("Inventory");
            }
        }

        /// <summary>Called by the Input System when the Toggle Stats shortcut is pressed.</summary>
        /// <param name="context">Callback context for the action.</param>
        public void OnToggleStats(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiController.ToggleWindow("Stats");
            }
        }

        /// <summary>Called by the Input System when the Toggle Equipment shortcut is pressed.</summary>
        /// <param name="context">Callback context for the action.</param>
        public void OnToggleEquipment(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiController.ToggleWindow("Equipment");
            }
        }

        /// <summary>Called by the Input System when the Toggle Skill Hotbar shortcut is pressed.</summary>
        /// <param name="context">Callback context for the action.</param>
        public void OnToggleSkillHotbar(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiController.ToggleWindow("SkillHotbar");
            }
        }

        /// <summary>Called by the Input System when the Toggle Skill Book shortcut is pressed.</summary>
        /// <param name="context">Callback context for the action.</param>
        public void OnToggleSkillBook(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                uiController.ToggleWindow("SkillBook");
            }
        }
    }
}