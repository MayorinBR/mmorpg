using UnityEngine;
using UnityEngine.AI;

namespace Project.Character.Movement
{
    /// <summary>
    /// Produces a movement intent from raw directional input (WASD or gamepad stick).
    /// Reads a normalized 2D axis provided by the Input System and projects it onto
    /// the horizontal plane.
    /// </summary>
    public class DirectionalMovementProvider
    {
        /// <summary>
        /// Builds a movement intent from a 2D input axis, rotated to align
        /// with the camera's current facing direction.
        /// </summary>
        /// <param name="axis">Normalized input axis, x = horizontal, y = forward/back.</param>
        /// <param name="cameraYaw">The camera's current yaw, in degrees, used to make input camera-relative.</param>
        /// <returns>A movement intent describing the resulting world-space direction.</returns>
        public IMovementIntent BuildIntent(Vector2 axis, float cameraYaw)
        {
            var rawDirection = new Vector3(axis.x, 0f, axis.y);
            var hasInput = rawDirection.sqrMagnitude > 0.0001f;
            var direction = hasInput ? Quaternion.Euler(0f, cameraYaw, 0f) * rawDirection.normalized : Vector3.zero;

            return new DirectionalIntent(direction, cancelsPathing: hasInput);
        }

        private readonly struct DirectionalIntent : IMovementIntent
        {
            public DirectionalIntent(Vector3 direction, bool cancelsPathing)
            {
                Direction = direction;
                CancelsPathing = cancelsPathing;
            }

            public bool CancelsPathing { get; }
            public Vector3 Direction { get; }
        }
    }

    /// <summary>
    /// Produces a movement intent from a NavMesh path toward a clicked or tapped
    /// destination point.
    /// </summary>
    public class ClickToMoveProvider
    {
        private readonly NavMeshAgent agent;

        /// <summary>
        /// Initializes the provider with the agent that owns the current path.
        /// </summary>
        /// <param name="agent">The NavMeshAgent used to resolve the path direction.</param>
        public ClickToMoveProvider(NavMeshAgent agent)
        {
            this.agent = agent;
        }

        /// <summary>
        /// Sets a new destination, replacing any path currently in progress.
        /// </summary>
        /// <param name="destination">World-space point to move toward.</param>
        public void SetDestination(Vector3 destination)
        {
            agent.SetDestination(destination);
        }

        /// <summary>
        /// Builds a movement intent from the agent's current desired velocity.
        /// Returns a zero-direction intent once the destination is reached.
        /// </summary>
        /// <returns>A movement intent describing the current pathing direction.</returns>
        public IMovementIntent BuildIntent()
        {
            var reachedDestination = agent.remainingDistance <= agent.stoppingDistance;
            var direction = reachedDestination ? Vector3.zero : agent.desiredVelocity.normalized;

            return new PathingIntent(direction);
        }

        private readonly struct PathingIntent : IMovementIntent
        {
            public PathingIntent(Vector3 direction)
            {
                Direction = direction;
            }

            public bool CancelsPathing => false;
            public Vector3 Direction { get; }
        }
    }
}