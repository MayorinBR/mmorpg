using UnityEngine;
using UnityEngine.UI;
using Project.Character.Stats;

namespace Project.Flow
{
    /// <summary>
    /// One class option on the Character Selection screen: a
    /// <see cref="Button"/> that knows its own <see cref="CharacterClass"/>
    /// and shows a highlight while selected. One of these sits on each of
    /// the 6 class buttons in the Editor — each tagged with its own class in
    /// the Inspector, so <see cref="CharacterSelectionController"/> never
    /// has to assume array order matches <see cref="CharacterClass"/>'s
    /// declaration order.
    /// </summary>
    public class ClassSelectionButton : MonoBehaviour
    {
        [SerializeField] private CharacterClass characterClass;
        [SerializeField] private Button button;
        [SerializeField] private GameObject selectedHighlight;

        /// <summary>Gets the class this button represents.</summary>
        public CharacterClass CharacterClass => characterClass;

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
