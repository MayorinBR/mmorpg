using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Project.Combat;
using Project.Items;
using Project.NPC;

namespace Project.Character.Movement
{
    /// <summary>
    /// Reads Input System callbacks (move axis and click/tap point) and
    /// forwards them to a <see cref="CharacterMovementController"/>,
    /// <see cref="PlayerTargetSelector"/>, <see cref="PlayerLootController"/>,
    /// or <see cref="PlayerNpcInteractionController"/> depending on what was
    /// clicked. Clicks that land on UI elements are ignored.
    /// </summary>
    public class PlayerInputRouter : MonoBehaviour
    {
        private const float NavMeshProbeStep = 0.25f;
        private const float NavMeshProbeRadius = 0.3f;
        private const float SameSurfaceTolerance = 0.5f;
        private const float MaxNavMeshProbeDistance = 200f;

        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private PlayerLootController lootController;
        [SerializeField] private PlayerNpcInteractionController npcInteractionController;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask itemLayer;
        [SerializeField] private LayerMask npcLayer;

        private bool isPointerOverUI;
        private bool clicksSuppressed;
        private Transform hoveredEnemy;

        /// <summary>Raised when the living enemy under the cursor changes, including to null.</summary>
        public event Action<Transform> HoveredEnemyChanged;

        /// <summary>Raised with the clicked world point after a plain ground click sends the character walking there.</summary>
        public event Action<Vector3> GroundClicked;

        private void Update()
        {
            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            UpdateHoveredEnemy();
        }

        /// <summary>
        /// Raycasts the enemy layer under the cursor and raises
        /// <see cref="HoveredEnemyChanged"/> with the transform owning the
        /// enemy's <see cref="IDamageable"/>, matching how
        /// <see cref="PlayerTargetSelector.SelectTarget"/> resolves a target.
        /// </summary>
        private void UpdateHoveredEnemy()
        {
            Transform enemy = null;

            if (!isPointerOverUI && worldCamera != null && Pointer.current != null)
            {
                var ray = worldCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

                if (Physics.Raycast(ray, out var hit, float.MaxValue, enemyLayer))
                {
                    var damageable = hit.collider.GetComponentInParent<IDamageable>();

                    if (damageable != null && !damageable.IsDead)
                    {
                        enemy = (damageable as Component)?.transform ?? hit.collider.transform;
                    }
                }
            }

            if (enemy == hoveredEnemy)
            {
                return;
            }

            hoveredEnemy = enemy;
            HoveredEnemyChanged?.Invoke(hoveredEnemy);
        }

        /// <summary>
        /// Called by the Input System when the Move action changes value,
        /// carrying the WASD or gamepad stick axis.
        /// </summary>
        /// <param name="context">Callback context containing the Vector2 axis value.</param>
        public void OnMove(InputAction.CallbackContext context)
        {
            var axis = context.ReadValue<Vector2>();
            movementController.SetDirectionalAxis(axis);
        }

        /// <summary>
        /// Called by the Input System when the Move Click action (left mouse
        /// button) is performed. Clicking an NPC sets it as the pending
        /// interaction target; clicking an item pickup sets it as the
        /// pending loot target; clicking anywhere else on the ground moves
        /// the character there.
        /// </summary>
        /// <param name="context">Callback context for the move click action.</param>
        public void OnMoveClick(InputAction.CallbackContext context)
        {
            if (!context.performed || isPointerOverUI || clicksSuppressed)
            {
                return;
            }

            var pointerPosition = Pointer.current.position.ReadValue();
            var ray = worldCamera.ScreenPointToRay(pointerPosition);
            var combinedMask = groundLayer | itemLayer | npcLayer;
            var hasHit = Physics.Raycast(ray, out var hit, float.MaxValue, combinedMask);

            if (!TryResolveWalkPoint(ray, hasHit, hit, out var destination))
            {
                return;
            }

            targetSelector.ClearTarget();

            if (hasHit && hit.collider.TryGetComponent(out NpcDialogueController npc))
            {
                lootController.SetTarget(null);
                npcInteractionController.SetTarget(npc);
                return;
            }

            if (hasHit && hit.collider.TryGetComponent(out ItemPickup pickup))
            {
                npcInteractionController.SetTarget(null);
                lootController.SetTarget(pickup);
                return;
            }

            lootController.SetTarget(null);
            npcInteractionController.SetTarget(null);
            movementController.SetClickDestination(destination);
            GroundClicked?.Invoke(destination);
        }

        /// <summary>
        /// Finds the first NavMesh surface the click ray passes through, so a bridge or pier baked into the
        /// NavMesh wins over the ground beneath it, even without a collider and even when no ground lies
        /// beneath it at all. Keeps the exact physics hit when the NavMesh point lies on the same surface.
        /// </summary>
        /// <param name="ray">The click ray from the camera.</param>
        /// <param name="hasHit">Whether the ray hit a collider on the clickable layers.</param>
        /// <param name="hit">The physics hit, valid only when <paramref name="hasHit"/> is true.</param>
        /// <param name="point">The world point the character should walk to.</param>
        /// <returns>False when the ray crosses no NavMesh and hits nothing.</returns>
        private static bool TryResolveWalkPoint(Ray ray, bool hasHit, RaycastHit hit, out Vector3 point)
        {
            var maxDistance = hasHit ? hit.distance : MaxNavMeshProbeDistance;

            for (var distance = 0f; distance <= maxDistance; distance += NavMeshProbeStep)
            {
                if (!NavMesh.SamplePosition(ray.GetPoint(distance), out var navHit, NavMeshProbeRadius, NavMesh.AllAreas))
                {
                    continue;
                }

                var sameSurface = hasHit && (navHit.position - hit.point).sqrMagnitude <= SameSurfaceTolerance * SameSurfaceTolerance;
                point = sameSurface ? hit.point : navHit.position;
                return true;
            }

            point = hit.point;
            return hasHit;
        }

        /// <summary>
        /// Blocks or allows the move click (left button) so a click that picks
        /// a skill target is not also read as a walk order.
        /// </summary>
        /// <param name="suppressed">True to ignore move clicks.</param>
        public void SetClicksSuppressed(bool suppressed)
        {
            clicksSuppressed = suppressed;
        }

        /// <summary>
        /// Assigns the camera used to convert screen clicks into world-space
        /// rays. Called by <see cref="Project.World.MapBootstrap"/> after a
        /// map loads, since this component is persisted across scene loads
        /// (<see cref="Project.World.PersistentPlayerAnchor"/>) while each
        /// map's camera is not, so a plain Inspector reference would go
        /// stale the moment the previous map's camera is destroyed.
        /// </summary>
        /// <param name="newWorldCamera">The active map's camera.</param>
        public void SetWorldCamera(Camera newWorldCamera)
        {
            worldCamera = newWorldCamera;
        }

        /// <summary>
        /// Called by the Input System when the Attack Click action (right
        /// mouse button) is performed, casting a ray against the enemy
        /// layer to select a combat target.
        /// </summary>
        /// <param name="context">Callback context for the attack click action.</param>
        public void OnAttackClick(InputAction.CallbackContext context)
        {
            if (!context.performed || isPointerOverUI)
            {
                return;
            }

            var pointerPosition = Pointer.current.position.ReadValue();
            var ray = worldCamera.ScreenPointToRay(pointerPosition);

            if (Physics.Raycast(ray, out var hit, float.MaxValue, enemyLayer))
            {
                lootController.SetTarget(null);
                npcInteractionController.SetTarget(null);
                targetSelector.SelectTarget(hit.collider);
            }
        }
    }
}