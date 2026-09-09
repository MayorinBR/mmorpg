using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Project.Persistence
{
    /// <summary>
    /// Resolves the on-disk save file path for a given character name and
    /// checks whether it already exists, without needing a loaded gameplay
    /// scene or a <see cref="PlayerSaveController"/> instance — used by the
    /// Character Selection screen (a separate scene entirely) to validate a
    /// typed name before creating or continuing a character.
    /// <see cref="PlayerSaveController"/> resolves the exact same path for
    /// whichever character it's told to load, so the two can never disagree
    /// about where a given character's save lives.
    /// </summary>
    public static class CharacterSaveLookup
    {
        private const string SavesFolderName = "Saves";

        /// <summary>
        /// The full path a character's save file lives at (whether or not
        /// it exists yet), under a dedicated "Saves" folder inside
        /// <see cref="Application.persistentDataPath"/> — kept separate
        /// from <see cref="PlayerSaveController"/>'s own single fixed
        /// fallback save file, so neither path can ever collide with the
        /// other.
        /// </summary>
        /// <param name="characterName">The character's name, exactly as typed.</param>
        public static string SaveFilePath(string characterName)
        {
            var fileName = Sanitize(characterName) + ".json";
            return Path.Combine(Application.persistentDataPath, SavesFolderName, fileName);
        }

        /// <summary>
        /// Whether a save file already exists for this character name.
        /// False for a null, empty, or whitespace-only name rather than
        /// throwing, so callers can check a raw text field's contents
        /// directly.
        /// </summary>
        /// <param name="characterName">The character name to check.</param>
        public static bool Exists(string characterName)
        {
            return !string.IsNullOrWhiteSpace(characterName) && File.Exists(SaveFilePath(characterName));
        }

        /// <summary>
        /// Replaces every character invalid in a file name (a player can
        /// type anything into the name field) with an underscore, so an odd
        /// name can never fail <see cref="Path.Combine(string, string)"/> or
        /// produce a path outside the Saves folder.
        /// </summary>
        private static string Sanitize(string characterName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var buffer = new StringBuilder(characterName.Length);

            foreach (var character in characterName)
            {
                buffer.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
            }

            return buffer.ToString();
        }
    }
}
