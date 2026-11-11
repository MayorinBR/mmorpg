using System.Collections.Generic;
using Project.Character.Combat;

namespace Project.UI
{
    /// <summary>
    /// Shows choice menus in the dialogue window, so skills can ask the player a question without
    /// depending on the UI.
    /// </summary>
    public sealed class DialogueChoicePrompt : IChoicePrompt
    {
        private readonly DialogueWindowUI window;

        /// <summary>Initializes the prompt.</summary>
        /// <param name="window">The dialogue window that displays the menu.</param>
        public DialogueChoicePrompt(DialogueWindowUI window)
        {
            this.window = window;
        }

        /// <inheritdoc />
        public void Show(string line, IReadOnlyList<ChoiceOption> options)
        {
            window.ShowChoices(line, options);
        }
    }
}
