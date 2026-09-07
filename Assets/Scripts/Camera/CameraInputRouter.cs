using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

namespace Project.CameraSystem
{
    /// <summary>
    /// Reads Input System callbacks for camera rotation and zoom and forwards
    /// them to an <see cref="IsometricCameraController"/>. Kept separate from
    /// the controller so input bindings can change without touching camera logic.
    /// Zoom is ignored while the pointer is over UI (e.g. scrolling the shop
    /// window) so the game camera doesn't zoom at the same time.
    /// </summary>
    public class CameraInputRouter : MonoBehaviour
    {
        [SerializeField] private IsometricCameraController cameraController;

        private bool isPointerOverUI;

        private void Update()
        {
            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Called by the Input System when the Rotate Camera action changes value.
        /// </summary>
        /// <param name="context">Callback context containing a float rotation axis.</param>
        public void OnRotateCamera(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<float>();
            cameraController.SetRotationInput(input);
        }

        /// <summary>
        /// Called by the Input System when the Zoom Camera action changes value.
        /// Ignored while the pointer is over UI, so scrolling a UI window
        /// (e.g. the shop's buy list) doesn't also zoom the game camera.
        /// </summary>
        /// <param name="context">Callback context containing a float zoom axis.</param>
        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            if (isPointerOverUI)
            {
                return;
            }

            var input = context.ReadValue<float>();
            cameraController.SetZoomInput(input);
        }
    }
}