using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Project.Character.Stats;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// Owns the enemy's stats, NavMeshAgent and current target, and drives
    /// the active <see cref="IEnemyState"/> each frame. Behavior logic lives
    /// in the states themselves; this class only holds shared data and
    /// manages state transitions.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CharacterStatsHolder))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float aggroRange = 5f;

        // Carries forward any per-prefab value already tuned under the old
        // field name so this rename doesn't silently reset it to default.
        [FormerlySerializedAs("leashRange")]
        [SerializeField] private float chaseGiveUpRange = 10f;
        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float wanderRadius = 6f;
        [SerializeField] private float minWanderPauseSeconds = 2f;
        [SerializeField] private float maxWanderPauseSeconds = 5f;
        [SerializeField] private EnemyBehaviorMode behaviorMode = EnemyBehaviorMode.Aggressive;
        [SerializeField] private HealthComponent health;

        private CharacterStatsHolder statsHolder;
        private IEnemyState currentState;
        private int? lastKnownHealth;

        private CharacterStatsHolder StatsHolder
        {
            get
            {
                if (statsHolder == null)
                {
                    statsHolder = GetComponent<CharacterStatsHolder>();
                }

                return statsHolder;
            }
        }

        /// <summary>Gets the NavMeshAgent used for wandering and chasing the target.</summary>
        public NavMeshAgent Agent => agent;

        /// <summary>Gets the distance within which the enemy detects and aggros onto a player.</summary>
        public float AggroRange => aggroRange;

        /// <summary>
        /// Gets the maximum distance the target may get from the enemy while
        /// being chased before the enemy gives up. Unlike the old leash
        /// range, this is measured against the target's current position,
        /// not the spawn point, so a chase is free to range anywhere on the
        /// map as long as it stays close enough to the target.
        /// </summary>
        public float ChaseGiveUpRange => chaseGiveUpRange;

        /// <summary>Gets the distance within which the enemy can attack its target.</summary>
        public float AttackRange => attackRange;

        /// <summary>Gets the radius, around a wander state's center point, that random wander destinations are picked within.</summary>
        public float WanderRadius => wanderRadius;

        /// <summary>Gets the minimum time the enemy waits at a wander destination before picking the next one.</summary>
        public float MinWanderPauseSeconds => minWanderPauseSeconds;

        /// <summary>Gets the maximum time the enemy waits at a wander destination before picking the next one.</summary>
        public float MaxWanderPauseSeconds => maxWanderPauseSeconds;

        /// <summary>Gets the enemy's base combat stats.</summary>
        public CharacterStatsDefinition Stats => StatsHolder.Stats;

        /// <summary>Gets whether this mob auto-aggros (Aggressive) or only retaliates when attacked (Passive).</summary>
        public EnemyBehaviorMode BehaviorMode => behaviorMode;

        /// <summary>Gets the world position where the enemy started, used as the default wander center and respawn point.</summary>
        public Vector3 SpawnPosition { get; private set; }

        /// <summary>Gets or sets the current chase/attack target.</summary>
        public Transform PlayerTarget { get; set; }

        /// <summary>
        /// Gets or sets the player this enemy remembers as a threat, even
        /// after giving up an active chase. A Passive mob still checks this
        /// while wandering and re-engages the moment this specific player
        /// comes back within <see cref="ChaseGiveUpRange"/>, bypassing its
        /// usual "only engage when attacked" rule. Cleared on respawn.
        /// </summary>
        public Transform RememberedAggressor { get; set; }

        private void Awake()
        {
            SpawnPosition = transform.position;
            agent.speed = Stats.MoveSpeed;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.HealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.HealthChanged -= HandleHealthChanged;
            }
        }

        private void Start()
        {
            // Left as null (rather than always defaulting to Wander here)
            // when something else has already set an initial state before
            // this ran — e.g. EnemyDeathHandler restoring a chase/attack
            // state saved from before the last map switch. Start() order
            // between sibling components isn't guaranteed, so this check is
            // what makes that restoration safe regardless of which Start()
            // runs first.
            if (currentState == null)
            {
                ChangeState(new EnemyWanderState());
            }
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
        /// Sets the given player as this enemy's current target, remembers
        /// them as a threat for future re-aggro even if this enemy is
        /// Passive, and switches into <see cref="EnemyChaseState"/>.
        /// </summary>
        /// <param name="player">The player to chase.</param>
        public void EngagePlayer(Transform player)
        {
            PlayerTarget = player;
            RememberedAggressor = player;
            ChangeState(new EnemyChaseState());
        }

        /// <summary>
        /// Searches for a player within <see cref="AggroRange"/>.
        /// </summary>
        /// <returns>The closest player transform found, or null if none are in range.</returns>
        public Transform DetectPlayer()
        {
            var hits = Physics.OverlapSphere(transform.position, aggroRange, playerLayer);

            // if (Time.frameCount % 60 == 0)
            // {
            //     Debug.Log($"{name} overlap check: {hits.Length} hit(s), layer mask value = {playerLayer.value}");
            // }

            return hits.Length > 0 ? hits[0].transform : null;
        }

        private void HandleHealthChanged(int current, int max)
        {
            var previous = lastKnownHealth ?? max;
            lastKnownHealth = current;

            if (current >= previous)
            {
                return;
            }

            if (behaviorMode != EnemyBehaviorMode.Passive || PlayerTarget != null)
            {
                return;
            }

            var attacker = DetectPlayer();

            if (attacker != null)
            {
                EngagePlayer(attacker);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, aggroRange);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Application.isPlaying ? SpawnPosition : transform.position, wanderRadius);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, chaseGiveUpRange);
        }
    }
}
