using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Project.Character.Stats;

namespace Project.Flow
{
    /// <summary>
    /// One of the 6 character slots on the Character Selection screen. Shows
    /// a character's name, level and class when one occupies this slot, or
    /// an empty placeholder otherwise — <see cref="CharacterSelectionController"/>
    /// decides which (if any) character each slot shows as the account's
    /// character list is paged and filtered.
    /// </summary>
    public class CharacterSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private GameObject selectedHighlight;

        /// <summary>Gets the underlying button, for the controller to subscribe to.</summary>
        public Button Button => button;

        /// <summary>Gets the character name currently shown, or null if this slot is empty.</summary>
        public string CharacterName { get; private set; }

        /// <summary>
        /// Shows a character's summary and marks this slot as occupied.
        /// </summary>
        /// <param name="characterName">The character's name.</param>
        /// <param name="level">The character's current Base Level.</param>
        /// <param name="characterClass">The character's class.</param>
        public void SetCharacter(string characterName, int level, CharacterClass characterClass)
        {
            CharacterName = characterName;
            button.interactable = true;
            nameText.text = characterName;
            infoText.text = $"Lv. {level}  {characterClass}";
        }

        /// <summary>Clears this slot back to its empty placeholder state.</summary>
        public void SetEmpty()
        {
            CharacterName = null;
            button.interactable = false;
            nameText.text = "Empty";
            infoText.text = "";
            SetSelected(false);
        }

        /// <summary>
        /// Shows or hides this slot's selected-state highlight.
        /// </summary>
        /// <param name="selected">Whether this slot is the currently selected one.</param>
        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(selected);
            }
        }
    }
}
