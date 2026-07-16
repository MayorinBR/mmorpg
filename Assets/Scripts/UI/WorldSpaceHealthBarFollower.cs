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
    }
}