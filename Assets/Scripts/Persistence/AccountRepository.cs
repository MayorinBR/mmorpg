using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

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

        /// <summary>Loads the given account's data from disk.</summary>
        /// <param name="login">The account's login name.</param>
        public static AccountData Load(string login)
        {
            return LoadFromFile(SaveFilePath(login));
        }

        private static AccountData LoadFromFile(string path)
        {
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<AccountData>(json);
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
