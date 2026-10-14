using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Positions a ground-level ring/disc indicator under a target
    /// transform each frame, and keeps it hidden while no target is set.
    /// Used for the current-combat-target and skill-target-picking
    /// indicators. The ring sits at the lowest point of the target's
    /// colliders, so it works for any pivot placement (feet, center, or
    /// an agent base offset). Kept target-agnostic (no billboarding, unlike
    /// <see cref="WorldSpaceHealthBarFollower"/>) since a flat disc lying
    /// on the ground doesn't need to face the camera.
    /// </summary>
    public class GroundRingFollower : MonoBehaviour
    {
        [Tooltip("Height of the ring above the lowest point of the target. Raise it if the ground hides part of the ring.")]
        [SerializeField] private float heightAboveGround = 0.1f;

        private Transform target;
        private Collider[] targetColliders;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var position = target.position;
            position.y = GetLowestPointY() + heightAboveGround;
            transform.position = position;
        }

        /// <summary>Shows the ring under the given target.</summary>
        /// <param name="newTarget">The transform to follow.</param>
        public void Show(Transform newTarget)
        {
            target = newTarget;
            targetColliders = newTarget != null ? newTarget.GetComponentsInChildren<Collider>() : null;
            gameObject.SetActive(true);
        }

        /// <summary>Hides the ring until <see cref="Show"/> is called again.</summary>
        public void Hide()
        {
            target = null;
            targetColliders = null;
            gameObject.SetActive(false);
        }

        private float GetLowestPointY()
        {
            var lowest = target.position.y;
            var found = false;

            foreach (var targetCollider in targetColliders)
            {
                if (targetCollider == null || !targetCollider.enabled)
                {
                    continue;
                }

                var minY = targetCollider.bounds.min.y;
                lowest = found ? Mathf.Min(lowest, minY) : minY;
                found = true;
            }

            return lowest;
        }
    }
}
