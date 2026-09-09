using System;
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
        private bool isMoving;
        private bool movementLocked;

        /// <summary>
        /// Raised the instant the character transitions from standing still
        /// to moving, regardless of whether the motion came from directional
        /// input or a click-to-move path. Used by UI such as
        /// <see cref="Project.UI.ShopWindowUI"/> to close itself as soon as
        /// the player walks away.
        /// </summary>
        public event Action MovementStarted;

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
            if (movementLocked)
            {
                return;
            }

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
        /// Locks or unlocks this component's own control of the character
        /// entirely — while locked, <see cref="Update"/> does nothing: no
        /// facing changes, no directional or click-to-move motion is
        /// applied, and the <see cref="NavMeshAgent"/> is stopped outright
        /// (<see cref="NavMeshAgent.isStopped"/>), which also freezes its
        /// own local-avoidance nudging against nearby agents. Used by
        /// combat (see <see cref="Combat.PlayerCombatController"/>) so it
        /// can own both facing and stillness while attacking in range,
        /// without this component's own rotation, its
        /// <see cref="NavMeshAgent"/> residual velocity/avoidance, or a
        /// still-held directional key fighting it or making the Animator's
        /// Speed parameter flicker mid-swing (which was making the attack
        /// animation's feet drift, since a flicker to 1 blends toward the
        /// Run pose for a frame).
        /// </summary>
        /// <param name="movementLocked">True to stop this component from moving or rotating the character.</param>
        public void SetMovementLocked(bool movementLocked)
        {
            this.movementLocked = movementLocked;
            agent.isStopped = movementLocked;

            if (movementLocked)
            {
                isMoving = false;
                animatorController?.SetMovementSpeed(0f);
            }
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

            var wasMoving = isMoving;
            isMoving = intent.Direction.sqrMagnitude > 0.0001f;

            if (isMoving)
            {
                transform.forward = intent.Direction;

                if (!wasMoving)
                {
                    MovementStarted?.Invoke();
                }
            }

            animatorController?.SetMovementSpeed(isMoving ? 1f : 0f);
        }
    }
}