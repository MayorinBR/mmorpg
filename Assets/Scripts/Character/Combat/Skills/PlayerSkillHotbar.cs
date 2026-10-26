using System;
using UnityEngine;
using Project.Persistence;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds which skill is currently assigned to each of the player's
    /// hotbar slots. Purely a mapping — casting is still handled by
    /// <see cref="PlayerSkillCaster"/>, and slot-to-key binding by
    /// <see cref="SkillInputRouter"/>. Kept separate so the hotbar UI can
    /// reassign slots at runtime (drag-and-drop) without either of those
    /// needing to know about it.
    /// </summary>
    public class PlayerSkillHotbar : MonoBehaviour, ISaveParticipant
    {
        /// <summary>The fixed number of hotbar slots.</summary>
        public const int SlotCount = 10;

        [SerializeField] private SkillDefinition[] slots = new SkillDefinition[SlotCount];
        [SerializeField] private SkillDatabase skillDatabase;

        /// <summary>Raised when a slot's assigned skill changes, with (slotIndex, newSkill).</summary>
        public event Action<int, SkillDefinition> SlotChanged;

        /// <summary>
        /// Gets the skill currently assigned to a slot.
        /// </summary>
        /// <param name="slotIndex">The slot index, from 0 to <see cref="SlotCount"/> - 1.</param>
        /// <returns>The assigned skill, or null if the slot is empty or the index is invalid.</returns>
        public SkillDefinition GetSkill(int slotIndex)
        {
            return IsValidIndex(slotIndex) ? slots[slotIndex] : null;
        }

        /// <summary>
        /// Assigns a skill to a slot, replacing whatever was there before.
        /// </summary>
        /// <param name="slotIndex">The slot index, from 0 to <see cref="SlotCount"/> - 1.</param>
        /// <param name="skill">The skill to assign.</param>
        public void SetSkill(int slotIndex, SkillDefinition skill)
        {
            if (!IsValidIndex(slotIndex))
            {
                return;
            }

            slots[slotIndex] = skill;
            SlotChanged?.Invoke(slotIndex, skill);
        }

        private bool IsValidIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < SlotCount;
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.hotbarSkillIds.Clear();

            if (skillDatabase == null)
            {
                return;
            }

            for (var i = 0; i < SlotCount; i++)
            {
                data.hotbarSkillIds.Add(skillDatabase.GetId(slots[i]));
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            if (skillDatabase == null)
            {
                return;
            }

            for (var i = 0; i < SlotCount && i < data.hotbarSkillIds.Count; i++)
            {
                SetSkill(i, skillDatabase.FindById(data.hotbarSkillIds[i]));
            }
        }
    }
}
