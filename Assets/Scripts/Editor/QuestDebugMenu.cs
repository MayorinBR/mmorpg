using UnityEditor;
using UnityEngine;
using Project.Quests;

namespace Project.EditorTools
{
    /// <summary>
    /// Editor-only Play Mode command that clears every active and
    /// completed quest, mirroring <see cref="PlayerStatsDebugMenu"/>'s
    /// reasoning: kept out of the shipped UI and out of
    /// <c>Project.DebugTools</c> entirely, since <c>Project.EditorTools</c>'s
    /// asmdef restricts <c>includePlatforms</c> to <c>Editor</c> — the whole
    /// assembly is excluded from every player build.
    /// </summary>
    public static class QuestDebugMenu
    {
        private const string ResetQuestsMenuPath = "Tools/Debug/Reset Quests (Play Mode)";

        [MenuItem(ResetQuestsMenuPath)]
        private static void ResetQuests()
        {
            var questManager = Object.FindFirstObjectByType<QuestManager>();

            if (questManager == null)
            {
                Debug.LogWarning("QuestDebugMenu: no QuestManager found in the current scene.");
                return;
            }

            var cleared = questManager.ResetQuests();
            Debug.Log($"QuestDebugMenu: {cleared} quest(s) cleared. Note this does not refresh an already-open Quest Log or Quest Board panel — close and reopen it to see the updated list.");
        }

        [MenuItem(ResetQuestsMenuPath, true)]
        private static bool ValidateResetQuests()
        {
            return Application.isPlaying;
        }
    }
}
