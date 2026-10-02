namespace Project.Flow
{
    /// <summary>
    /// One-shot carrier that lets Character Creation tell Character
    /// Selection which character to select when it opens, since a scene load
    /// cannot pass parameters. Consumed once, so a later visit to the
    /// selection screen starts with nothing selected.
    /// </summary>
    public static class CharacterSelectionHandoff
    {
        private static string pendingCharacterName;

        /// <summary>
        /// Records the character that Character Selection should select next.
        /// </summary>
        /// <param name="characterName">The character's name.</param>
        public static void QueueCharacterToSelect(string characterName)
        {
            pendingCharacterName = characterName;
        }

        /// <summary>
        /// Returns and clears the queued character name.
        /// </summary>
        /// <returns>The queued name, or null if nothing was queued.</returns>
        public static string ConsumeCharacterToSelect()
        {
            var characterName = pendingCharacterName;
            pendingCharacterName = null;
            return characterName;
        }
    }
}
