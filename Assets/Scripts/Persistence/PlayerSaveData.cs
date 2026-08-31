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
    /// Cross-assembly references (a learned skill, an equipped element)
    /// are stored as an id string or an enum's underlying int, resolved
    /// back to the real type by whichever <see cref="ISaveParticipant"/>
    /// owns that data.
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
    }
}
