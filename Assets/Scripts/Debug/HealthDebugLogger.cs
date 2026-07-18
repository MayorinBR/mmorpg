using UnityEngine;
using Project.Combat;

namespace Project.DebugTools
{
    /// <summary>
    /// Temporary debug helper that logs health changes and death to the
    /// Console. Intended to validate <see cref="HealthComponent"/> before a
    /// real health bar UI exists; remove once that UI is in place.
    /// </summary>
    [RequireComponent(typeof(HealthComponent))]
    public class HealthDebugLogger : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;

        private void OnEnable()
        {
            health.HealthChanged += LogHealthChanged;
            health.Died += LogDeath;
        }

        private void OnDisable()
        {
            health.HealthChanged -= LogHealthChanged;
            health.Died -= LogDeath;
        }

        private void LogHealthChanged(int current, int max)
        {
            Debug.Log($"{name} health: {current}/{max}");
        }

        private void LogDeath()
        {
            Debug.Log($"{name} died.");
        }
    }
}