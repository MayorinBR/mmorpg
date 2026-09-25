using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.CameraSystem
{
    /// <summary>
    /// Persists across scene loads and forwards Input System camera
    /// rotate/zoom callbacks to whichever map's <see cref="CameraInputRouter"/>
    /// is currently active. <see cref="PlayerInput"/>'s Invoke Unity Events
    /// binding needs a fixed target object at author time, but each map's
    /// <see cref="CameraInputRouter"/> is destroyed on
    /// <c>SceneManager.LoadScene</c> — this indirection is what lets the
    /// binding stay valid across map switches, re-targeted by
    /// <see cref="Project.World.MapBootstrap"/> on every load.
    /// </summary>
    public class CameraInputForwarder : MonoBehaviour
    {
        private CameraInputRouter activeRouter;

        /// <summary>
        /// Points this forwarder at the current map's input router. Called by
        /// <see cref="Project.World.MapBootstrap"/> once per map load.
        /// </summary>
        /// <param name="newRouter">The active map's camera input router.</param>
        public void SetActiveRouter(CameraInputRouter newRouter)
        {
            activeRouter = newRouter;
        }

        /// <summary>
        /// Called by the Input System when the Rotate Camera action changes value.
        /// </summary>
        /// <param name="context">Callback context containing a float rotation axis.</param>
        public void OnRotateCamera(InputAction.CallbackContext context)
        {
            activeRouter?.OnRotateCamera(context);
        }

        /// <summary>
        /// Called by the Input System when the Zoom Camera action changes value.
        /// </summary>
        /// <param name="context">Callback context containing a float zoom axis.</param>
        public void OnZoomCamera(InputAction.CallbackContext context)
        {
            activeRouter?.OnZoomCamera(context);
        }
    }
}
