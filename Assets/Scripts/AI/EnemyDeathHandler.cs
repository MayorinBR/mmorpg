using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Project.Character.Combat;
using Project.Combat;
using Project.Items;

namespace Project.AI
{
    /// <summary>
    /// Reacts to the enemy's death by granting experience, spawning loot,
    /// hiding the enemy, waiting a fixed delay, then reviving it at its
    /// spawn point.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class EnemyDeathHandler : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private EnemyController controller;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Collider hitCollider;
        [SerializeField] private Renderer visualRenderer;
        [SerializeField] private GameObject healthBarRoot;
        [SerializeField] private PlayerExperience playerExperience;
        [SerializeField] private LootTableDefinition lootTable;
        [SerializeField] private ItemPickup itemPickupPrefab;
        [SerializeField] private float respawnDelaySeconds = 5f;

        private void OnEnable()
        {
            health.Died += HandleDeath;
        }

        private void OnDisable()
        {
            health.Died -= HandleDeath;
        }

        private void HandleDeath()
        {
            if (playerExperience != null)
            {
                playerExperience.AddExperience(controller.Stats.ExperienceReward);
            }

            SpawnLoot();
            StartCoroutine(RespawnRoutine());
        }

        private void SpawnLoot()
        {
            if (lootTable == null || itemPickupPrefab == null)
            {
                return;
            }

            foreach (var (item, quantity) in lootTable.RollDrops())
            {
                var pickup = Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
                pickup.Initialize(item, quantity);
            }
        }

        private IEnumerator RespawnRoutine()
        {
            SetAlive(false);

            yield return new WaitForSeconds(respawnDelaySeconds);

            transform.position = controller.SpawnPosition;
            SetAlive(true);
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

            if (healthBarRoot != null)
            {
                healthBarRoot.SetActive(isAlive);
            }
        }
    }
}