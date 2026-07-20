using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Project.Items;

namespace Project.UI
{
    /// <summary>
    /// A single shared tooltip panel showing an item's icon, name,
    /// description, stat bonuses, and requirements (level/class). Hover
    /// triggers across the UI (inventory slots, equipment slots, world
    /// pickups) all show and hide the same instance rather than each owning
    /// their own panel.
    /// </summary>
    public class ItemTooltipUI : MonoBehaviour
    {
        /// <summary>Gets the active tooltip instance in the scene.</summary>
        public static ItemTooltipUI Instance { get; private set; }

        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private TMP_Text requirementsText;
        [SerializeField] private Vector2 offsetFromPointer = new Vector2(16f, -16f);

        private Canvas parentCanvas;

        private void Awake()
        {
            Instance = this;
            parentCanvas = GetComponentInParent<Canvas>();
            Hide();
        }

        /// <summary>
        /// Shows the tooltip for the given item at the given screen position.
        /// </summary>
        /// <param name="item">The item to describe. If null, the tooltip hides instead.</param>
        /// <param name="screenPosition">The screen-space position to anchor the tooltip near (typically the pointer position).</param>
        public void Show(ItemDefinition item, Vector2 screenPosition)
        {
            if (item == null)
            {
                Hide();
                return;
            }

            iconImage.enabled = item.Icon != null;
            iconImage.sprite = item.Icon;
            nameText.text = item.ItemName;
            descriptionText.text = item.Description;
            statsText.text = BuildStatsText(item);
            requirementsText.text = BuildRequirementsText(item);

            rectTransform.position = ClampToScreen(screenPosition + offsetFromPointer);
            root.SetActive(true);
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

        /// <summary>Hides the tooltip.</summary>
        public void Hide()
        {
            root.SetActive(false);
        }

        private string BuildStatsText(ItemDefinition item)
        {
            if (item.ItemType != ItemType.Equipment)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            AppendIfNonZero(builder, "STR", item.StatBonuses.Strength);
            AppendIfNonZero(builder, "AGI", item.StatBonuses.Agility);
            AppendIfNonZero(builder, "VIT", item.StatBonuses.Vitality);
            AppendIfNonZero(builder, "INT", item.StatBonuses.Intelligence);
            AppendIfNonZero(builder, "DEX", item.StatBonuses.Dexterity);
            AppendIfNonZero(builder, "LUK", item.StatBonuses.Luck);
            return builder.ToString();
        }

        private void AppendIfNonZero(StringBuilder builder, string label, int value)
        {
            if (value == 0)
            {
                return;
            }

            var sign = value > 0 ? "+" : string.Empty;
            builder.AppendLine($"{label} {sign}{value}");
        }

        private string BuildRequirementsText(ItemDefinition item)
        {
            if (item.ItemType != ItemType.Equipment)
            {
                return string.Empty;
            }

            var classText = item.AllowedClasses.Count > 0
                ? string.Join(", ", item.AllowedClasses)
                : "Qualquer classe";

            return $"Nível requerido: {item.RequiredLevel}\nClasses: {classText}";
        }
    }
}