using UnityEngine;
using UnityEngine.AI;

namespace Project.AI
{
    /// <summary>
    /// Default state: the enemy roams to random points within
    /// <see cref="EnemyController.WanderRadius"/> of a center position,
    /// pausing between destinations, instead of standing still. The center
    /// is the enemy's spawn point unless a different one is supplied (used
    /// when a chase is abandoned, so the enemy keeps wandering around
    /// wherever it ended up rather than snapping back home).
    /// While wandering, an Aggressive mob keeps scanning for a player
    /// entering its aggro range — at most every
    /// <see cref="EnemyController.DetectionIntervalSeconds"/> rather than
    /// every single frame, since <see cref="EnemyController.DetectPlayer"/>
    /// is a physics query and most ticks in between wouldn't have changed
    /// its answer anyway — and any mob — Passive included — checks whether
    /// its <see cref="EnemyController.RememberedAggressor"/> has come back
    /// within <see cref="EnemyController.ChaseGiveUpRange"/>. Either
    /// detection transitions to <see cref="EnemyChaseState"/>.
    /// </summary>
    public class EnemyWanderState : IEnemyState
    {
        private const float DestinationReachedThreshold = 0.2f;

        private readonly Vector3? explicitWanderCenter;
        private Vector3 wanderCenter;
        private bool isWaitingAtDestination;
        private float waitTimeRemaining;
        private float timeUntilNextDetectionCheck;

        /// <summary>
        /// Creates the wander state.
        /// </summary>
        /// <param name="wanderCenter">
        /// The point to wander around. Pass null to use the enemy's own
        /// <see cref="EnemyController.SpawnPosition"/>.
        /// </param>
        public EnemyWanderState(Vector3? wanderCenter = null)
        {
            explicitWanderCenter = wanderCenter;
        }

        /// <inheritdoc />
        public void Enter(EnemyController enemy)
        {
            wanderCenter = explicitWanderCenter ?? enemy.SpawnPosition;
            timeUntilNextDetectionCheck = 0f;
            PickNewDestination(enemy);
        }

        /// <inheritdoc />
        public void Tick(EnemyController enemy)
        {
            if (TryReaggroRememberedPlayer(enemy))
            {
                return;
            }

            if (enemy.BehaviorMode == EnemyBehaviorMode.Aggressive)
            {
                timeUntilNextDetectionCheck -= Time.deltaTime;

                if (timeUntilNextDetectionCheck <= 0f)
                {
                    timeUntilNextDetectionCheck = enemy.DetectionIntervalSeconds;
                    var detectedPlayer = enemy.DetectPlayer();

                    if (detectedPlayer != null)
                    {
                        enemy.EngagePlayer(detectedPlayer);
                        return;
                    }
                }
            }

            TickWander(enemy);
        }

        /// <inheritdoc />
        public void Exit(EnemyController enemy)
        {
        }

        private bool TryReaggroRememberedPlayer(EnemyController enemy)
        {
            var rememberedPlayer = enemy.RememberedAggressor;

            if (rememberedPlayer == null)
            {
                return false;
            }

            var distanceToRemembered = Vector3.Distance(enemy.transform.position, rememberedPlayer.position);

            if (distanceToRemembered > enemy.ChaseGiveUpRange)
            {
                return false;
            }

            enemy.EngagePlayer(rememberedPlayer);
            return true;
        }

        private void TickWander(EnemyController enemy)
        {
            if (isWaitingAtDestination)
            {
                waitTimeRemaining -= Time.deltaTime;

                if (waitTimeRemaining <= 0f)
                {
                    PickNewDestination(enemy);
                }

                return;
            }

            if (!enemy.Agent.pathPending && enemy.Agent.remainingDistance <= DestinationReachedThreshold)
            {
                BeginWaiting(enemy);
            }
        }

        private void PickNewDestination(EnemyController enemy)
        {
            var randomOffset = Random.insideUnitCircle * enemy.WanderRadius;
            var candidatePosition = wanderCenter + new Vector3(randomOffset.x, 0f, randomOffset.y);

            if (NavMesh.SamplePosition(candidatePosition, out var hit, enemy.WanderRadius, NavMesh.AllAreas))
            {
                isWaitingAtDestination = false;
                enemy.Agent.SetDestination(hit.position);
            }
            else
            {
                // No walkable point found near this sample; wait out this
                // cycle and try again next time rather than issuing an
                // invalid destination.
                BeginWaiting(enemy);
            }
        }

        private void BeginWaiting(EnemyController enemy)
        {
            isWaitingAtDestination = true;
            waitTimeRemaining = Random.Range(enemy.MinWanderPauseSeconds, enemy.MaxWanderPauseSeconds);
        }
    }
}
