using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Skills
{
    /// <summary>
    /// Maps each <see cref="CharacterClass"/> to the <see cref="SkillDatabase"/>
    /// listing that class's skills. Read by the Skill Book window to pick
    /// the right database for the player's current class, so each class's
    /// skill list lives in exactly one place — its own per-class database
    /// asset — instead of a manually kept-in-sync copy. Mirrors
    /// <see cref="ClassIconLookup"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassSkillDatabaseLookup", menuName = "Project/Skills/Class Skill Database Lookup")]
    public class ClassSkillDatabaseLookup : ScriptableObject
    {
        [SerializeField] private ClassDatabaseEntry[] databases;

        /// <summary>
        /// Gets the skill database for the given class.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>That class's skill database, or null if none is configured.</returns>
        public SkillDatabase GetDatabase(CharacterClass characterClass)
        {
            foreach (var entry in databases)
            {
                if (entry.CharacterClass == characterClass)
                {
                    return entry.Database;
                }
            }

            return null;
        }

        [Serializable]
        private struct ClassDatabaseEntry
        {
            [SerializeField] private CharacterClass characterClass;
            [SerializeField] private SkillDatabase database;

            public CharacterClass CharacterClass => characterClass;
            public SkillDatabase Database => database;
        }
    }
}
