using System;

namespace Project.Character.Combat
{
    /// <summary>
    /// One entry of a choice menu shown to the player.
    /// </summary>
    public readonly struct ChoiceOption
    {
        /// <summary>Initializes the option.</summary>
        /// <param name="label">The text shown on the option.</param>
        /// <param name="onSelected">Invoked when the player picks the option.</param>
        public ChoiceOption(string label, Action onSelected)
        {
            Label = label;
            OnSelected = onSelected;
        }

        /// <summary>Gets the text shown on the option.</summary>
        public string Label { get; }

        /// <summary>Gets the action invoked when the player picks the option.</summary>
        public Action OnSelected { get; }
    }
}
