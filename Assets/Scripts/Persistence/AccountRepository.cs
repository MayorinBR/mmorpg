using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Persistence
{
    /// <summary>
    /// Resolves the on-disk file for a given login and handles signing in
    /// against it, mirroring how <see cref="CharacterSaveLookup"/> resolves a
    /// character's save file. Used by the Login screen to sign in — creating
    /// the account the first time a login is used, since this is a local
    /// prototype with no real registration flow — and by Character
    /// Selection/Creation to read and update which characters belong to the
    /// signed-in account.
    /// </summary>
    public static class AccountRepository
    {
        private const string AccountsFolderName = "Accounts";

        /// <summary>
        /// The full path an account's file lives at (whether or not it
        /// exists yet), under a dedicated "Accounts" folder inside
        /// <see cref="Application.persistentDataPath"/>.
        /// </summary>
        /// <param name="login">The account's login name, exactly as typed.</param>
        public static string SaveFilePath(string login)
        {
            var fileName = Sanitize(login) + ".json";
            return Path.Combine(Application.persistentDataPath, AccountsFolderName, fileName);
        }

        /// <summary>Whether an account file already exists for this login.</summary>
        /// <param name="login">The login name to check.</param>
        public static bool Exists(string login)
        {
            return !string.IsNullOrWhiteSpace(login) && File.Exists(SaveFilePath(login));
        }

        /// <summary>
        /// Signs in with the given login and password: creates a brand-new
        /// account (with no characters yet) the first time a login is used,
        /// or validates the password against the existing account otherwise.
        /// </summary>
        /// <param name="login">The typed login name.</param>
        /// <param name="password">The typed password.</param>
        /// <param name="account">The signed-in account's data, or null on failure.</param>
        /// <param name="error">A user-facing message describing why sign-in failed, or null on success.</param>
        /// <returns>True if sign-in succeeded.</returns>
        public static bool TryLogin(string login, string password, out AccountData account, out string error)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                account = null;
                error = "Enter a login and password.";
                return false;
            }

            var path = SaveFilePath(login);
            var hash = Hash(password);

            if (!File.Exists(path))
            {
                account = new AccountData { login = login, passwordHash = hash };
                Save(account);
                error = null;
                return true;
            }

            account = LoadFromFile(path);

            if (account.passwordHash != hash)
            {
                account = null;
                error = "Incorrect password.";
                return false;
            }

            error = null;
            return true;
        }

        /// <summary>
        /// Appends a newly-created character to an account's character list
        /// and persists the change, so it survives a game restart.
        /// </summary>
        /// <param name="login">The owning account's login name.</param>
        /// <param name="characterName">The new character's name.</param>
        public static void AddCharacter(string login, string characterName)
        {
            var path = SaveFilePath(login);
            var account = File.Exists(path) ? LoadFromFile(path) : new AccountData { login = login };

            if (!account.characterNames.Contains(characterName))
            {
                account.characterNames.Add(characterName);
            }

            Save(account);
        }

        /// <summary>
        /// Adds a newly-created character to an account without starting it:
        /// records the name in the account's character list and keeps the
        /// chosen class and gender until the character's first save exists.
        /// </summary>
        /// <param name="login">The owning account's login name.</param>
        /// <param name="characterName">The new character's name.</param>
        /// <param name="characterClass">The class chosen for the character.</param>
        /// <param name="gender">The gender chosen for the character.</param>
        /// <param name="appearance">The look chosen for the character, or null to keep the model's default look.</param>
        public static void AddNewCharacter(string login, string characterName, CharacterClass characterClass, CharacterGender gender, CharacterAppearanceData appearance = null)
        {
            var path = SaveFilePath(login);
            var account = File.Exists(path) ? LoadFromFile(path) : new AccountData { login = login };

            if (!account.characterNames.Contains(characterName))
            {
                account.characterNames.Add(characterName);
            }

            account.pendingCharacters.RemoveAll(pending => pending.name == characterName);
            account.pendingCharacters.Add(new PendingCharacterData
            {
                name = characterName,
                characterClassIndex = (int)characterClass,
                characterGenderIndex = (int)gender,
                appearance = appearance ?? new CharacterAppearanceData()
            });

            Save(account);
        }

        /// <summary>
        /// Looks up the creation choices of a character that has not been
        /// played yet.
        /// </summary>
        /// <param name="login">The owning account's login name.</param>
        /// <param name="characterName">The character's name.</param>
        /// <param name="pending">The stored creation choices, or null if there are none.</param>
        /// <returns>True if the character is still waiting for its first save.</returns>
        public static bool TryGetPendingCharacter(string login, string characterName, out PendingCharacterData pending)
        {
            pending = null;

            if (!Exists(login))
            {
                return false;
            }

            pending = LoadFromFile(SaveFilePath(login)).pendingCharacters.Find(entry => entry.name == characterName);
            return pending != null;
        }

        /// <summary>
        /// Removes a character from an account, including its stored
        /// creation choices. The character's save file is not touched; see
        /// <see cref="CharacterSaveLookup.Delete"/>.
        /// </summary>
        /// <param name="login">The owning account's login name.</param>
        /// <param name="characterName">The name of the character to remove.</param>
        public static void RemoveCharacter(string login, string characterName)
        {
            if (!Exists(login))
            {
                return;
            }

            var account = LoadFromFile(SaveFilePath(login));
            account.characterNames.Remove(characterName);
            account.pendingCharacters.RemoveAll(pending => pending.name == characterName);
            Save(account);
        }

        /// <summary>
        /// Whether a character name is already in use, either by a saved
        /// character or by a character on any account that has not been
        /// played yet. Character names share one namespace because each name
        /// is also the key of a save file.
        /// </summary>
        /// <param name="characterName">The name to check.</param>
        public static bool IsCharacterNameTaken(string characterName)
        {
            if (CharacterSaveLookup.Exists(characterName))
            {
                return true;
            }

            var directory = Path.Combine(Application.persistentDataPath, AccountsFolderName);

            if (!Directory.Exists(directory))
            {
                return false;
            }

            foreach (var file in Directory.GetFiles(directory, "*.json"))
            {
                var account = LoadFromFile(file);

                if (account.characterNames.Exists(name => string.Equals(name, characterName, StringComparison.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>Loads the given account's data from disk.</summary>
        /// <param name="login">The account's login name.</param>
        public static AccountData Load(string login)
        {
            return LoadFromFile(SaveFilePath(login));
        }

        private static AccountData LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            var account = JsonUtility.FromJson<AccountData>(json);
            account.characterNames ??= new List<string>();
            account.pendingCharacters ??= new List<PendingCharacterData>();
            return account;
        }

        private static void Save(AccountData account)
        {
            var path = SaveFilePath(account.login);
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonUtility.ToJson(account, true));
        }

        private static string Hash(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var builder = new StringBuilder(bytes.Length * 2);

            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }

            return builder.ToString();
        }

        /// <summary>
        /// Replaces every character invalid in a file name with an
        /// underscore, matching <see cref="CharacterSaveLookup"/>'s own
        /// sanitization so a login can never produce a path outside the
        /// Accounts folder.
        /// </summary>
        private static string Sanitize(string login)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var buffer = new StringBuilder(login.Length);

            foreach (var character in login)
            {
                buffer.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
            }

            return buffer.ToString();
        }
    }
}
