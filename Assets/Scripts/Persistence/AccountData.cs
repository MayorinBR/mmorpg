using System;
using System.Collections.Generic;

namespace Project.Persistence
{
    /// <summary>
    /// Local, single-player stand-in for a real account: a login/password
    /// pair and the characters created under it, persisted as its own JSON
    /// file by <see cref="AccountRepository"/>. Deliberately minimal — no
    /// tokens or server round-trip — until the project's planned
    /// authoritative-server migration replaces this with a real login flow.
    /// </summary>
    [Serializable]
    public class AccountData
    {
        /// <summary>The login name, exactly as typed on the Login screen.</summary>
        public string login = "";

        /// <summary>SHA-256 hash (hex) of the account's password. Never stored in plain text.</summary>
        public string passwordHash = "";

        /// <summary>
        /// Names of every character created under this account, in creation
        /// order — the order the Character Selection screen lists them in.
        /// Each name is also a key into <see cref="CharacterSaveLookup"/>,
        /// which owns that character's actual save data; this list only
        /// records which characters belong to this account.
        /// </summary>
        public List<string> characterNames = new List<string>();
    }
}
