using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Project.Items;

namespace Project.Character.Movement
{
    /// <summary>
    /// Reads Input System callbacks (move axis and click/tap point) and
    /// forwards them to a <see cref="CharacterMovementController"/>,
    /// <see cref="PlayerTargetSelector"/>, or <see cref="PlayerLootController"/>
    /// depending on what was clicked. Clicks that land on UI elements are ignored.
    /// </summary>
    public class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private PlayerLootController lootController;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private LayerMask itemLayer;

        private bool isPointerOverUI;

        private void Update()
        {
            isPointerOverUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
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
        /// button) is performed. Clicking an item pickup sets it as the
        /// pending loot target; clicking anywhere else on the ground moves
        /// the character there.
        /// </summary>
        /// <param name="context">Callback context for the move click action.</param>
        public void OnMoveClick(InputAction.CallbackContext context)
        {
            if (!context.performed || isPointerOverUI)
            {
                return;
            }

            var pointerPosition = Pointer.current.position.ReadValue();
            var ray = worldCamera.ScreenPointToRay(pointerPosition);
            var combinedMask = groundLayer | itemLayer;

            if (!Physics.Raycast(ray, out var hit, float.MaxValue, combinedMask))
            {
                return;
            }

            targetSelector.ClearTarget();

            if (hit.collider.TryGetComponent(out ItemPickup pickup))
            {
                lootController.SetTarget(pickup);
                return;
            }

            lootController.SetTarget(null);
            movementController.SetClickDestination(hit.point);
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
                targetSelector.SelectTarget(hit.collider);
            }
        }
    }
}