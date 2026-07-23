using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Combat;
using Project.Character.Stats;
using Project.Character.Combat;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// Drives the main player HUD: name, HP/SP bars with numeric text,
    /// base and job level with their experience bars, carried weight,
    /// Zeny, and the class icon. Subscribes to each source's own change
    /// event and additionally does a full sync in <see cref="Start"/>,
    /// since some sources (health, mana) may finish their own <c>Awake</c>
    /// after this component's <c>OnEnable</c> runs.
    /// </summary>
    public class PlayerHudUI : MonoBehaviour
    {
        [SerializeField] private PlayerNameProvider nameProvider;
        [SerializeField] private HealthComponent health;
        [SerializeField] private PlayerExperience experience;
        [SerializeField] private PlayerJobProgress jobProgress;
        [SerializeField] private PlayerInventory inventory;
        [SerializeField] private PlayerCurrency currency;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private ClassIconLookup classIconLookup;

        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;
        [SerializeField] private TMP_Text baseLevelText;
        [SerializeField] private Slider baseExpSlider;
        [SerializeField] private TMP_Text jobLevelText;
        [SerializeField] private Slider jobExpSlider;
        [SerializeField] private TMP_Text weightText;
        [SerializeField] private TMP_Text zenyText;
        [SerializeField] private Image classIcon;
        [SerializeField] private TMP_Text classNameText;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Start()
        {
            RefreshAll();
        }

        private void Subscribe()
        {
            if (health != null)
            {
                health.HealthChanged -= UpdateHealth;
                health.HealthChanged += UpdateHealth;
            }

            if (experience != null)
            {
                experience.ExperienceChanged -= UpdateBaseExperience;
                experience.ExperienceChanged += UpdateBaseExperience;
                experience.LeveledUp -= UpdateBaseLevel;
                experience.LeveledUp += UpdateBaseLevel;
            }

            if (jobProgress != null)
            {
                jobProgress.JobExperienceChanged -= UpdateJobExperience;
                jobProgress.JobExperienceChanged += UpdateJobExperience;
                jobProgress.JobLeveledUp -= UpdateJobLevel;
                jobProgress.JobLeveledUp += UpdateJobLevel;
            }

            if (inventory != null && inventory.Items != null)
            {
                inventory.Items.InventoryChanged -= UpdateWeight;
                inventory.Items.InventoryChanged += UpdateWeight;
            }

            if (currency != null)
            {
                currency.ZenyChanged -= UpdateZeny;
                currency.ZenyChanged += UpdateZeny;
            }

            if (classController != null)
            {
                classController.ClassChanged -= UpdateClassIcon;
                classController.ClassChanged += UpdateClassIcon;
            }

            if (nameProvider != null)
            {
                nameProvider.NameChanged -= UpdateName;
                nameProvider.NameChanged += UpdateName;
            }
        }

        private void Unsubscribe()
        {
            if (health != null)
            {
                health.HealthChanged -= UpdateHealth;
            }

            if (experience != null)
            {
                experience.ExperienceChanged -= UpdateBaseExperience;
                experience.LeveledUp -= UpdateBaseLevel;
            }

            if (jobProgress != null)
            {
                jobProgress.JobExperienceChanged -= UpdateJobExperience;
                jobProgress.JobLeveledUp -= UpdateJobLevel;
            }

            if (inventory != null && inventory.Items != null)
            {
                inventory.Items.InventoryChanged -= UpdateWeight;
            }

            if (currency != null)
            {
                currency.ZenyChanged -= UpdateZeny;
            }

            if (classController != null)
            {
                classController.ClassChanged -= UpdateClassIcon;
            }

            if (nameProvider != null)
            {
                nameProvider.NameChanged -= UpdateName;
            }
        }

        private void RefreshAll()
        {
            if (nameProvider != null)
            {
                UpdateName(nameProvider.PlayerName);
            }

            if (health != null)
            {
                UpdateHealth(health.CurrentHealth, health.MaxHealth);
            }

            if (experience != null)
            {
                UpdateBaseLevel(experience.CurrentLevel);
                UpdateBaseExperience(experience.CurrentExperience, experience.RequiredExperienceForNextLevel);
            }

            if (jobProgress != null)
            {
                UpdateJobLevel(jobProgress.JobLevel);
                UpdateJobExperience(jobProgress.CurrentExperience, jobProgress.RequiredExperienceForNextLevel);
            }

            if (inventory != null && inventory.Items != null)
            {
                UpdateWeight();
            }

            if (currency != null)
            {
                UpdateZeny(currency.CurrentZeny);
            }

            if (classController != null)
            {
                UpdateClassIcon(classController.CurrentClass);
            }
        }

        private void UpdateName(string playerName)
        {
            if (nameText != null)
            {
                nameText.text = playerName;
            }
        }

        private void UpdateHealth(int current, int max)
        {
            if (hpText != null)
            {
                hpText.text = $"{current} / {max}";
            }

            if (hpSlider != null)
            {
                hpSlider.value = max > 0 ? (float)current / max : 0f;
            }
        }

        private void UpdateBaseLevel(int level)
        {
            if (baseLevelText != null)
            {
                baseLevelText.text = $"Base Lv. {level}";
            }
        }

        private void UpdateBaseExperience(int current, int required)
        {
            if (baseExpSlider != null)
            {
                baseExpSlider.value = required > 0 ? (float)current / required : 0f;
            }
        }

        private void UpdateJobLevel(int level)
        {
            if (jobLevelText != null)
            {
                jobLevelText.text = $"Job Lv. {level}";
            }
        }

        private void UpdateJobExperience(int current, int required)
        {
            if (jobExpSlider != null)
            {
                jobExpSlider.value = required > 0 ? (float)current / required : 0f;
            }
        }

        private void UpdateWeight()
        {
            if (weightText != null)
            {
                weightText.text = $"{inventory.Items.CurrentWeight:F0} / {inventory.Items.MaxCarryWeight:F0}";
            }
        }

        private void UpdateZeny(int amount)
        {
            if (zenyText != null)
            {
                zenyText.text = $"{amount:N0} Zeny";
            }
        }

        private void UpdateClassIcon(CharacterClass characterClass)
        {
            if (classNameText != null)
            {
                classNameText.text = characterClass.ToString();
            }

            if (classIcon == null || classIconLookup == null)
            {
                return;
            }

            var sprite = classIconLookup.GetIcon(characterClass);
            classIcon.sprite = sprite;
            classIcon.enabled = sprite != null;
        }
    }
}