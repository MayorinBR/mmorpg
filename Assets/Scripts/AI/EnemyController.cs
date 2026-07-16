using UnityEngine;
using UnityEngine.AI;
using Project.Character.Stats;

namespace Project.AI
{
    /// <summary>
    /// Owns the enemy's stats, NavMeshAgent and current target, and drives
    /// the active <see cref="IEnemyState"/> each frame. Behavior logic lives
    /// in the states themselves; this class only holds shared data and
    /// manages state transitions.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private CharacterStatsDefinition stats;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float aggroRange = 5f;
        [SerializeField] private float leashRange = 10f;
        [SerializeField] private float attackRange = 1.5f;

        private IEnemyState currentState;

        /// <summary>Gets the NavMeshAgent used for chasing the target.</summary>
        public NavMeshAgent Agent => agent;

        /// <summary>Gets the distance within which the enemy detects and aggros onto a player.</summary>
        public float AggroRange => aggroRange;

        /// <summary>Gets the maximum distance from the spawn point before the enemy gives up the chase.</summary>
        public float LeashRange => leashRange;

        /// <summary>Gets the distance within which the enemy can attack its target.</summary>
        public float AttackRange => attackRange;

        /// <summary>Gets the enemy's base combat stats.</summary>
        public CharacterStatsDefinition Stats => stats;

        /// <summary>Gets the world position where the enemy started, used to evaluate the leash range.</summary>
        public Vector3 SpawnPosition { get; private set; }

        /// <summary>Gets or sets the current chase/attack target.</summary>
        public Transform PlayerTarget { get; set; }

        private void Awake()
        {
            SpawnPosition = transform.position;
            agent.speed = stats.MoveSpeed;
        }

        private void Start()
        {
            ChangeState(new EnemyIdleState());
        }

        private void Update()
        {
            currentState?.Tick(this);
        }

        /// <summary>
        /// Switches the active state, calling <see cref="IEnemyState.Exit"/>
        /// on the previous state and <see cref="IEnemyState.Enter"/> on the new one.
        /// </summary>
        /// <param name="newState">The state to transition into.</param>
        public void ChangeState(IEnemyState newState)
        {
            currentState?.Exit(this);
            currentState = newState;
            currentState.Enter(this);
        }

        /// <summary>
        /// Searches for a player within <see cref="AggroRange"/>.
        /// </summary>
        /// <returns>The closest player transform found, or null if none are in range.</returns>
        public Transform DetectPlayer()
        {
            var hits = Physics.OverlapSphere(transform.position, aggroRange, playerLayer);

            return hits.Length > 0 ? hits[0].transform : null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Application.isPlaying ? SpawnPosition : transform.position, leashRange);
        }
    }
}