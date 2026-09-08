using UnityEditor;
using UnityEngine;
using Project.Character.Combat;

namespace Project.EditorTools
{
    /// <summary>
    /// Editor-only Play Mode command that respecs the player's stats via
    /// <see cref="PlayerStatsController.ResetStats"/>. Deliberately kept out
    /// of the shipped UI: unlike a debug component under
    /// <c>Project.DebugTools</c> (which still compiles into a player build
    /// and could be forgotten on a scene object, see FUTURE_IMPROVEMENTS.md),
    /// this lives in <c>Project.EditorTools</c>, whose asmdef restricts
    /// <c>includePlatforms</c> to <c>Editor</c> — the whole assembly is
    /// excluded from every player build, so there is no scene wiring and no
    /// risk of a reset option reaching players. <see cref="PlayerStatsController.ResetStats"/>
    /// itself is left in place as a normal public method; only its exposure
    /// through gameplay UI was removed.
    /// </summary>
    public static class PlayerStatsDebugMenu
    {
        private const string MenuPath = "Tools/Debug/Reset Player Stats (Play Mode)";

        [MenuItem(MenuPath)]
        private static void ResetPlayerStats()
        {
            var statsController = Object.FindFirstObjectByType<PlayerStatsController>();

            if (statsController == null)
            {
                Debug.LogWarning("PlayerStatsDebugMenu: no PlayerStatsController found in the current scene.");
                return;
            }

            statsController.ResetStats();
        }

        [MenuItem(MenuPath, true)]
        private static bool ValidateResetPlayerStats()
        {
            return Application.isPlaying;
        }
    }
}
