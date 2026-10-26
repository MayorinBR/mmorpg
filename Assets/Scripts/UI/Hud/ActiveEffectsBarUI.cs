using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Character.Combat;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// A row above the skill hotbar listing every condition affecting the
    /// player. Slots are built at runtime and pooled. A small button at the
    /// start of the row shows or hides permanent passive skills; the choice
    /// is saved in PlayerPrefs.
    /// </summary>
    public class ActiveEffectsBarUI : MonoBehaviour
    {
        private const string ShowPassivesKey = "ActiveEffectsBar.ShowPassives";
        private const int MaxVisibleSlots = 16;
        private const float SlotSize = 34f;
        private const float Spacing = 4f;

        [Tooltip("Source of the active effects. Resolved from the scene when left empty.")]
        [SerializeField] private ActiveEffectsTracker tracker;

        private readonly List<ActiveEffect> effects = new List<ActiveEffect>();
        private readonly List<ActiveEffectSlotUI> slots = new List<ActiveEffectSlotUI>();
        private Image toggleImage;
        private TMP_Text overflowText;
        private bool showPassives;

        private void Awake()
        {
            showPassives = PlayerPrefs.GetInt(ShowPassivesKey, 0) == 1;
            BuildRow();
        }

        private void Update()
        {
            if (tracker == null)
            {
                tracker = FindFirstObjectByType<ActiveEffectsTracker>();

                if (tracker == null)
                {
                    return;
                }
            }

            tracker.Collect(effects, showPassives);
            var visible = Mathf.Min(effects.Count, MaxVisibleSlots);

            while (slots.Count < visible)
            {
                var slot = ActiveEffectSlotUI.Create(transform, SlotSize);
                slot.transform.SetSiblingIndex(slots.Count + 1);
                slots.Add(slot);
            }

            for (var i = 0; i < slots.Count; i++)
            {
                var active = i < visible;
                slots[i].gameObject.SetActive(active);

                if (active)
                {
                    slots[i].Bind(effects[i]);
                }
            }

            var hidden = effects.Count - visible;
            overflowText.gameObject.SetActive(hidden > 0);
            overflowText.text = "+" + hidden;
        }

        private void TogglePassives()
        {
            showPassives = !showPassives;
            PlayerPrefs.SetInt(ShowPassivesKey, showPassives ? 1 : 0);
            RefreshToggleVisual();
        }

        private void BuildRow()
        {
            var layout = gameObject.GetComponent<HorizontalLayoutGroup>();

            if (layout == null)
            {
                layout = gameObject.AddComponent<HorizontalLayoutGroup>();
            }

            layout.spacing = Spacing;
            layout.childAlignment = TextAnchor.LowerLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var button = new GameObject("PassivesToggle", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            button.transform.SetParent(transform, false);
            button.transform.SetAsFirstSibling();
            var element = button.GetComponent<LayoutElement>();
            element.preferredWidth = 18f;
            element.preferredHeight = 18f;
            toggleImage = button.GetComponent<Image>();
            button.GetComponent<Button>().onClick.AddListener(TogglePassives);

            var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            label.transform.SetParent(button.transform, false);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelText = label.GetComponent<TextMeshProUGUI>();
            labelText.text = "P";
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.fontSize = 11f;
            labelText.color = Color.white;
            labelText.raycastTarget = false;

            var overflow = new GameObject("Overflow", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            overflow.transform.SetParent(transform, false);
            overflow.GetComponent<LayoutElement>().preferredWidth = 28f;
            overflowText = overflow.GetComponent<TextMeshProUGUI>();
            overflowText.fontSize = 14f;
            overflowText.alignment = TextAlignmentOptions.MidlineLeft;
            overflowText.color = Color.white;
            overflowText.raycastTarget = false;
            overflow.SetActive(false);

            RefreshToggleVisual();
        }

        private void RefreshToggleVisual()
        {
            toggleImage.color = showPassives ? new Color(0.25f, 0.45f, 0.75f, 0.95f) : new Color(0.15f, 0.15f, 0.2f, 0.7f);
        }
    }
}
