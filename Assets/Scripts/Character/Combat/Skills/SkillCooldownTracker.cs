using System.Collections.Generic;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Tracks when each skill becomes usable again. Time is passed in by the caller so the logic has no
    /// dependency on the engine clock.
    /// </summary>
    public sealed class SkillCooldownTracker
    {
        private readonly Dictionary<SkillDefinition, float> endTimes = new Dictionary<SkillDefinition, float>();

        /// <summary>Gets whether the skill is currently on cooldown.</summary>
        /// <param name="skill">The skill to check.</param>
        /// <param name="now">The current time in seconds.</param>
        /// <returns>True if the cooldown has not finished yet.</returns>
        public bool IsOnCooldown(SkillDefinition skill, float now)
        {
            return now < GetEndTime(skill);
        }

        /// <summary>Gets how long until the skill is usable again.</summary>
        /// <param name="skill">The skill to check.</param>
        /// <param name="now">The current time in seconds.</param>
        /// <returns>Seconds remaining, or 0 if the skill is ready.</returns>
        public float GetRemaining(SkillDefinition skill, float now)
        {
            var remaining = GetEndTime(skill) - now;
            return remaining > 0f ? remaining : 0f;
        }

        /// <summary>Starts the cooldown of a skill.</summary>
        /// <param name="skill">The skill that was just used.</param>
        /// <param name="now">The current time in seconds.</param>
        /// <param name="duration">The cooldown length in seconds.</param>
        public void Start(SkillDefinition skill, float now, float duration)
        {
            endTimes[skill] = now + duration;
        }

        private float GetEndTime(SkillDefinition skill)
        {
            return endTimes.TryGetValue(skill, out var endTime) ? endTime : 0f;
        }
    }
}
