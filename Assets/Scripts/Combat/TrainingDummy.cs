using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Makes a <see cref="HealthComponent"/> effectively immortal for damage testing: whenever the target
    /// drops to or below <see cref="refillBelowFraction"/> of its maximum health, it is refilled. Damage
    /// still goes through the normal pipeline, so popups, events and mitigation behave exactly like a
    /// regular enemy.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class TrainingDummy : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;

        [Tooltip("Health fraction at or below which the dummy is refilled to full.")]
        [SerializeField, Range(0.1f, 0.9f)] private float refillBelowFraction = 0.5f;

        private void Awake()
        {
            if (health == null)
            {
                health = GetComponent<HealthComponent>();
            }
        }

        private void OnEnable()
        {
            health.HealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            health.HealthChanged -= HandleHealthChanged;
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            if (currentHealth <= maxHealth * refillBelowFraction)
            {
                health.ResetHealth();
            }
        }
    }
}
