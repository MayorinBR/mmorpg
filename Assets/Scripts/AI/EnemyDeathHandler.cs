using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Project.Character.Combat;
using Project.Combat;
using Project.Items;
using Project.World;

namespace Project.AI
{
    /// <summary>
    /// Reacts to the enemy's death by granting experience, spawning loot,
    /// hiding the enemy, waiting a fixed delay, then reviving it at its
    /// spawn point. Also saves and restores this enemy's position, current
    /// health, alive/dead status, whether it was actively engaging the
    /// player, and pending respawn time across map switches via
    /// <see cref="EnemyWorldStateRegistry"/>, since enemies otherwise have
    /// no persistence between scene loads (unlike the player, which
    /// survives via <c>DontDestroyOnLoad</c>).
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private EnemyController controller;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Collider hitCollider;
        [SerializeField] private Renderer visualRenderer;
        [SerializeField] private Renderer[] additionalRenderers;
        [SerializeField] private GameObject healthBarRoot;
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private PlayerJobProgress playerJobProgress;
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private ItemPickup itemPickupPrefab;
        [SerializeField] private float dropSpreadRadius = 1.5f;
        [SerializeField] private float respawnDelaySeconds = 5f;

        [Tooltip("Stable id used to remember this enemy's state across map switches. Leave blank to use the GameObject's name — only needs to be set explicitly if two enemies share a name within the same scene.")]
        [SerializeField] private string enemyId;

        private bool isCurrentlyAlive = true;
        private float pendingRespawnAtTime;

        private string EffectiveEnemyId => string.IsNullOrEmpty(enemyId) ? gameObject.name : enemyId;

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void Start()
        {
            RestoreWorldStateIfAny();
        }

        private void OnDestroy()
        {
            EnemyWorldStateRegistry.Save(
                gameObject.scene.name,
                EffectiveEnemyId,
                new EnemyWorldState(
                    transform.position,
                    isCurrentlyAlive,
                    pendingRespawnAtTime,
                    health.CurrentHealth,
                    controller.PlayerTarget != null));
        }

        private void HandleDeath()
        {
            if (playerExperience != null)
            {
                playerExperience.AddExperience(controller.Stats.ExperienceReward);
            }

            if (playerJobProgress != null)
            {
                playerJobProgress.AddExperience(controller.Stats.JobExperienceReward);
            }

            SpawnLoot();
            BeginRespawn(respawnDelaySeconds);
        }

        private void SpawnLoot()
        {
            if (lootTable == null || itemPickupPrefab == null)
            {
                return;
            }

            foreach (var (item, quantity) in lootTable.RollDrops())
            {
                var dropPosition = transform.position + GetRandomSpreadOffset();
                var pickup = Instantiate(itemPickupPrefab, dropPosition, Quaternion.identity);
                pickup.Initialize(item, quantity);
            }
        }

        private Vector3 GetRandomSpreadOffset()
        {
            var randomCircle = Random.insideUnitCircle * dropSpreadRadius;
            return new Vector3(randomCircle.x, 0f, randomCircle.y);
        }

        /// <summary>
        /// Checks for state saved by <see cref="OnDestroy"/> the last time
        /// this map was unloaded. An alive enemy is warped back to where it
        /// was left, resumes chasing the player if it was engaging them, and
        /// has its health restored; a dead enemy resumes hiding for whatever
        /// time remains on its respawn countdown (already elapsed while the
        /// map was unloaded counts too, since the countdown is an absolute
        /// time).
        /// </summary>
        private void RestoreWorldStateIfAny()
        {
            if (!EnemyWorldStateRegistry.TryGet(gameObject.scene.name, EffectiveEnemyId, out var savedState))
            {
                return;
            }

            if (savedState.IsAlive)
            {
                agent.Warp(savedState.Position);

                if (savedState.WasEngagingPlayer)
                {
                    ResumeEngagingPlayer();
                }

                // Restored after the engagement check above so that, for a
                // Passive mob, HealthComponent.HealthChanged's own retaliate-
                // on-damage logic (in EnemyController) sees PlayerTarget
                // already set and skips its redundant DetectPlayer() call.
                health.SetCurrentHealth(savedState.CurrentHealth);
                return;
            }

            var remainingSeconds = Mathf.Max(0f, savedState.RespawnAtTime - Time.time);
            BeginRespawn(remainingSeconds);
        }

        private void ResumeEngagingPlayer()
        {
            var player = PersistentPlayerAnchor.Instance;

            if (player == null)
            {
                return;
            }

            controller.PlayerTarget = player.transform;
            controller.ChangeState(new EnemyChaseState());
        }

        private void BeginRespawn(float delaySeconds)
        {
            isCurrentlyAlive = false;
            pendingRespawnAtTime = Time.time + delaySeconds;
            StartCoroutine(RespawnRoutine(delaySeconds));
        }

        private IEnumerator RespawnRoutine(float delaySeconds)
        {
            SetAlive(false);

            yield return new WaitForSeconds(delaySeconds);

            transform.position = controller.SpawnPosition;
            SetAlive(true);
            isCurrentlyAlive = true;
            health.ResetHealth();
            controller.PlayerTarget = null;
            controller.ChangeState(new EnemyIdleState());
        }

        private void SetAlive(bool isAlive)
        {
            visualRenderer.enabled = isAlive;
            hitCollider.enabled = isAlive;
            agent.enabled = isAlive;
            controller.enabled = isAlive;

            foreach (var additionalRenderer in additionalRenderers)
            {
                additionalRenderer.enabled = isAlive;
            }

            if (healthBarRoot != null)
            {
                healthBarRoot.SetActive(isAlive);
            }
        }
    }
}
