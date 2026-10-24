using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// One square of the active effects bar: category colored border, icon
    /// (or a colored tile with initials when there is no sprite), a
    /// regressive timer bar along the bottom and a hover tooltip.
    /// </summary>
    public class ActiveEffectSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float BorderSize = 2f;
        private const float TimerBarHeight = 5f;
        private const float BlinkSpeed = 6f;

        private Image border;
        private Image icon;
        private TMP_Text initials;
        private RectTransform timerFill;
        private Image timerFillImage;
        private GameObject timerRoot;
        private ActiveEffect current;
        private bool hovered;

        /// <summary>
        /// Gets the border color used for a category, shared with the tooltip.
        /// </summary>
        /// <param name="category">The effect category.</param>
        /// <returns>The category color.</returns>
        public static Color CategoryColor(EffectCategory category)
        {
            switch (category)
            {
                case EffectCategory.Buff:
                    return new Color(0.35f, 0.75f, 1f);
                case EffectCategory.Debuff:
                    return new Color(1f, 0.55f, 0.15f);
                case EffectCategory.Status:
                    return new Color(0.9f, 0.25f, 0.25f);
                case EffectCategory.Toggle:
                    return new Color(0.75f, 0.55f, 1f);
                default:
                    return new Color(0.7f, 0.7f, 0.7f);
            }
        }

        /// <summary>
        /// Creates an empty slot under a parent.
        /// </summary>
        /// <param name="parent">The bar the slot belongs to.</param>
        /// <param name="size">The side of the square slot in pixels.</param>
        /// <returns>The new slot.</returns>
        public static ActiveEffectSlotUI Create(Transform parent, float size)
        {
            var go = new GameObject("EffectSlot", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var layout = go.GetComponent<LayoutElement>();
            layout.preferredWidth = size;
            layout.preferredHeight = size;

            var slot = go.AddComponent<ActiveEffectSlotUI>();
            slot.Build(go.GetComponent<Image>());
            return slot;
        }

        /// <summary>
        /// Shows an effect in this slot and refreshes the tooltip when hovered.
        /// </summary>
        /// <param name="effect">The effect to show.</param>
        public void Bind(ActiveEffect effect)
        {
            current = effect;
            var descriptor = effect.Descriptor;
            var categoryColor = CategoryColor(descriptor.Category);
            border.color = categoryColor;

            icon.sprite = descriptor.Icon;
            icon.color = descriptor.Icon != null ? Color.white : TileColor(descriptor.Id, categoryColor);
            initials.text = descriptor.Icon != null ? string.Empty : Initials(descriptor.DisplayName);

            var blinkAlpha = ActiveEffectRules.IsExpiring(effect) ? Mathf.Lerp(0.35f, 1f, Mathf.PingPong(Time.unscaledTime * BlinkSpeed, 1f)) : 1f;
            var iconColor = icon.color;
            iconColor.a = blinkAlpha;
            icon.color = iconColor;

            timerRoot.SetActive(effect.HasTimer);

            if (effect.HasTimer)
            {
                var fraction = effect.RemainingFraction;
                timerFill.anchorMax = new Vector2(fraction, 1f);
                timerFillImage.color = fraction > 0.5f
                    ? Color.Lerp(Color.yellow, Color.green, (fraction - 0.5f) * 2f)
                    : Color.Lerp(Color.red, Color.yellow, fraction * 2f);
            }

            if (hovered)
            {
                ShowTooltip();
            }
        }

        /// <inheritdoc />
        public void OnPointerEnter(PointerEventData eventData)
        {
            hovered = true;
            ShowTooltip();
        }

        /// <inheritdoc />
        public void OnPointerExit(PointerEventData eventData)
        {
            hovered = false;

            if (EffectTooltipUI.Instance != null)
            {
                EffectTooltipUI.Instance.Hide();
            }
        }

        private void OnDisable()
        {
            if (hovered && EffectTooltipUI.Instance != null)
            {
                EffectTooltipUI.Instance.Hide();
            }

            hovered = false;
        }

        private void ShowTooltip()
        {
            if (EffectTooltipUI.Instance != null && Mouse.current != null)
            {
                EffectTooltipUI.Instance.Show(current, Mouse.current.position.ReadValue());
            }
        }

        private void Build(Image borderImage)
        {
            border = borderImage;

            var iconObject = NewChild("Icon", typeof(Image));
            Stretch((RectTransform)iconObject.transform, BorderSize);
            icon = iconObject.GetComponent<Image>();
            icon.raycastTarget = false;

            var textObject = NewChild("Initials", typeof(TextMeshProUGUI));
            Stretch((RectTransform)textObject.transform, BorderSize);
            initials = textObject.GetComponent<TextMeshProUGUI>();
            initials.alignment = TextAlignmentOptions.Center;
            initials.fontSize = 13f;
            initials.fontStyle = FontStyles.Bold;
            initials.color = Color.white;
            initials.raycastTarget = false;

            timerRoot = NewChild("TimerBar", typeof(Image));
            var timerRect = (RectTransform)timerRoot.transform;
            timerRect.anchorMin = Vector2.zero;
            timerRect.anchorMax = new Vector2(1f, 0f);
            timerRect.pivot = new Vector2(0.5f, 0f);
            timerRect.offsetMin = new Vector2(BorderSize, BorderSize);
            timerRect.offsetMax = new Vector2(-BorderSize, BorderSize + TimerBarHeight);
            var background = timerRoot.GetComponent<Image>();
            background.color = new Color(0f, 0f, 0f, 0.75f);
            background.raycastTarget = false;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(timerRect, false);
            timerFill = (RectTransform)fill.transform;
            timerFill.anchorMin = Vector2.zero;
            timerFill.anchorMax = Vector2.one;
            timerFill.offsetMin = Vector2.zero;
            timerFill.offsetMax = Vector2.zero;
            timerFillImage = fill.GetComponent<Image>();
            timerFillImage.raycastTarget = false;
        }

        private GameObject NewChild(string childName, params System.Type[] components)
        {
            var types = new System.Type[components.Length + 1];
            types[0] = typeof(RectTransform);
            components.CopyTo(types, 1);
            var child = new GameObject(childName, types);
            child.transform.SetParent(transform, false);
            return child;
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(-inset, -inset);
        }

        private static string Initials(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
            {
                return string.Empty;
            }

            var parts = displayName.Split(' ');
            return parts.Length > 1 && parts[1].Length > 0
                ? string.Concat(char.ToUpperInvariant(parts[0][0]), char.ToUpperInvariant(parts[1][0]))
                : displayName.Substring(0, Mathf.Min(2, displayName.Length));
        }

        private static Color TileColor(string id, Color categoryColor)
        {
            var hash = id != null ? id.GetHashCode() : 0;
            var hue = (hash & 0xFFFF) / 65535f;
            var tile = Color.HSVToRGB(hue, 0.55f, 0.55f);
            return Color.Lerp(tile, categoryColor, 0.25f);
        }
    }
}
