using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Project.Character.Combat;
using Project.Skills;

namespace Project.UI
{
    /// <summary>
    /// Displays one hotbar slot: the assigned skill's icon, a cooldown
    /// overlay with remaining time while cooling down, and a red tint
    /// when the skill can't currently be cast for any other reason
    /// (not learned, insufficient mana, no valid target). Accepts
    /// drag-and-drop from <see cref="SkillBookEntryUI"/> to assign a
    /// skill to this slot, and casts on click when ready.
    /// </summary>
    public class SkillHotbarSlotUI : MonoBehaviour, IDropHandler, IPointerClickHandler
    {
        private static readonly Color ReadyColor = Color.white;
        private static readonly Color CooldownColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        private static readonly Color UnusableColor = new Color(1f, 0.4f, 0.4f, 1f);

        [SerializeField] private int slotIndex;
        [SerializeField] private PlayerSkillHotbar hotbar;
        [SerializeField] private PlayerSkillCaster caster;
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject cooldownOverlay;
        [SerializeField] private TMP_Text cooldownText;

        private SkillDefinition assignedSkill;

        private void OnEnable()
        {
            hotbar.SlotChanged += OnSlotChanged;
            RefreshIcon(hotbar.GetSkill(slotIndex));
        }

        private void OnDisable()
        {
            hotbar.SlotChanged -= OnSlotChanged;
        }

        private void Update()
        {
            RefreshState();
        }

        public void OnDrop(PointerEventData eventData)
        {
            var source = eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<SkillBookEntryUI>()
                : null;

            if (source != null && source.Skill != null)
            {
                hotbar.SetSkill(slotIndex, source.Skill);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (assignedSkill != null && caster.GetAvailability(assignedSkill) == SkillAvailability.Ready)
            {
                caster.TryCastSkill(assignedSkill);
            }
        }

        private void OnSlotChanged(int changedSlotIndex, SkillDefinition skill)
        {
            if (changedSlotIndex == slotIndex)
            {
                RefreshIcon(skill);
            }
        }

        private void RefreshIcon(SkillDefinition skill)
        {
            assignedSkill = skill;
            iconImage.enabled = skill != null;
            iconImage.sprite = skill != null ? skill.Icon : null;
        }

        private void RefreshState()
        {
            if (assignedSkill == null)
            {
                cooldownOverlay.SetActive(false);
                return;
            }

            var availability = caster.GetAvailability(assignedSkill);

            if (availability == SkillAvailability.OnCooldown)
            {
                iconImage.color = CooldownColor;
                cooldownOverlay.SetActive(true);
                cooldownText.text = caster.GetCooldownRemaining(assignedSkill).ToString("0.0");
                return;
            }

            cooldownOverlay.SetActive(false);
            iconImage.color = availability == SkillAvailability.Ready ? ReadyColor : UnusableColor;
        }
    }
}