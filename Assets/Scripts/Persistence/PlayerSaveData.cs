using System;
using System.Collections.Generic;

namespace Project.Persistence
{
    /// <summary>
    /// Plain, engine-agnostic snapshot of everything about the player that
    /// currently gets saved. Deliberately holds only primitive-safe types
    /// (no ScriptableObject or enum references) so it serializes cleanly
    /// with <see cref="UnityEngine.JsonUtility"/> and so this whole
    /// assembly stays free of dependencies on any gameplay assembly.
    /// Cross-assembly references (a learned skill, an inventory item, an
    /// equipped element) are stored as an id string or an enum's underlying
    /// int, resolved back to the real type by whichever
    /// <see cref="ISaveParticipant"/> owns that data.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        /// <summary>The player's current Base Level.</summary>
        public int baseLevel = 1;

        /// <summary>Accumulated Base Experience toward the next Base Level.</summary>
        public int baseExperience;

        /// <summary>Current base Strength value.</summary>
        public int strength = 1;

        /// <summary>Current base Agility value.</summary>
        public int agility = 1;

        /// <summary>Current base Vitality value.</summary>
        public int vitality = 1;

        /// <summary>Current base Intelligence value.</summary>
        public int intelligence = 1;

        /// <summary>Current base Dexterity value.</summary>
        public int dexterity = 1;

        /// <summary>Current base Luck value.</summary>
        public int luck = 1;

        /// <summary>Unspent stat points available to invest.</summary>
        public int availableStatPoints;

        /// <summary>The player's current Job Level.</summary>
        public int jobLevel = 1;

        /// <summary>Accumulated Job Experience toward the next Job Level.</summary>
        public int jobExperience;

        /// <summary>Unspent skill points available to learn or upgrade skills.</summary>
        public int availableSkillPoints;

        /// <summary>The underlying int value of the player's <c>CharacterClass</c>.</summary>
        public int characterClassIndex;

        /// <summary>Current Zeny amount.</summary>
        public int zeny;

        /// <summary>The underlying int value of the Mage's selected <c>Element</c>.</summary>
        public int mageElementIndex;

        /// <summary>Ids (asset names) of every learned skill, parallel to <see cref="learnedSkillLevels"/>.</summary>
        public List<string> learnedSkillIds = new List<string>();

        /// <summary>Current level of each learned skill, parallel to <see cref="learnedSkillIds"/>.</summary>
        public List<int> learnedSkillLevels = new List<int>();

        /// <summary>Id (asset name) assigned to each hotbar slot, in slot order. An empty string means the slot is unassigned.</summary>
        public List<string> hotbarSkillIds = new List<string>();

        /// <summary>
        /// Id (asset name) held in each inventory slot, in slot order,
        /// parallel to <see cref="inventoryQuantities"/>. An empty string
        /// means that slot is empty. Length matches the inventory's slot
        /// count at the time of saving.
        /// </summary>
        public List<string> inventoryItemIds = new List<string>();

        /// <summary>Quantity held in each inventory slot, parallel to <see cref="inventoryItemIds"/>.</summary>
        public List<int> inventoryQuantities = new List<int>();

        /// <summary>Id (asset name) of each currently equipped item, parallel to <see cref="equippedSlotMasks"/>.</summary>
        public List<string> equippedItemIds = new List<string>();

        /// <summary>
        /// Bitmask of the <c>EquipmentSlot</c> value(s) each equipped item
        /// occupies (bit N set means slot N, by the enum's underlying int),
        /// parallel to <see cref="equippedItemIds"/>. A multi-slot item
        /// (e.g. a two-handed weapon) has more than one bit set.
        /// </summary>
        public List<int> equippedSlotMasks = new List<int>();

        /// <summary>Number of ammo units currently equipped.</summary>
        public int equippedAmmoCount;

        /// <summary>Stable id of each saved UI window, parallel to the other <c>window*</c> lists.</summary>
        public List<string> windowIds = new List<string>();

        /// <summary>Whether each saved window was open (1) or closed (0), parallel to <see cref="windowIds"/>.</summary>
        public List<int> windowIsOpen = new List<int>();

        /// <summary>Whether each saved window was minimized (1) or not (0), parallel to <see cref="windowIds"/>.</summary>
        public List<int> windowIsMinimized = new List<int>();

        /// <summary>Whether each saved window had a custom (dragged) position (1) or was still cascade-positioned (0), parallel to <see cref="windowIds"/>.</summary>
        public List<int> windowHasCustomPosition = new List<int>();

        /// <summary>X component of each saved window's anchored position, parallel to <see cref="windowIds"/>.</summary>
        public List<float> windowPositionX = new List<float>();

        /// <summary>Y component of each saved window's anchored position, parallel to <see cref="windowIds"/>.</summary>
        public List<float> windowPositionY = new List<float>();
    }
}
