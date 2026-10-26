using UnityEngine;
using Project.CameraSystem;

namespace Project.UI
{
    /// <summary>
    /// Keeps a world-space canvas positioned above a target transform and
    /// billboarded to face the camera. Kept separate from
    /// <see cref="HealthBarUI"/> so screen-space bars (like the player HUD)
    /// don't carry positioning logic they don't need.
    /// </summary>
    public class WorldSpaceHealthBarFollower : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);
        [SerializeField] private Camera viewCamera;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.position + offset;

            if (viewCamera == null)
            {
                // Falls back to the persisted camera singleton instead of staying
                // unbillboarded. Needed for prefab instances (e.g. enemies) that
                // can't have a scene camera pre-wired at author time, and doubles
                // as the re-wiring path for any instance whose camera reference
                // would otherwise go stale after a map load.
                viewCamera = IsometricCameraController.Instance?.GetComponent<Camera>();
            }

            if (viewCamera != null)
            {
                transform.forward = viewCamera.transform.forward;
            }
        }

        /// <summary>
        /// Explicitly assigns the camera to billboard toward, overriding the
        /// automatic fallback to <see cref="IsometricCameraController.Instance"/>.
        /// </summary>
        /// <param name="newViewCamera">The camera to billboard toward.</param>
        public void SetViewCamera(Camera newViewCamera)
        {
            viewCamera = newViewCamera;
        }
    }
}