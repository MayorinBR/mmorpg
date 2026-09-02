using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Positions a ground-level ring/disc indicator under a target
    /// transform each frame, and keeps it hidden while no target is set.
    /// Used for the current-combat-target and skill-target-picking
    /// indicators. Kept target-agnostic (no billboarding, unlike
    /// <see cref="WorldSpaceHealthBarFollower"/>) since a flat disc lying
    /// on the ground doesn't need to face the camera.
    /// </summary>
    public class GroundRingFollower : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 0.05f, 0f);

        private Transform target;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = target.position + offset;
        }

        /// <summary>Shows the ring under the given target.</summary>
        /// <param name="newTarget">The transform to follow.</param>
        public void Show(Transform newTarget)
        {
            target = newTarget;
            gameObject.SetActive(true);
        }

        /// <summary>Hides the ring until <see cref="Show"/> is called again.</summary>
        public void Hide()
        {
            target = null;
            gameObject.SetActive(false);
        }
    }
}
