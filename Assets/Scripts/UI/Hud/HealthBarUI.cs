using UnityEngine;
using UnityEngine.UI;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Drives a non-interactive <see cref="Slider"/> based on a
    /// <see cref="HealthComponent"/>'s current/max health, and tints the
    /// fill graphic according to <see cref="colorByPercent"/>. Reusable for
    /// both screen-space (player HUD) and world-space (enemy overhead)
    /// health bars, since it only depends on the health data, not on how
    /// the bar is positioned.
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private HealthComponent health;
        [SerializeField] private Slider slider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Gradient colorByPercent;

        private void OnEnable()
        {
            health.HealthChanged += UpdateSlider;
            UpdateSlider(health.CurrentHealth, health.MaxHealth);
        }

        private void OnDisable()
        {
            health.HealthChanged -= UpdateSlider;
        }

        private void Start()
        {
            UpdateSlider(health.CurrentHealth, health.MaxHealth);
        }

        private void UpdateSlider(int current, int max)
        {
            var percent = max > 0 ? (float)current / max : 0f;
            slider.value = percent;
            fillImage.color = colorByPercent.Evaluate(percent);
        }
    }
}