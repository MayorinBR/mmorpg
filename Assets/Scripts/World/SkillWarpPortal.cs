using UnityEngine;
using Project.Character.Combat;

namespace Project.World
{
    /// <summary>
    /// A temporary portal opened by Warp Portal. Sends the player to its destination as soon as they step
    /// into it, then removes itself. Uses a horizontal distance check because the player has no collider.
    /// </summary>
    public class SkillWarpPortal : MonoBehaviour
    {
        private ISkillTravelService service;
        private WarpDestination destination;
        private float activationRadius;
        private bool used;

        /// <summary>
        /// Sets where the portal leads.
        /// </summary>
        /// <param name="travelService">The service that moves the player.</param>
        /// <param name="target">Where the portal leads.</param>
        /// <param name="radius">How close, in meters, the player must be to use it.</param>
        public void Initialize(ISkillTravelService travelService, WarpDestination target, float radius)
        {
            service = travelService;
            destination = target;
            activationRadius = Mathf.Max(radius, 0.5f);
        }

        private void Update()
        {
            var player = PersistentPlayerAnchor.Instance;

            if (used || service == null || player == null)
            {
                return;
            }

            var offset = player.transform.position - transform.position;
            offset.y = 0f;

            if (offset.sqrMagnitude > activationRadius * activationRadius)
            {
                return;
            }

            used = true;
            service.TravelTo(destination);
            Destroy(gameObject);
        }
    }
}
