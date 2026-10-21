using System;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Static channel for announcing that a character just cast a named
    /// skill (e.g. a boss's Rage), so nearby players can see it. Decouples
    /// the gameplay systems that cast from whatever UI displays the name:
    /// <c>Project.UI</c> subscribes to <see cref="SkillAnnounced"/> without
    /// the casting assemblies needing a reference to it.
    /// </summary>
    public static class SkillAnnouncementChannel
    {
        /// <summary>Raised with the skill name and the world position to show it at.</summary>
        public static event Action<string, Vector3> SkillAnnounced;

        /// <summary>
        /// Announces a cast. Safe to call with no subscriber attached.
        /// </summary>
        /// <param name="skillName">The skill's display name.</param>
        /// <param name="worldPosition">Where the announcement should appear.</param>
        public static void Announce(string skillName, Vector3 worldPosition)
        {
            SkillAnnounced?.Invoke(skillName, worldPosition);
        }
    }
}
