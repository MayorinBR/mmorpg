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
        /// Updates the displayed value.
        /// </summary>
        /// <param name="value">The stat's current value.</param>
        public void SetValue(int value)
        {
            valueText.text = value.ToString();
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