using UnityEngine;
using UnityEngine.UI;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Drives a non-interactive <see cref="Slider"/> based on a
    /// <see cref="ManaComponent"/>'s current/max mana. The fill uses a
    /// fixed solid color set directly on its Image in the Inspector.
    /// </summary>
    public class ManaBarUI : MonoBehaviour
    {
        [SerializeField] private ManaComponent mana;
        [SerializeField] private Slider slider;

        private void OnEnable()
        {
            mana.ManaChanged += UpdateSlider;
            UpdateSlider(mana.CurrentMana, mana.MaxMana);
        }

        private void OnDisable()
        {
            mana.ManaChanged -= UpdateSlider;
        }

        private void Start()
        {
            UpdateSlider(mana.CurrentMana, mana.MaxMana);
        }

        private void UpdateSlider(int current, int max)
        {
            slider.value = max > 0 ? (float)current / max : 0f;
        }
    }
}