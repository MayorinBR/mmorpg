using UnityEngine;
using UnityEngine.AI;
using Project.CameraSystem;
using Project.Character.Animation;

namespace Project.Character.Movement
{
    /// <summary>
    /// Decides, every frame, which movement source is in control of the
    /// character and applies the resulting motion. Directional input (WASD
    /// or gamepad) always takes priority and cancels any path in progress;
    /// otherwise the character continues following its click-to-move path.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class CharacterMovementController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private MonoBehaviour cameraYawSource;
        [SerializeField] private PlayerAnimatorController animatorController;

        private DirectionalMovementProvider directionalProvider;
        private ClickToMoveProvider clickToMoveProvider;
        private ICameraYawProvider cameraYawProvider;
        private Vector2 directionalAxis;

        private void Awake()
        {
            directionalProvider = new DirectionalMovementProvider();
            clickToMoveProvider = new ClickToMoveProvider(agent);
            cameraYawProvider = cameraYawSource as ICameraYawProvider;
            // Debug.Log(cameraYawProvider != null ? "Camera yaw provider resolved correctly." : "Camera yaw provider is NULL, check cameraYawSource assignment.");
            agent.speed = moveSpeed;
        }

        private void Update()
        {
            var cameraYaw = cameraYawProvider != null ? cameraYawProvider.CurrentYaw : 0f;
            var directionalIntent = directionalProvider.BuildIntent(directionalAxis, cameraYaw);

            var activeIntent = directionalIntent.CancelsPathing
                ? directionalIntent
                : clickToMoveProvider.BuildIntent();

            Apply(activeIntent, directionalIntent.CancelsPathing);
        }

        /// <summary>
        /// Sets the current directional input axis, typically called from an
        /// Input System callback for keyboard or gamepad movement.
        /// </summary>
        /// <param name="axis">Normalized input axis, x = horizontal, y = forward/back.</param>
        public void SetDirectionalAxis(Vector2 axis)
        {
            if (!enabled)
            {
                return;
            }

            directionalAxis = axis;
        }

        /// <summary>
        /// Requests a click-to-move path toward the given world position,
        /// typically called from a mouse click or touch tap handler.
        /// </summary>
        /// <param name="destination">World-space point to move toward.</param>
        public void SetClickDestination(Vector3 destination)
        {
            if (!enabled)
            {
                return;
            }

            clickToMoveProvider.SetDestination(destination);
        }

        /// <summary>
        /// Cancels any path currently in progress, typically called when
        /// another system (such as combat) needs to stop the character in place.
        /// </summary>
        public void StopMovement()
        {
            agent.ResetPath();
        }

        /// <summary>
        /// Teleports the character to a new position, keeping the
        /// NavMeshAgent properly synced. Use this instead of setting
        /// transform.position directly (e.g. on respawn).
        /// </summary>
        /// <param name="position">The world-space position to warp to.</param>
        public void WarpTo(Vector3 position)
        {
            if (!agent.Warp(position))
            {
                Debug.LogWarning($"CharacterMovementController.WarpTo: {position} is not close enough to a baked NavMesh, character was not moved.", this);
            }
        }

        /// <summary>
        /// Assigns the camera used to read yaw for camera-relative WASD
        /// movement. Called by <see cref="Project.World.MapBootstrap"/>
        /// after a map loads, since this component is persisted across
        /// scene loads while <paramref name="newSource"/> is not
        /// (<see cref="cameraYawSource"/> is only ever wired for the
        /// scene this component originally loaded in).
        /// </summary>
        /// <param name="newSource">The active map's camera yaw provider.</param>
        public void SetCameraYawSource(ICameraYawProvider newSource)
        {
            cameraYawProvider = newSource;
        }

        private void Apply(IMovementIntent intent, bool isDirectMovement)
        {
            if (isDirectMovement)
            {
                agent.ResetPath();
                agent.Move(intent.Direction * moveSpeed * Time.deltaTime);
            }

            var isMoving = intent.Direction.sqrMagnitude > 0.0001f;

            if (isMoving)
            {
                transform.forward = intent.Direction;
            }

            animatorController?.SetMovementSpeed(isMoving ? 1f : 0f);
        }
    }
}