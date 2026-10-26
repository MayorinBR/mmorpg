using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// A single shared tooltip for the active effects bar. Builds its own
    /// panel at startup and shows an effect's name, category, description,
    /// detail lines and live remaining time near the pointer.
    /// </summary>
    public class EffectTooltipUI : MonoBehaviour
    {
        private const float PanelWidth = 240f;
        private const float Padding = 10f;
        private static readonly Vector2 OffsetFromPointer = new Vector2(12f, 12f);

        private RectTransform panel;
        private CanvasGroup group;
        private TMP_Text text;
        private Canvas parentCanvas;

        /// <summary>Gets the active tooltip instance in the scene.</summary>
        public static EffectTooltipUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
            parentCanvas = GetComponentInParent<Canvas>();
            BuildPanel();
            Hide();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Shows or refreshes the tooltip for an effect.
        /// </summary>
        /// <param name="effect">The effect to describe.</param>
        /// <param name="screenPosition">The pointer position in screen space.</param>
        public void Show(ActiveEffect effect, Vector2 screenPosition)
        {
            if (panel == null)
            {
                return;
            }

            text.text = BuildText(effect);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);
            panel.position = ClampToScreen(screenPosition + OffsetFromPointer);
            group.alpha = 1f;
        }

        /// <summary>Hides the tooltip.</summary>
        public void Hide()
        {
            if (group != null)
            {
                group.alpha = 0f;
            }
        }

        private static string BuildText(ActiveEffect effect)
        {
            var descriptor = effect.Descriptor;
            var builder = new StringBuilder();
            builder.Append("<b>").Append(descriptor.DisplayName).Append("</b>\n");
            builder.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(ActiveEffectSlotUI.CategoryColor(descriptor.Category)));
            builder.Append(">").Append(descriptor.Category).Append("</color>\n");

            if (!string.IsNullOrEmpty(descriptor.Description))
            {
                builder.Append(descriptor.Description).Append('\n');
            }

            if (!string.IsNullOrEmpty(effect.Detail))
            {
                builder.Append("<color=#BFD7FF>").Append(effect.Detail).Append("</color>\n");
            }

            if (effect.HasTimer)
            {
                builder.Append("Remaining: ").Append(ActiveEffectRules.FormatRemaining(effect.RemainingSeconds));
            }
            else
            {
                builder.Append("Lasts until removed");
            }

            return builder.ToString();
        }

        private void BuildPanel()
        {
            var go = new GameObject("EffectTooltipPanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            go.transform.SetParent(transform, false);
            panel = (RectTransform)go.transform;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.zero;
            panel.pivot = Vector2.zero;
            panel.sizeDelta = new Vector2(PanelWidth, 10f);

            group = go.GetComponent<CanvasGroup>();
            group.blocksRaycasts = false;
            group.interactable = false;

            go.GetComponent<Image>().color = new Color(UiTheme.Ink.r, UiTheme.Ink.g, UiTheme.Ink.b, 0.95f);
            go.GetComponent<Image>().raycastTarget = false;

            var layout = go.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset((int)Padding, (int)Padding, (int)Padding, (int)Padding);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            go.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var label = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(panel, false);
            text = label.GetComponent<TextMeshProUGUI>();
            text.fontSize = 14f;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
        }

        private Vector2 ClampToScreen(Vector2 desired)
        {
            var scale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            var size = panel.rect.size * scale;
            desired.x = Mathf.Clamp(desired.x, 0f, Mathf.Max(0f, Screen.width - size.x));
            desired.y = Mathf.Clamp(desired.y, 0f, Mathf.Max(0f, Screen.height - size.y));
            return desired;
        }
    }
}
