using System.Collections.Generic;
using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Keeps track of every active switchable light group so a day and night controller can drive them without scene searches.
    /// </summary>
    public static class LightRegistry
    {
        private static readonly List<ILightSwitchable> Sources = new();

        /// <summary>
        /// Gets the switchable light sources currently registered.
        /// </summary>
        public static IReadOnlyList<ILightSwitchable> All => Sources;

        /// <summary>
        /// Registers a source. Registering the same source twice has no effect.
        /// </summary>
        /// <param name="source">The source to register.</param>
        public static void Register(ILightSwitchable source)
        {
            if (!Sources.Contains(source))
            {
                Sources.Add(source);
            }
        }

        /// <summary>
        /// Removes a source from the registry.
        /// </summary>
        /// <param name="source">The source to remove.</param>
        public static void Unregister(ILightSwitchable source)
        {
            Sources.Remove(source);
        }

        /// <summary>
        /// Switches every registered source on or off.
        /// </summary>
        /// <param name="lit">True to light all sources, false to turn them all off.</param>
        public static void SetAllLit(bool lit)
        {
            for (var i = 0; i < Sources.Count; i++)
            {
                Sources[i].SetLit(lit);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnDomainReload()
        {
            Sources.Clear();
        }
    }
}
