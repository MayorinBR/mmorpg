using UnityEngine;
using UnityEngine.AI;

namespace Project.Combat
{
    /// <summary>
    /// Drags a <see cref="NavMeshAgent"/> from where it stands to a destination over a short time, easing out,
    /// then removes itself. Started through <see cref="KnockbackUtility.Slide"/>.
    /// </summary>
    public sealed class KnockbackSlide : MonoBehaviour
    {
        private NavMeshAgent agent;
        private Vector3 start;
        private Vector3 end;
        private float duration;
        private float elapsed;
        private bool wasStopped;

        /// <summary>
        /// Starts, or restarts, a slide on the agent.
        /// </summary>
        /// <param name="agent">The agent to move.</param>
        /// <param name="destination">The NavMesh position the slide ends at.</param>
        /// <param name="duration">How long the slide lasts, in seconds.</param>
        internal static void Begin(NavMeshAgent agent, Vector3 destination, float duration)
        {
            var slide = agent.GetComponent<KnockbackSlide>();

            if (slide == null)
            {
                slide = agent.gameObject.AddComponent<KnockbackSlide>();
                slide.wasStopped = agent.isStopped;
            }

            slide.agent = agent;
            slide.start = agent.transform.position;
            slide.end = destination;
            slide.duration = duration;
            slide.elapsed = 0f;
            agent.isStopped = true;
        }

        private void Update()
        {
            if (agent == null || !agent.isOnNavMesh)
            {
                Destroy(this);
                return;
            }

            elapsed += Time.deltaTime;
            var t = Mathf.Clamp01(elapsed / duration);
            var eased = 1f - (1f - t) * (1f - t);
            agent.Move(Vector3.Lerp(start, end, eased) - transform.position);

            if (t >= 1f)
            {
                agent.isStopped = wasStopped;
                Destroy(this);
            }
        }
    }
}
