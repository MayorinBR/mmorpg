using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Character.Stats;

namespace Project.UI
{
    /// <summary>
    /// Displays a single stat's current value and an increase button.
    /// Raises <see cref="IncreaseRequested"/> when clicked; the actual stat
    /// increase logic lives in <see cref="StatAllocationUI"/>, keeping this
    /// component purely presentational.
    /// </summary>
    public class StatRowUI : MonoBehaviour
    {
        [SerializeField] private StatType statType;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private Button increaseButton;

        /// <summary>Raised when the increase button is clicked, carrying which stat this row represents.</summary>
        public event Action<StatType> IncreaseRequested;

        /// <summary>Gets the stat this row displays.</summary>
        public StatType StatType => statType;

        private void Awake()
        {
            increaseButton.onClick.AddListener(() => IncreaseRequested?.Invoke(statType));
        }

        /// <summary>
        /// Updates the displayed value, always showing the equipment bonus
        /// in parentheses, even when it's zero (e.g. "5 (+3)" or "1 (+0)").
        /// </summary>
        public void SetValue(int baseValue, int equipmentBonus)
        {
            var sign = equipmentBonus >= 0 ? "+" : string.Empty;
            valueText.text = $"{baseValue} ({sign}{equipmentBonus})";
        }

        /// <summary>
        /// Enables or disables the increase button, typically based on
        /// whether stat points are available to spend.
        /// </summary>
        /// <param name="canIncrease">True to allow clicking the increase button.</param>
        public void SetInteractable(bool canIncrease)
        {
            increaseButton.interactable = canIncrease;
        }
    }
}