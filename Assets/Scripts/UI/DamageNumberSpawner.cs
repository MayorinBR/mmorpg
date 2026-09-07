using UnityEngine;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Spawns a floating <see cref="DamagePopup"/> above this character
    /// every time its <see cref="HealthComponent"/> reports damage taken.
    /// Attach alongside a <see cref="HealthComponent"/> on the player and
    /// on each enemy — <see cref="health"/> resolves itself via
    /// <see cref="GetComponent{T}"/> when left unassigned, so adding this
    /// component is the only setup step needed.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private Vector3 spawnOffset = new Vector3(0f, 2f, 0f);
        [SerializeField] private float horizontalJitter = 0.3f;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            health.DamageTaken += HandleDamageTaken;
        }

        private void OnDisable()
        {
            health.DamageTaken -= HandleDamageTaken;
        }

        private void HandleDamageTaken(int amount)
        {
            var jitter = new Vector3(Random.Range(-horizontalJitter, horizontalJitter), 0f, 0f);
            DamagePopup.Create(amount, transform.position + spawnOffset + jitter);
        }
    }
}
