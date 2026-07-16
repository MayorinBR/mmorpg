using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Project.Character.Combat;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// Reacts to the enemy's death by granting experience to the player,
    /// hiding the enemy, waiting a fixed delay, then reviving it at its
    /// spawn point. Kept as a separate listener so this respawn behavior
    /// can be extended later (loot drops, death animation) without changing
    /// <see cref="HealthComponent"/> or <see cref="EnemyController"/>.
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

            StartCoroutine(RespawnRoutine());
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