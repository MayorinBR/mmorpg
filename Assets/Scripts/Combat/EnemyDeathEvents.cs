using System;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Publishes which species died whenever any enemy is defeated, for
    /// systems that need to react without holding a direct reference to
    /// every enemy instance in the scene (e.g. <c>Project.Quests</c>
    /// tracking a kill-creature objective). A static class, rather than a
    /// scene object either side would need a direct reference to, mirrors
    /// <c>Project.Maps.CurrentMapTracker</c>'s publisher pattern.
    /// Lives in <c>Project.Combat</c> rather than <c>Project.AI</c> (its
    /// original home) specifically so <c>Project.Quests</c> can subscribe to
    /// it without depending on <c>Project.AI</c> — <c>Project.AI</c>
    /// references <c>Project.World</c>, which references <c>Project.UI</c>,
    /// which already needs to reference <c>Project.Quests</c> for the Quest
    /// Log window; keeping this event on that path would close the loop back
    /// into a cyclic assembly reference. <c>Project.Combat</c> is a true leaf
    /// (only <c>Project.Character.Stats</c>), so it can't cycle back.
    /// </summary>
    public static class EnemyDeathEvents
    {
        /// <summary>Raised with the defeated enemy's species stats whenever an enemy's death is handled.</summary>
        public static event Action<CharacterStatsDefinition> EnemyKilled;

        /// <summary>Raises <see cref="EnemyKilled"/>. Called by <c>Project.AI.EnemyDeathHandler</c> only.</summary>
        /// <param name="species">The defeated enemy's species stats.</param>
        public static void RaiseEnemyKilled(CharacterStatsDefinition species)
        {
            EnemyKilled?.Invoke(species);
        }
    }
}
