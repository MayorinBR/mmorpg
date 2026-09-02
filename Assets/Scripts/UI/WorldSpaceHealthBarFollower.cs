using UnityEngine;

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

            if (viewCamera != null)
            {
                transform.forward = viewCamera.transform.forward;
            }
        }

        /// <summary>
        /// Assigns the camera to billboard toward. Needed on any instance
        /// that survives a scene load (e.g. parented under a persisted
        /// player) while its target camera does not — the previous map's
        /// camera is destroyed on <c>SceneManager.LoadScene</c>, and without
        /// a fresh reference billboarding silently stops, leaving this
        /// object to inherit its parent's rotation instead of facing the
        /// camera.
        /// </summary>
        /// <param name="newViewCamera">The active map's camera.</param>
        public void SetViewCamera(Camera newViewCamera)
        {
            viewCamera = newViewCamera;
        }
    }
}