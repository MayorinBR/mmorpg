using UnityEngine;

namespace Project.Skills
{
    /// <summary>
    /// Resolves a stable string id to a <see cref="SkillDefinition"/> asset
    /// and back, using the asset's own name. Exists so the save system
    /// (which cannot serialize a direct ScriptableObject reference through
    /// JSON) can record and later look up "which skill" without depending
    /// on any particular UI component's own list of known skills.
    /// </summary>
    [CreateAssetMenu(fileName = "SkillDatabase", menuName = "Project/Skills/Skill Database")]
    public class SkillDatabase : ScriptableObject
    {
        [SerializeField] private SkillDefinition[] allSkills;

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
