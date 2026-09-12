using UnityEditor;
using UnityEngine;
using Project.Character.Combat;
using Project.Items;

namespace Project.EditorTools
{
    /// <summary>
    /// Editor-only Play Mode commands for resetting player state, e.g.
    /// <see cref="PlayerStatsController.ResetStats"/> alone, or a full
    /// reset of everything progression-related. Deliberately kept out of
    /// the shipped UI: unlike a debug component under
    /// <c>Project.DebugTools</c> (which still compiles into a player build
    /// and could be forgotten on a scene object, see FUTURE_IMPROVEMENTS.md),
    /// this lives in <c>Project.EditorTools</c>, whose asmdef restricts
    /// <c>includePlatforms</c> to <c>Editor</c> — the whole assembly is
    /// excluded from every player build, so there is no scene wiring and no
    /// risk of a reset option reaching players. Every reset method these
    /// commands call is left in place as a normal public method; only its
    /// exposure through gameplay UI was removed.
    /// </summary>
    public static class PlayerStatsDebugMenu
    {
        private const string ResetStatsMenuPath = "Tools/Debug/Reset Player Stats (Play Mode)";
        private const string ResetPlayerMenuPath = "Tools/Debug/Reset Player - Full (Play Mode)";

        [MenuItem(ResetStatsMenuPath)]
        private static void ResetPlayerStats()
        {
            var statsController = Object.FindFirstObjectByType<PlayerStatsController>();

            if (statsController == null)
            {
                Debug.LogWarning("PlayerStatsDebugMenu: no PlayerStatsController found in the current scene.");
                return;
            }

            var refunded = statsController.ResetStats();
            Debug.Log($"PlayerStatsDebugMenu: stats reset, {refunded} points refunded. Note this does not refresh an already-open Stat Allocation panel — close and reopen it, or spend/gain a point, to see the updated values.");
        }

        [MenuItem(ResetStatsMenuPath, true)]
        private static bool ValidateResetPlayerStats()
        {
            return Application.isPlaying;
        }

        /// <summary>
        /// Resets base level, job level, skill points, stat points,
        /// inventory, equipment and Zeny back to a fresh character's
        /// starting values. Job progress is reset before stats so the
        /// final stat refresh (see <see cref="PlayerStatsController.ResetStats"/>)
        /// no longer includes a Job Level bonus the reset job level no
        /// longer qualifies for. Equipment is cleared directly rather than
        /// unequipped item-by-item, since the inventory it would otherwise
        /// return items to is wiped in the same reset.
        /// </summary>
        [MenuItem(ResetPlayerMenuPath)]
        private static void ResetPlayerFull()
        {
            var statsController = Object.FindFirstObjectByType<PlayerStatsController>();

            if (statsController == null)
            {
                Debug.LogWarning("PlayerStatsDebugMenu: no PlayerStatsController found in the current scene.");
                return;
            }

            var experience = Object.FindFirstObjectByType<PlayerExperience>();
            var jobProgress = Object.FindFirstObjectByType<PlayerJobProgress>();
            var skillBook = Object.FindFirstObjectByType<PlayerSkillBook>();
            var currency = Object.FindFirstObjectByType<PlayerCurrency>();
            var inventory = Object.FindFirstObjectByType<PlayerInventory>();
            var equipment = Object.FindFirstObjectByType<EquipmentManager>();

            jobProgress?.ResetProgress();
            skillBook?.ResetLearnedSkills();
            currency?.ResetToStarting();
            equipment?.ClearAllEquipment();
            inventory?.Items.Clear();
            experience?.ResetProgress();
            statsController.ResetStats();

            Debug.Log("PlayerStatsDebugMenu: player fully reset (level, job level, skill points, stat points, inventory, equipment, Zeny). Note this does not refresh already-open UI panels — close and reopen them to see the updated values.");
        }

        [MenuItem(ResetPlayerMenuPath, true)]
        private static bool ValidateResetPlayerFull()
        {
            return Application.isPlaying;
        }
    }
}
