using System.Collections.Generic;
using UnityEngine;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Gathers the effects reported by every <see cref="IActiveEffectSource"/>
    /// on the player (timed buffs, statuses, toggles, overweight, passives)
    /// into one sorted list for the active effects bar. Sources are found
    /// automatically in the player hierarchy, so a new kind of effect only
    /// needs a new source component.
    /// </summary>
    public class ActiveEffectsTracker : MonoBehaviour
    {
        private readonly List<IActiveEffectSource> sources = new List<IActiveEffectSource>();
        private readonly List<ActiveEffect> scratch = new List<ActiveEffect>();
        private bool resolved;

        /// <summary>
        /// Finds the effect sources again, for a source component added after startup.
        /// </summary>
        public void RefreshSources()
        {
            sources.Clear();
            sources.AddRange(transform.root.GetComponentsInChildren<IActiveEffectSource>(true));
            resolved = true;
        }

        /// <summary>
        /// Fills a list with the effects currently affecting the player, sorted for display.
        /// </summary>
        /// <param name="into">The list to fill. It is cleared first.</param>
        /// <param name="includePassives">Whether permanent passive skills are included.</param>
        public void Collect(List<ActiveEffect> into, bool includePassives)
        {
            if (!resolved)
            {
                RefreshSources();
            }

            into.Clear();
            scratch.Clear();

            foreach (var source in sources)
            {
                source.CollectActiveEffects(scratch);
            }

            foreach (var effect in scratch)
            {
                if (includePassives || effect.Descriptor.Category != EffectCategory.Passive)
                {
                    into.Add(effect);
                }
            }

            ActiveEffectRules.SortForDisplay(into);
        }
    }
}
