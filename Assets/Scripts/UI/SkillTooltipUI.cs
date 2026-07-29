using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Skills;

namespace Project.UI
{
    /// <summary>
    /// A single shared tooltip panel showing a skill's icon, name,
    /// description, mana cost, and cooldown. Mirrors <see cref="ItemTooltipUI"/>:
    /// hover triggers across the UI (currently the hotbar) show and hide
    /// this same instance rather than each owning their own panel.
    /// </summary>
    public class SkillTooltipUI : MonoBehaviour
    {
        /// <summary>Gets the active tooltip instance in the scene.</summary>
        public static SkillTooltipUI Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text manaCostText;
        [SerializeField] private TMP_Text cooldownText;
        [SerializeField] private Vector2 offsetFromPointer = new Vector2(16f, -16f);

        private Canvas parentCanvas;

        private void Awake()
        {
            Instance = this;
            parentCanvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>
        /// Shows the tooltip for the given skill at the given screen position.
        /// </summary>
        /// <param name="skill">The skill to describe. If null, the tooltip hides instead.</param>
        /// <param name="screenPosition">The screen-space position to anchor the tooltip near (typically the pointer position).</param>
        public void Show(SkillDefinition skill, Vector2 screenPosition)
        {
            if (skill == null)
            {
                Hide();
                return;
            }

            iconImage.enabled = skill.Icon != null;
            iconImage.sprite = skill.Icon;
            nameText.text = skill.SkillName;
            descriptionText.text = skill.Description;
            manaCostText.text = $"{skill.ManaCost} SP";
            cooldownText.text = $"{skill.CooldownSeconds:0.#}s";

            rectTransform.position = ClampToScreen(screenPosition + offsetFromPointer);
            root.SetActive(true);
        }

        /// <summary>Hides the tooltip.</summary>
        public void Hide()
        {
            root.SetActive(false);
        }

        private Vector2 ClampToScreen(Vector2 desiredPosition)
        {
            var scaleFactor = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
            var size = rectTransform.rect.size * scaleFactor;
            var pivot = rectTransform.pivot;

            var minX = desiredPosition.x - (size.x * pivot.x);
            var maxX = desiredPosition.x + (size.x * (1f - pivot.x));
            var minY = desiredPosition.y - (size.y * pivot.y);
            var maxY = desiredPosition.y + (size.y * (1f - pivot.y));

            var clamped = desiredPosition;

            if (maxX > Screen.width)
            {
                clamped.x -= maxX - Screen.width;
            }

            if (minX < 0f)
            {
                clamped.x -= minX;
            }

            if (maxY > Screen.height)
            {
                clamped.y -= maxY - Screen.height;
            }

            if (minY < 0f)
            {
                clamped.y -= minY;
            }

            return clamped;
        }
    }
}