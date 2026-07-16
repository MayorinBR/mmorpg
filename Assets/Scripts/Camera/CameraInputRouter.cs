using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.CameraSystem
{
    /// <summary>
    /// Reads Input System callbacks for camera rotation and zoom and forwards
    /// them to an <see cref="IsometricCameraController"/>. Kept separate from
    /// the controller so input bindings can change without touching camera logic.
    /// </summary>
    public class CameraInputRouter : MonoBehaviour
    {
        [SerializeField] private IsometricCameraController cameraController;

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
        /// </summary>
        /// <param name="context">Callback context containing a float zoom axis.</param>
        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            var input = context.ReadValue<float>();
            cameraController.SetZoomInput(input);
        }
    }
}