using UnityEngine;
using UnityEngine.AI;

namespace Project.Combat
{
    /// <summary>
    /// Repositions a <see cref="NavMeshAgent"/> a fixed distance directly
    /// away from a source position: instantly (<see cref="Apply"/>, used by
    /// Back Slide on the caster) or as a visible slide
    /// (<see cref="Slide"/>, used by Arrow Repel on its target).
    /// </summary>
    public static class KnockbackUtility
    {
        private const float SlideSpeed = 14f;
        private const float MinSlideSeconds = 0.15f;

        /// <summary>
        /// Moves <paramref name="agent"/> <paramref name="distance"/>
        /// meters directly away from <paramref name="sourcePosition"/>,
        /// clamped to the nearest valid point on the NavMesh. Falls back to
        /// pushing straight back along the agent's own facing if
        /// <paramref name="sourcePosition"/> is (near) the agent's own
        /// position. No-ops if <paramref name="agent"/> is null, distance
        /// is zero or negative, or no valid NavMesh point is found within
        /// the requested distance.
        /// </summary>
        /// <param name="agent">The agent to reposition.</param>
        /// <param name="sourcePosition">The position to push away from.</param>
        /// <param name="distance">How far to push, in meters.</param>
        public static void Apply(NavMeshAgent agent, Vector3 sourcePosition, float distance)
        {
            if (agent == null || distance <= 0f)
            {
                return;
            }

            var direction = agent.transform.position - sourcePosition;
            direction = direction.sqrMagnitude < 0.0001f ? -agent.transform.forward : direction.normalized;
            var desiredPosition = agent.transform.position + direction * distance;

            if (NavMesh.SamplePosition(desiredPosition, out var hit, distance, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
        }

        /// <summary>
        /// Same destination as <see cref="Apply"/>, but the agent is dragged there over a short time so the push is visible.
        /// The agent is held still while it slides and resumes afterwards.
        /// </summary>
        /// <param name="agent">The agent to push.</param>
        /// <param name="sourcePosition">The position to push away from.</param>
        /// <param name="distance">How far to push, in meters.</param>
        public static void Slide(NavMeshAgent agent, Vector3 sourcePosition, float distance)
        {
            if (agent == null || distance <= 0f)
            {
                return;
            }

            var direction = agent.transform.position - sourcePosition;
            direction = direction.sqrMagnitude < 0.0001f ? -agent.transform.forward : direction.normalized;
            var desiredPosition = agent.transform.position + direction * distance;

            if (NavMesh.SamplePosition(desiredPosition, out var hit, distance, NavMesh.AllAreas))
            {
                KnockbackSlide.Begin(agent, hit.position, Mathf.Max(MinSlideSeconds, distance / SlideSpeed));
            }
        }
    }
}
