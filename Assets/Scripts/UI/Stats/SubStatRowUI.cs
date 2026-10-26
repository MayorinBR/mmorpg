using UnityEngine;
using TMPro;
using Project.Character.Stats;

namespace Project.UI
{
    /// <summary>
    /// Displays a single read-only sub-stat value (e.g. Atk, Def, Flee).
    /// Unlike <see cref="StatRowUI"/>, a sub-stat is a calculated value with
    /// no direct player interaction, so this only exposes a value setter —
    /// the calculation and refresh timing live in <see cref="SubStatsPanelUI"/>.
    /// </summary>
    public class SubStatRowUI : MonoBehaviour
    {
        [SerializeField] private SubStatType subStatType;
        [SerializeField] private TMP_Text valueText;

        /// <summary>Gets the sub-stat this row displays.</summary>
        public SubStatType SubStatType => subStatType;

        /// <summary>
        /// Updates the displayed value.
        /// </summary>
        /// <param name="value">The text to display, already formatted by the caller.</param>
        public void SetValue(string value)
        {
            valueText.text = value;
        }
    }
}
