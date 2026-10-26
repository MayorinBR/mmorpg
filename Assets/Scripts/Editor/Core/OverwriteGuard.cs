using UnityEditor;

namespace Project.EditorTools
{
    /// <summary>Asks for confirmation before a generator replaces content that may contain manual edits.</summary>
    public static class OverwriteGuard
    {
        private const string Title = "Overwrite existing content";
        private const string Ok = "Overwrite";
        private const string Cancel = "Cancel";

        /// <summary>Shows a confirmation dialog whose default answer keeps the existing content.</summary>
        /// <param name="target">Human readable description of what would be replaced.</param>
        /// <returns>True when the user explicitly accepted the overwrite.</returns>
        public static bool Confirm(string target)
        {
            return EditorUtility.DisplayDialog(Title, $"{target} will be regenerated and manual edits will be lost.", Ok, Cancel);
        }
    }
}
