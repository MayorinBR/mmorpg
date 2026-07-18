using UnityEngine;
using TMPro;
using Project.Character.Combat;
using Project.Character.Stats;

namespace Project.UI
{
    /// <summary>
    /// Orchestrates the six stat rows and the available-points display,
    /// applying increases through <see cref="PlayerStatsController.BaseStats"/>.
    /// Also refreshes automatically on level up, since that grants new points.
    /// </summary>
    public class StatAllocationUI : MonoBehaviour
    {
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private PlayerExperience experience;
        [SerializeField] private Items.EquipmentManager equipment;
        [SerializeField] private TMP_Text availablePointsText;
        [SerializeField] private StatRowUI[] statRows;

        private void OnEnable()
        {
            foreach (var row in statRows)
            {
                row.IncreaseRequested += HandleIncreaseRequested;
            }

            if (experience != null)
            {
                experience.LeveledUp += HandleLeveledUp;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged += RefreshAll;
            }
        }

        private void OnDisable()
        {
            foreach (var row in statRows)
            {
                row.IncreaseRequested -= HandleIncreaseRequested;
            }

            if (experience != null)
            {
                experience.LeveledUp -= HandleLeveledUp;
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged -= RefreshAll;
            }
        }

        private void Start()
        {
            RefreshAll();
        }

        private void HandleIncreaseRequested(StatType statType)
        {
            statsController.BaseStats.TryIncreaseStat(statType);
            RefreshAll();
        }

        private void HandleLeveledUp(int newLevel)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            var availablePoints = statsController.BaseStats.AvailablePoints;
            availablePointsText.text = $"Available Points: {availablePoints}";

            foreach (var row in statRows)
            {
                var baseValue = statsController.BaseStats.GetValue(row.StatType);
                var equipmentBonus = equipment != null ? equipment.GetBonus(row.StatType) : 0;
                row.SetValue(baseValue, equipmentBonus);
                row.SetInteractable(availablePoints > 0);
            }
        }
    }
}