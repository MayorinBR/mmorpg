using System.Collections.Generic;
using UnityEngine;

namespace Project.Skills
{
    /// <summary>
    /// Lists a set of skills (<see cref="AllSkills"/>) and resolves a
    /// stable string id to one of them and back (<see cref="GetId"/>/
    /// <see cref="FindById"/>), using each skill asset's own name as its
    /// id. Authored as two different kinds of instance of this same type:
    /// one per <see cref="Project.Character.Stats.CharacterClass"/> (see
    /// <see cref="ClassSkillDatabaseLookup"/>), listing only that class's
    /// skills for the Skill Book window; and a single master instance
    /// covering every skill in the game, used by the save system
    /// (<see cref="Project.Character.Combat.PlayerSkillBook"/>,
    /// <see cref="Project.Character.Combat.PlayerSkillHotbar"/>) to
    /// resolve a learned or hotbarred skill by id regardless of the
    /// player's current class.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "Project/Skills/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        [SerializeField] private SkillDefinition[] allSkills;

        /// <summary>
        /// Gets every skill in the database, in author order.
        /// </summary>
        public IReadOnlyList<SkillDefinition> AllSkills => allSkills;

        /// <summary>
        /// Gets the stable id for a skill, currently its asset name.
        /// </summary>
        /// <param name="skill">The skill to get an id for.</param>
        /// <returns>The skill's id, or an empty string if <paramref name="skill"/> is null.</returns>
        public string GetId(SkillDefinition skill)
        {
            return skill != null ? skill.name : string.Empty;
        }

        /// <summary>
        /// Finds the skill asset with the given id.
        /// </summary>
        /// <param name="id">The id to look up, as returned by <see cref="GetId"/>.</param>
        /// <returns>The matching skill, or null if not found or <paramref name="id"/> is empty.</returns>
        public SkillDefinition FindById(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            foreach (var skill in allSkills)
            {
                if (skill != null && skill.name == id)
                {
                    return skill;
                }
            }

            return null;
        }
    }
}
