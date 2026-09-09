using UnityEngine;
using UnityEngine.UI;
using Project.Character.Stats;

namespace Project.Flow
{
    /// <summary>
    /// One gender option on the Character Selection screen — the same
    /// self-describing-button pattern as <see cref="ClassSelectionButton"/>,
    /// kept as its own small type rather than a shared generic component
    /// since <see cref="CharacterGender"/> and <see cref="Character.Stats.CharacterClass"/>
    /// are unrelated enums and Unity cannot serialize a generic MonoBehaviour field.
    /// </summary>
    public class GenderSelectionButton : MonoBehaviour
    {
        [SerializeField] private CharacterGender gender;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedHighlight;

        /// <summary>Gets the gender this button represents.</summary>
        public CharacterGender Gender => gender;

        /// <summary>Gets the underlying button, for the controller to subscribe to.</summary>
        public Button Button => button;

        /// <summary>
        /// Shows or hides this option's selected-state highlight.
        /// </summary>
        /// <param name="selected">Whether this option is the currently selected one.</param>
        public void SetSelected(bool selected)
        {
            if (selectedHighlight != null)
            {
                selectedHighlight.SetActive(selected);
            }
        }
    }
}
