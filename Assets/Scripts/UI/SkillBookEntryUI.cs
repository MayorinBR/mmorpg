using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Project.Character.Combat;
using Project.Skills;

namespace Project.UI
{
    /// <summary>
    /// Displays one row in the Skill Book window: a skill's icon, name,
    /// and current/max level, with a button to learn or upgrade it. Acts
    /// as a drag source for assigning the skill to a hotbar slot once
    /// it's been learned (dragging an unlearned skill is a no-op), and
    /// shows the shared <see cref="SkillTooltipUI"/> on hover.
    /// </summary>
    public class SkillBookEntryUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text manaCostText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private Button learnButton;

        private Canvas rootCanvas;
        private PlayerSkillBook skillBook;
        private PlayerExperience experience;
        private Image dragIcon;

        private static readonly Color TextColor = new Color(0.15f, 0.17f, 0.22f, 1f);

        /// <summary>The width of a card created by <see cref="CreateCard"/>.</summary>
        public const float CardWidth = 72f;

        /// <summary>The height of a card created by <see cref="CreateCard"/>.</summary>
        public const float CardHeight = 90f;

        /// <summary>
        /// Builds a compact skill card at runtime: icon, name, level and a
        /// learn button. Mana cost, cooldown and description are left to the
        /// shared <see cref="SkillTooltipUI"/> shown on hover. The card is
        /// assembled while inactive so <c>Awake</c> only runs once every
        /// reference is assigned.
        /// </summary>
        /// <param name="parent">The transform the card is parented to.</param>
        /// <returns>The new, active card.</returns>
        public static SkillBookEntryUI CreateCard(Transform parent)
        {
            var root = new GameObject("SkillCard", typeof(RectTransform), typeof(Image));
            root.SetActive(false);
            root.transform.SetParent(parent, false);
            root.GetComponent<Image>().color = Color.clear;

            var card = root.AddComponent<SkillBookEntryUI>();

            var icon = CreateChild("Icon", root.transform, typeof(Image)).GetComponent<Image>();
            icon.raycastTarget = false;
            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = iconRect.pivot = new Vector2(0.5f, 1f);
            iconRect.sizeDelta = new Vector2(44f, 44f);
            iconRect.anchoredPosition = new Vector2(0f, -4f);

            var nameLabel = CreateLabel("Name", root.transform, 10f, TextAlignmentOptions.Center);
            var nameRect = nameLabel.rectTransform;
            nameRect.anchorMin = new Vector2(0f, 0f);
            nameRect.anchorMax = new Vector2(1f, 0f);
            nameRect.pivot = new Vector2(0.5f, 0f);
            nameRect.sizeDelta = new Vector2(-4f, 24f);
            nameRect.anchoredPosition = new Vector2(0f, 26f);
            nameLabel.color = TextColor;
            nameLabel.enableAutoSizing = true;
            nameLabel.fontSizeMin = 7f;
            nameLabel.fontSizeMax = 10f;

            var levelLabel = CreateLabel("Level", root.transform, 11f, TextAlignmentOptions.MidlineRight);
            var levelRect = levelLabel.rectTransform;
            levelRect.anchorMin = levelRect.anchorMax = levelRect.pivot = Vector2.zero;
            levelRect.sizeDelta = new Vector2(44f, 20f);
            levelRect.anchoredPosition = new Vector2(1f, 3f);
            levelLabel.color = TextColor;

            var buttonObject = CreateChild("Learn", root.transform, typeof(Image), typeof(Button));
            var buttonRect = (RectTransform)buttonObject.transform;
            buttonRect.anchorMin = buttonRect.anchorMax = buttonRect.pivot = new Vector2(1f, 0f);
            buttonRect.sizeDelta = new Vector2(20f, 20f);
            buttonRect.anchoredPosition = new Vector2(-4f, 3f);
            buttonObject.GetComponent<Image>().color = new Color(0.25f, 0.45f, 0.85f, 1f);

            var plus = CreateLabel("Plus", buttonObject.transform, 14f, TextAlignmentOptions.Center);
            plus.text = "+";
            plus.fontStyle = FontStyles.Bold;
            Stretch(plus.rectTransform);

            card.iconImage = icon;
            card.nameText = nameLabel;
            card.levelText = levelLabel;
            card.learnButton = buttonObject.GetComponent<Button>();
            root.SetActive(true);
            return card;
        }

        private static GameObject CreateChild(string childName, Transform parent, params System.Type[] components)
        {
            var child = new GameObject(childName, typeof(RectTransform));

            foreach (var component in components)
            {
                child.AddComponent(component);
            }

            child.transform.SetParent(parent, false);
            return child;
        }

        private static TMP_Text CreateLabel(string labelName, Transform parent, float size, TextAlignmentOptions alignment)
        {
            var label = CreateChild(labelName, parent, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>Gets the skill this row represents.</summary>
        public SkillDefinition Skill { get; private set; }

        private void Awake()
        {
            rootCanvas = GetComponentInParent<Canvas>();
            learnButton.onClick.AddListener(OnLearnClicked);
        }

        private void OnDisable()
        {
            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Hide();
            }
        }

        /// <summary>
        /// Configures this row for a specific skill, refreshing its display immediately.
        /// </summary>
        /// <param name="skill">The skill to represent.</param>
        /// <param name="book">The player's skill book, used to read/spend levels.</param>
        /// <param name="playerExperience">Optional. The player's level, used to gray out the Learn button below <see cref="SkillDefinition.RequiredLevel"/>. Left null, the button ignores the level requirement (the underlying check in <see cref="PlayerSkillBook.TryLearnOrUpgrade"/> still applies either way).</param>
        public void Setup(SkillDefinition skill, PlayerSkillBook book, PlayerExperience playerExperience = null)
        {
            Skill = skill;
            skillBook = book;
            experience = playerExperience;
            iconImage.sprite = skill.Icon;
            nameText.text = skill.SkillName;

            if (manaCostText != null)
            {
                manaCostText.text = skill.EffectType == SkillEffectType.Passive ? "Passive" : $"{skill.ManaCost} SP";
            }

            if (cooldownText != null)
            {
                cooldownText.text = skill.EffectType == SkillEffectType.Passive ? string.Empty : $"{skill.CooldownSeconds:0.#}s";
            }

            Refresh();
        }

        /// <summary>Refreshes the displayed level and learn button state from the current skill book.</summary>
        public void Refresh()
        {
            var level = skillBook.GetLevel(Skill);
            levelText.text = $"{level}/{Skill.MaxLevel}";
            var meetsLevelRequirement = experience == null || experience.CurrentLevel >= Skill.RequiredLevel;
            learnButton.interactable = level < Skill.MaxLevel && meetsLevelRequirement;
            iconImage.color = level > 0 ? Color.white : new Color(1f, 1f, 1f, 0.5f);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Skill != null && SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Show(Skill, eventData.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Hide();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (SkillTooltipUI.Instance != null)
            {
                SkillTooltipUI.Instance.Hide();
            }

            if (skillBook.GetLevel(Skill) <= 0 || Skill.EffectType == SkillEffectType.Passive || rootCanvas == null)
            {
                // A passive skill applies its bonus automatically while
                // learned (see PlayerPassiveSkillController) and is never
                // cast, so it can't be dragged onto the hotbar.
                return;
            }

            dragIcon = new GameObject("SkillDragIcon", typeof(Image)).GetComponent<Image>();
            dragIcon.transform.SetParent(rootCanvas.transform, false);
            dragIcon.transform.SetAsLastSibling();
            dragIcon.raycastTarget = false;
            dragIcon.sprite = Skill.Icon;
            dragIcon.rectTransform.sizeDelta = iconImage.rectTransform.sizeDelta;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                dragIcon.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragIcon != null)
            {
                Destroy(dragIcon.gameObject);
                dragIcon = null;
            }
        }

        private void OnLearnClicked()
        {
            if (skillBook.TryLearnOrUpgrade(Skill))
            {
                Refresh();
            }
        }
    }
}