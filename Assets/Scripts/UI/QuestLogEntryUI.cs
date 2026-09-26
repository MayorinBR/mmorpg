using TMPro;
using UnityEngine;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// One row in the Quest Log window: a quest's title and the progress
    /// line for each of its requirements. Built entirely from code via
    /// <see cref="Create"/> rather than a hand-authored prefab, the same
    /// approach <c>DamagePopup</c> uses and for the same reason — a single
    /// <see cref="TextMeshProUGUI"/> is all a row needs, so there is nothing
    /// a prefab would save.
    /// </summary>
    public class QuestLogEntryUI : MonoBehaviour
    {
        private const float LineHeight = 20f;
        private const float VerticalPadding = 6f;

        private TMP_Text bodyText;

        /// <summary>
        /// Creates one row under <paramref name="parent"/> showing
        /// <paramref name="quest"/>'s title and current per-requirement
        /// progress. The row stretches to the parent's width and sizes its
        /// own height to the number of lines it needs.
        /// </summary>
        /// <param name="parent">The Quest Log's content root.</param>
        /// <param name="quest">The active quest this row displays.</param>
        public static QuestLogEntryUI Create(Transform parent, ActiveQuest quest)
        {
            var requirements = quest.Definition.Requirements;
            var lineCount = 1 + requirements.Count;

            var go = new GameObject("QuestLogEntry", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, lineCount * LineHeight + VerticalPadding);

            var entry = go.AddComponent<QuestLogEntryUI>();
            entry.bodyText = go.AddComponent<TextMeshProUGUI>();
            entry.bodyText.fontSize = 14;
            entry.bodyText.color = Color.white;
            entry.bodyText.enableWordWrapping = false;

            entry.Refresh(quest);
            return entry;
        }

        /// <summary>Redraws the title and per-requirement progress text from the given quest's current state.</summary>
        /// <param name="quest">The active quest this row displays.</param>
        public void Refresh(ActiveQuest quest)
        {
            var requirements = quest.Definition.Requirements;
            var lines = new string[1 + requirements.Count];
            lines[0] = quest.Definition.Title;

            for (var i = 0; i < requirements.Count; i++)
            {
                lines[i + 1] = requirements[i].GetProgressText(quest.GetProgress(i));
            }

            bodyText.text = string.Join("\n", lines);
        }
    }
}
