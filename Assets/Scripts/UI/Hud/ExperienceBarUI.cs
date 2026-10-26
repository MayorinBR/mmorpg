using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Project.UI
{
    /// <summary>
    /// Drives a non-interactive XP <see cref="Slider"/> and a level label
    /// based on <see cref="PlayerExperience"/> events. Initial state is
    /// pulled directly in <see cref="Start"/> rather than relying solely on
    /// the first event, avoiding a stale display if this component enables
    /// before <see cref="PlayerExperience"/> fires its own startup event.
    /// </summary>
    public class ExperienceBarUI : MonoBehaviour
    {
        [SerializeField] private Character.Combat.PlayerExperience experience;
        [SerializeField] private Slider xpSlider;
        [SerializeField] private TMP_Text levelText;

        private void OnEnable()
        {
            experience.ExperienceChanged += UpdateExperience;
            experience.LeveledUp += UpdateLevel;
        }

        private void OnDisable()
        {
            experience.ExperienceChanged -= UpdateExperience;
            experience.LeveledUp -= UpdateLevel;
        }

        private void Start()
        {
            UpdateExperience(experience.CurrentExperience, experience.RequiredExperienceForNextLevel);
            UpdateLevel(experience.CurrentLevel);
        }

        private void UpdateExperience(int current, int required)
        {
            xpSlider.value = required > 0 ? (float)current / required : 0f;
        }

        private void UpdateLevel(int newLevel)
        {
            levelText.text = $"{newLevel}";
        }
    }
}