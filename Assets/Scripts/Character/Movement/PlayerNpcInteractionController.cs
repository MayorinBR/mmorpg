using System;
using UnityEngine;
using Project.Combat;
using Project.NPC;

namespace Project.Character.Movement
{
    /// <summary>
    /// Holds the NPC the player has clicked on and walks toward it if out
    /// of range, opening its shop automatically once close enough. Mirrors
    /// the chase-then-act pattern <see cref="PlayerLootController"/> uses
    /// for item pickups. Abandons the approach if the player dies before
    /// reaching the NPC. Clicking the NPC whose shop is already open closes
    /// it instead of re-opening it; <see cref="ClearOpenShop"/> keeps that
    /// state in sync when the shop closes some other way (its close button,
    /// or the player walking off), so the next click on that NPC opens it
    /// again rather than being mistaken for a second toggle-close.
    /// </summary>
    public class PlayerNpcInteractionController : MonoBehaviour
    {
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private HealthComponent playerHealth;
        [SerializeField] private float interactionRange = 2f;

        private NpcShopKeeper pendingNpc;
        private NpcShopKeeper openShopNpc;

        /// <summary>Raised once the player is close enough to the targeted NPC to interact with it.</summary>
        public event Action<NpcShopKeeper> ShopOpened;

        /// <summary>Raised when the player clicks the NPC whose shop is currently open, requesting it close.</summary>
        public event Action ShopCloseRequested;

        private void Update()
        {
            if (pendingNpc == null)
            {
                return;
            }

            if (playerHealth != null && playerHealth.IsDead)
            {
                pendingNpc = null;
                return;
            }

            var distanceToNpc = Vector3.Distance(transform.position, pendingNpc.transform.position);

            if (distanceToNpc > interactionRange)
            {
                movementController.SetClickDestination(pendingNpc.transform.position);
                return;
            }

            movementController.StopMovement();
            var npc = pendingNpc;
            pendingNpc = null;
            openShopNpc = npc;
            npc.AnimationController?.PlayInteract();
            ShopOpened?.Invoke(npc);
        }

        /// <summary>
        /// Sets (or clears, with null) the NPC the player should walk
        /// toward and interact with. Passing the NPC whose shop is already
        /// open requests that it close instead of re-approaching it.
        /// </summary>
        /// <param name="npc">The NPC to approach, or null to cancel.</param>
        public void SetTarget(NpcShopKeeper npc)
        {
            if (npc != null && npc == openShopNpc)
            {
                openShopNpc = null;
                pendingNpc = null;
                npc.AnimationController?.PlayInteract();
                ShopCloseRequested?.Invoke();
                return;
            }

            pendingNpc = npc;
        }

        /// <summary>
        /// Clears the tracked "currently open shop" without requesting a
        /// close, called when the shop window closed some other way (its
        /// own close button, or auto-closing as the player walks off) so
        /// this controller's toggle state doesn't go stale.
        /// </summary>
        public void ClearOpenShop()
        {
            openShopNpc = null;
        }
    }
}
