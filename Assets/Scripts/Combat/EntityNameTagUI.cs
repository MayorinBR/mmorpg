using TMPro;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Displays a name on a world-space label — above an enemy's health bar,
    /// or above the player's own HUD elements — optionally prefixed with a
    /// level (e.g. "Lv.3 Poring"). The level prefix is opt-in so the same
    /// component serves both a monster's "Lv.X Name" tag and the player's
    /// plain name tag without a separate variant.
    /// </summary>
    public class EntityNameTagUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        [Tooltip("The name to display — a monster's species name (e.g. \"Poring\") or the player's character name. Can also be set at runtime via SetName.")]
        [SerializeField] private string entityName;

        [Tooltip("If true, the label is prefixed with \"Lv.{level} \". Leave off for a tag that never shows a level, such as the player's own name tag. A monster's level is normally set at runtime via SetLevel rather than authored here.")]
        [SerializeField] private bool showLevel;

        private int level;

        private void Awake()
        {
            Refresh();
        }

        /// <summary>Sets the displayed name and refreshes the label.</summary>
        /// <param name="newName">The new name to display.</param>
        public void SetName(string newName)
        {
            entityName = newName;
            Refresh();
        }

        /// <summary>
        /// Sets the level shown in the "Lv.{level}" prefix, then refreshes
        /// the label. Intended for a monster instance whose level is rolled
        /// at runtime (see Project.AI.MonsterLevelController). Does not
        /// change whether the prefix is shown — that stays under
        /// <see cref="showLevel"/>'s own control, so the tag can be
        /// configured to track a rolled level without ever displaying it.
        /// </summary>
        /// <param name="newLevel">The level to show.</param>
        public void SetLevel(int newLevel)
        {
            level = newLevel;
            Refresh();
        }

        private void Refresh()
        {
            if (label != null)
            {
                label.text = showLevel ? $"Lv.{level} {entityName}" : entityName;
            }
        }
    }
}
