using System;
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

        [Tooltip("While chasing, landing a hit on this enemy refreshes this grace window — it won't give up on ChaseGiveUpRange alone until this many seconds have passed since the last hit it took. Keeps a ranged/spell attacker sniping from beyond that range from hitting it for free: as long as they keep attacking, the enemy keeps trying to close the distance instead of repeatedly re-engaging and giving up.")]
        [SerializeField] private float chaseGiveUpGraceSeconds = 3f;

        [Tooltip("While wandering, how often (in seconds) an Aggressive mob re-runs DetectPlayer's Physics.OverlapSphere check, instead of every single frame. A player can't cross from outside AggroRange to inside it faster than this interval matters for gameplay feel, so this trades an imperceptible detection delay for a much cheaper wander loop when many enemies are on screen at once.")]
        [SerializeField] private float detectionIntervalSeconds = 0.2f;

        [SerializeField] private float attackRange = 1.5f;
        [SerializeField] private float wanderRadius = 6f;
        [SerializeField] private float minWanderPauseSeconds = 2f;
        [SerializeField] private float maxWanderPauseSeconds = 5f;
        [SerializeField] private EnemyBehaviorMode behaviorMode = EnemyBehaviorMode.Aggressive;
        [SerializeField] private HealthComponent health;

        [Tooltip("Optional. Source of temporary buff/debuff modifiers (see BuffController) affecting this enemy's outgoing attack power — e.g. a Provoke debuff landed on it. Left empty, this enemy is never affected by one.")]
        [SerializeField] private BuffController buffs;

        private CharacterStatsHolder statsHolder;
        private IEnemyState currentState;
        private float lastAttackedTime = float.NegativeInfinity;

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

        /// <summary>
        /// Gets the grace window, in seconds, that a landed hit buys against
        /// giving up on <see cref="ChaseGiveUpRange"/> alone — see
        /// <see cref="LastAttackedTime"/> and <see cref="EnemyChaseState"/>.
        /// </summary>
        public float ChaseGiveUpGraceSeconds => chaseGiveUpGraceSeconds;

        /// <summary>
        /// Gets the <see cref="Time.time"/> this enemy last took a hit (see
        /// <see cref="HandleAttacked"/>), or negative infinity if it never
        /// has. <see cref="EnemyChaseState"/> checks this against
        /// <see cref="ChaseGiveUpGraceSeconds"/> before giving up a chase.
        /// </summary>
        public float LastAttackedTime => lastAttackedTime;

        /// <summary>
        /// Gets how often, in seconds, <see cref="EnemyWanderState"/>
        /// re-runs <see cref="DetectPlayer"/>'s physics overlap check while
        /// wandering, instead of every frame.
        /// </summary>
        public float DetectionIntervalSeconds => detectionIntervalSeconds;

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

        /// <summary>
        /// Gets the temporary buff/debuff modifiers currently affecting
        /// this enemy (see <see cref="BuffController"/>), or null if none
        /// is wired — read by <see cref="EnemyAttackState"/> to apply an
        /// active attack-power modifier, e.g. from Provoke.
        /// </summary>
        public BuffController Buffs => buffs;

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

        /// <summary>
        /// Raised whenever <see cref="EngagePlayer"/> starts a chase, from
        /// fresh detection, passive retaliation, or memory-based re-aggro.
        /// Purely a notification for cosmetic reactions (e.g. an overhead
        /// aggro icon) — nothing in the state machine depends on it.
        /// </summary>
        public event Action PlayerEngaged;

        private void Awake()
        {
            SpawnPosition = transform.position;
            agent.speed = Stats.MoveSpeed;
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.AttackedBy += HandleAttacked;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.AttackedBy -= HandleAttacked;
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
            PlayerEngaged?.Invoke();
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

        /// <summary>
        /// Targets whoever just landed a hit, immediately and regardless of
        /// distance — being hit is proof enough of who the attacker is, so
        /// this doesn't need <see cref="DetectPlayer"/>'s range-limited scan
        /// the way proximity aggro does (see <see cref="EnemyWanderState"/>).
        /// Applies to both <see cref="EnemyBehaviorMode.Aggressive"/> and
        /// <see cref="EnemyBehaviorMode.Passive"/> mobs alike: an Aggressive
        /// mob sniped from outside its own <see cref="AggroRange"/> reacts
        /// just as reliably as a Passive one retaliating.
        /// <see cref="LastAttackedTime"/> refreshes on every hit — even a
        /// repeat hit from the already-current target — so
        /// <see cref="EnemyChaseState"/> keeps pursuing for as long as the
        /// attacks keep landing, instead of giving up mid-chase against a
        /// ranged or spell attacker standing beyond
        /// <see cref="ChaseGiveUpRange"/>.
        /// </summary>
        /// <param name="attacker">The transform that dealt the hit.</param>
        private void HandleAttacked(Transform attacker)
        {
            if (attacker == null)
            {
                return;
            }

            lastAttackedTime = Time.time;

            if (PlayerTarget == attacker)
            {
                return;
            }

            EngagePlayer(attacker);
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
