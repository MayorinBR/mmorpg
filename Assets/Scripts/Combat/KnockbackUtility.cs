using UnityEngine;
using UnityEngine.AI;

namespace Project.Combat
{
    /// <summary>
    /// Instantly repositions a <see cref="NavMeshAgent"/> a fixed distance
    /// directly away from a source position — e.g. Arrow Repel pushing its
    /// target back, Back Slide pushing the caster back. Real Ragnarok
    /// Online's own knockback is an instant reposition, not a tween, so
    /// this warps the agent rather than moving it over time.
    /// </summary>
    public static class KnockbackUtility
    {
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
    }
}
