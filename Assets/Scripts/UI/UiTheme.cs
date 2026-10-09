using UnityEngine;

namespace Project.UI
{
    /// <summary>
    /// Shared text and accent colors for runtime-built UI, matching the
    /// light panels used by the in-game windows so dynamically created
    /// text stays readable against them.
    /// </summary>
    public static class UiTheme
    {
        /// <summary>Primary text color on light panels.</summary>
        public static readonly Color Ink = new Color(0.12f, 0.16f, 0.27f);

        /// <summary>Secondary, less prominent text color on light panels.</summary>
        public static readonly Color Muted = new Color(0.35f, 0.40f, 0.52f);

        /// <summary>Accent fill used for primary action buttons.</summary>
        public static readonly Color Accent = new Color(0.25f, 0.45f, 0.75f);

        /// <summary>Warning text color for minor overweight on light panels.</summary>
        public static readonly Color Warning = new Color(0.85f, 0.45f, 0f);

        /// <summary>Danger text color for major overweight on light panels.</summary>
        public static readonly Color Danger = new Color(0.78f, 0.12f, 0.12f);
    }
}
