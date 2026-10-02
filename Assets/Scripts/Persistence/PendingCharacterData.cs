using System;

namespace Project.Persistence
{
    /// <summary>
    /// Creation choices of a character that exists on an account but has not
    /// been played yet, so no save file has been written for it. Lets the
    /// Character Selection screen list the character, and lets Start begin
    /// it as a brand-new character, without starting the game at creation
    /// time. Stored as plain indices because it is serialized with
    /// <see cref="UnityEngine.JsonUtility"/> inside <see cref="AccountData"/>.
    /// </summary>
    [Serializable]
    public class PendingCharacterData
    {
        /// <summary>The character's name.</summary>
        public string name = "";

        /// <summary>The chosen class, as a <c>CharacterClass</c> index.</summary>
        public int characterClassIndex;

        /// <summary>The chosen gender, as a <c>CharacterGender</c> index.</summary>
        public int characterGenderIndex;
    }
}
