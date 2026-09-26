using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Project.Quests;

namespace Project.UI
{
    /// <summary>
    /// One row in the Quest Board window: a posted quest's title and
    /// description, with an Accept button. Built entirely from code via
    /// <see cref="Create"/> rather than a hand-authored prefab, the same
    /// approach <see cref="QuestLogEntryUI"/> uses — nothing here needs
    /// anything a prefab would provide over a few <c>AddComponent</c> calls.
    /// </summary>
    public class QuestBoardEntryUI : MonoBehaviour
    {
        private const float TitleHeight = 22f;
        private const float DescriptionHeight = 50f;
        private const float ButtonHeight = 24f;
        private const float ButtonWidth = 90f;
        private const float VerticalPadding = 10f;

        private QuestDefinition quest;
        private Action<QuestDefinition> onAccept;

        /// <summary>
        /// Creates one row under <paramref name="parent"/> showing
        /// <paramref name="quest"/>'s title and description, with an Accept
        /// button that invokes <paramref name="acceptCallback"/> when clicked.
        /// </summary>
        /// <param name="parent">The Quest Board's content root.</param>
        /// <param name="quest">The posted quest this row displays.</param>
        /// <param name="acceptCallback">Invoked with this row's quest when its Accept button is clicked.</param>
        public static QuestBoardEntryUI Create(Transform parent, QuestDefinition quest, Action<QuestDefinition> acceptCallback)
        {
            var totalHeight = TitleHeight + DescriptionHeight + ButtonHeight + VerticalPadding;

            var go = new GameObject("QuestBoardEntry", typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(0, totalHeight);

            var entry = go.AddComponent<QuestBoardEntryUI>();
            entry.quest = quest;
            entry.onAccept = acceptCallback;

            var titleText = CreateStretchedText(rect, "Title", 0, TitleHeight, 14, FontStyles.Bold);
            titleText.text = quest.Title;

            var descriptionText = CreateStretchedText(rect, "Description", TitleHeight, DescriptionHeight, 12, FontStyles.Normal);
            descriptionText.color = new Color(0.85f, 0.85f, 0.85f);
            descriptionText.enableWordWrapping = true;
            descriptionText.text = quest.Description;

            var buttonGo = new GameObject("AcceptButton", typeof(RectTransform), typeof(Image), typeof(Button));
            var buttonRect = (RectTransform)buttonGo.transform;
            buttonRect.SetParent(rect, false);
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0, 1);
            buttonRect.anchoredPosition = new Vector2(0, -(TitleHeight + DescriptionHeight));
            buttonRect.sizeDelta = new Vector2(ButtonWidth, ButtonHeight);
            buttonGo.GetComponent<Image>().color = new Color(0.25f, 0.5f, 0.25f);

            var buttonTextRect = new GameObject("Text", typeof(RectTransform));
            var buttonTextTransform = (RectTransform)buttonTextRect.transform;
            buttonTextTransform.SetParent(buttonRect, false);
            buttonTextTransform.anchorMin = Vector2.zero;
            buttonTextTransform.anchorMax = Vector2.one;
            buttonTextTransform.sizeDelta = Vector2.zero;

            var buttonText = buttonTextRect.AddComponent<TextMeshProUGUI>();
            buttonText.text = "Accept";
            buttonText.fontSize = 12;
            buttonText.fontStyle = FontStyles.Bold;
            buttonText.color = Color.white;
            buttonText.alignment = TextAlignmentOptions.Center;

            buttonGo.GetComponent<Button>().onClick.AddListener(() => entry.onAccept?.Invoke(entry.quest));

            return entry;
        }

        private static TMP_Text CreateStretchedText(Transform parent, string name, float yOffset, float height, int fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(0, -yOffset);
            rect.sizeDelta = new Vector2(0, height);

            var text = go.AddComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            return text;
        }
    }
}
