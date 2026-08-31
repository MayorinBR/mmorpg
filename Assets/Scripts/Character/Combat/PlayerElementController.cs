using System;
using UnityEngine;
using Project.Combat;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks the Mage's currently selected attack element. Standalone and
    /// purely informational today — no resistance system exists yet to
    /// make the choice matter mechanically (see FUTURE_IMPROVEMENTS.md),
    /// but the selection is tracked and exposed now so combat code and UI
    /// can start reading it.
    /// </summary>
    public class PlayerElementController : MonoBehaviour, ISaveParticipant
    {
        private static readonly Element[] AllElements = (Element[])Enum.GetValues(typeof(Element));

        [SerializeField] private Element startingElement = Element.Fire;

        /// <summary>Raised whenever the selected element changes.</summary>
        public event Action<Element> ElementChanged;

        /// <summary>Gets the currently selected element.</summary>
        public Element CurrentElement { get; private set; }

        private void Awake()
        {
            CurrentElement = startingElement;
        }

        /// <summary>
        /// Sets the currently selected element directly.
        /// </summary>
        /// <param name="element">The element to select.</param>
        public void SetElement(Element element)
        {
            if (element == CurrentElement)
            {
                return;
            }

            CurrentElement = element;
            ElementChanged?.Invoke(CurrentElement);
        }

        /// <summary>Cycles to the next element in declaration order, wrapping around.</summary>
        public void CycleElement()
        {
            var nextIndex = (Array.IndexOf(AllElements, CurrentElement) + 1) % AllElements.Length;
            SetElement(AllElements[nextIndex]);
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.mageElementIndex = (int)CurrentElement;
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            SetElement((Element)data.mageElementIndex);
        }
    }
}
