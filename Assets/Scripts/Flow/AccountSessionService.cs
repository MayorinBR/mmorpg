namespace Project.Flow
{
    /// <summary>
    /// Static carrier for the account signed in on the Login screen, read by
    /// Character Selection and Character Creation to know which account's
    /// characters to list or add to. Unlike <see cref="Project.Persistence.GameSessionService"/>'s
    /// one-shot pending state, this is set once at login and stays available
    /// across every menu scene until a new login overwrites it — there is no
    /// explicit logout in this prototype.
    /// </summary>
    public static class AccountSessionService
    {
        /// <summary>Gets the login name of the currently signed-in account, or null if no one is signed in.</summary>
        public static string CurrentAccountLogin { get; private set; }

        /// <summary>Records the account that just signed in.</summary>
        /// <param name="login">The signed-in account's login name.</param>
        public static void SetCurrentAccount(string login)
        {
            CurrentAccountLogin = login;
        }
    }
}
