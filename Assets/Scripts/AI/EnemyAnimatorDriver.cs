using UnityEngine;
using UnityEngine.AI;

namespace Project.AI
{
    /// <summary>
    /// Feeds an enemy's <see cref="Animator"/> from its AI. Writes the agent's current speed
    /// (and a matching walk playback multiplier, so the legs keep pace with the movement) every
    /// frame, and fires the attack trigger whenever <see cref="EnemyController.AttackStarted"/> is raised.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyAnimatorDriver : MonoBehaviour
    {
        private static readonly int SpeedParameter = Animator.StringToHash("Speed");
        private static readonly int WalkPlaybackParameter = Animator.StringToHash("WalkPlaybackSpeed");
        private static readonly int AttackParameter = Animator.StringToHash("Attack");

        [SerializeField] private Animator animator;
        [SerializeField] private EnemyController controller;
        [SerializeField] private NavMeshAgent agent;

        [Tooltip("Agent speed (units per second) at which the walk clip plays at normal speed. Faster movement speeds the clip up, slower slows it down.")]
        [SerializeField] private float strideSpeed = 2f;

        [SerializeField] private float minWalkPlayback = 0.6f;
        [SerializeField] private float maxWalkPlayback = 1.6f;

        [Tooltip("Smoothing time, in seconds, applied to the Speed parameter so short stops don't flicker between Idle and Walk.")]
        [SerializeField] private float speedDampSeconds = 0.1f;

        private void OnEnable()
        {
            if (controller != null)
            {
                controller.AttackStarted += HandleAttackStarted;
            }
        }

        private void OnDisable()
        {
            if (controller != null)
            {
                controller.AttackStarted -= HandleAttackStarted;
            }
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            var speed = agent != null && agent.enabled && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
            animator.SetFloat(SpeedParameter, speed, speedDampSeconds, Time.deltaTime);
            animator.SetFloat(WalkPlaybackParameter, Mathf.Clamp(speed / strideSpeed, minWalkPlayback, maxWalkPlayback));
        }

        private void HandleAttackStarted()
        {
            if (animator != null)
            {
                animator.SetTrigger(AttackParameter);
            }
        }
    }
}
