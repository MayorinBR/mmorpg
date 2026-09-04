using UnityEngine;
using Project.Combat;
using Project.Items;

namespace Project.Character.Movement
{
    /// <summary>
    /// Holds the item pickup the player has clicked on and walks toward it
    /// if out of range, collecting it automatically once close enough.
    /// Mirrors the chase-then-act pattern used by combat auto-attack.
    /// Abandons the pursuit if the player dies before reaching it.
    /// </summary>
    public class PlayerLootController : MonoBehaviour
    {
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private float collectRange = 1.5f;

        private ItemPickup pendingPickup;

        private void Update()
        {
            if (pendingPickup == null)
            {
                return;
            }

            if (playerHealth != null && playerHealth.IsDead)
            {
                pendingPickup = null;
                return;
            }

            var distanceToPickup = Vector3.Distance(transform.position, pendingPickup.transform.position);

            if (distanceToPickup > collectRange)
            {
                movementController.SetClickDestination(pendingPickup.transform.position);
                return;
            }

            movementController.StopMovement();
            pendingPickup.TryCollect(inventory);
            pendingPickup = null;
        }

        /// <summary>
        /// Sets (or clears, with null) the pickup the player should walk
        /// toward and collect.
        /// </summary>
        /// <param name="pickup">The pickup to pursue, or null to cancel.</param>
        public void SetTarget(ItemPickup pickup)
        {
            pendingPickup = pickup;
        }
    }
}