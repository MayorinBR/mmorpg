namespace Project.UI
{
    /// <summary>
    /// Hides every shared tooltip at once. Tooltips are shown from pointer-enter
    /// events, and Unity sends no pointer-exit when a hover source is deactivated,
    /// so anything that hides a hover source (such as closing a window) must hide
    /// the tooltips explicitly.
    /// </summary>
    public static class TooltipHub
    {
        /// <summary>Hides the item, skill, effect and map tooltips if they exist in the scene.</summary>
        public static void HideAll()
        {
            if (ItemTooltipUI.Instance != null)
            {
                ItemTooltipUI.Instance.Hide();
            }

            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Hide();
            }

            if (EffectTooltipUI.Instance != null)
            {
                EffectTooltipUI.Instance.Hide();
            }

            if (MapTooltipUI.Instance != null)
            {
                MapTooltipUI.Instance.Hide();
            }
        }
    }
}
