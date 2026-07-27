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
    /// current/max level, mana cost, and cooldown, with a button to learn
    /// or upgrade it. Acts as a drag source for assigning the skill to a
    /// hotbar slot once it's been learned (dragging an unlearned skill is
    /// a no-op). Resolves its parent Canvas automatically at runtime,
    /// since instances are cloned from a prefab asset and can't have a
    /// scene reference dragged into them ahead of time.
    /// </summary>
    public class SkillBookEntryUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text manaCostText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private Button learnButton;

        private Canvas rootCanvas;
        private PlayerSkillBook skillBook;
        private Image dragIcon;

        /// <summary>Gets the skill this row represents.</summary>
        public SkillDefinition Skill { get; private set; }

        private void Awake()
        {
            rootCanvas = GetComponentInParent<Canvas>();
            learnButton.onClick.AddListener(OnLearnClicked);
        }

        /// <summary>
        /// Configures this row for a specific skill, refreshing its display immediately.
        /// </summary>
        /// <param name="skill">The skill to represent.</param>
        /// <param name="book">The player's skill book, used to read/spend levels.</param>
        public void Setup(SkillDefinition skill, PlayerSkillBook book)
        {
            Skill = skill;
            skillBook = book;
            iconImage.sprite = skill.Icon;
            nameText.text = skill.SkillName;
            manaCostText.text = $"{skill.ManaCost} SP";
            cooldownText.text = $"{skill.CooldownSeconds:0.#}s";
            Refresh();
        }

        /// <summary>Refreshes the displayed level and learn button state from the current skill book.</summary>
        public void Refresh()
        {
            var level = skillBook.GetLevel(Skill);
            levelText.text = $"{level}/{Skill.MaxLevel}";
            learnButton.interactable = level < Skill.MaxLevel;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (skillBook.GetLevel(Skill) <= 0 || rootCanvas == null)
            {
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