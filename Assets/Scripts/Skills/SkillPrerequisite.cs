using System;
using UnityEngine;

namespace Project.Skills
{
    /// <summary>
    /// A skill that must be learned at a minimum level before another skill
    /// can be learned or upgraded (e.g. Magnum Break needs Bash level 5).
    /// </summary>
    [Serializable]
    public struct SkillPrerequisite
    {
        [SerializeField] private SkillDefinition skill;
        [SerializeField, Min(1)] private int level;

        /// <summary>
        /// Creates a prerequisite.
        /// </summary>
        /// <param name="skill">The skill that must be learned first.</param>
        /// <param name="level">The minimum level that skill must have reached.</param>
        public SkillPrerequisite(SkillDefinition skill, int level)
        {
            this.skill = skill;
            this.level = level;
        }

        /// <summary>Gets the skill that must be learned first.</summary>
        public SkillDefinition Skill => skill;

        /// <summary>Gets the minimum level the required skill must have reached.</summary>
        public int Level => level;
    }
}
