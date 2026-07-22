using System.Collections.Generic;

namespace Project.UI
{
    /// <summary>
    /// Maps stable window IDs to their displayed title text. Kept as the
    /// single lookup point so introducing real localization later only
    /// requires changing this class, not every place that references a
    /// window by ID.
    /// </summary>
    public static class WindowTitleLookup
    {
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { "Inventory", "Inventory" },
            { "Stats", "Status" },
            { "Equipment", "Equipment" }
        };

        /// <summary>
        /// Gets the display name for a window ID, falling back to the ID
        /// itself if no mapping is registered.
        /// </summary>
        /// <param name="id">The window's stable ID.</param>
        /// <returns>The text to display in the window's title bar.</returns>
        public static string GetDisplayName(string id)
        {
            return DisplayNames.TryGetValue(id, out var displayName) ? displayName : id;
        }
    }
}