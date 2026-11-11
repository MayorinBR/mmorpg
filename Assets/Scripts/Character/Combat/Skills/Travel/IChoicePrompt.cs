using System.Collections.Generic;

namespace Project.Character.Combat
{
    /// <summary>
    /// Asks the player to pick one option from a short menu.
    /// </summary>
    public interface IChoicePrompt
    {
        /// <summary>
        /// Shows the menu. Picking an option closes it and invokes that option's action.
        /// </summary>
        /// <param name="line">The question or description shown above the options.</param>
        /// <param name="options">The options to offer.</param>
        void Show(string line, IReadOnlyList<ChoiceOption> options);
    }
}
