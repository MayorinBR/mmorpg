using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// In-memory, session-lifetime store of the items lying on each map, keyed by scene name. A pickup
    /// saves itself here right before its map unloads and
    /// <see cref="DroppedItemRestorer"/> recreates it when that map loads again. Expiry is an absolute
    /// time, so a dropped item keeps running out while its map is unloaded.
    /// </summary>
    public static class DroppedItemRegistry
    {
        private static readonly Dictionary<string, List<DroppedItemState>> StatesByScene = new Dictionary<string, List<DroppedItemState>>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            StatesByScene.Clear();
        }

        /// <summary>
        /// Records a dropped item as lying on the given map.
        /// </summary>
        /// <param name="sceneName">Name of the map scene the item lies on.</param>
        /// <param name="state">The snapshot to record.</param>
        public static void Save(string sceneName, DroppedItemState state)
        {
            if (!StatesByScene.TryGetValue(sceneName, out var states))
            {
                states = new List<DroppedItemState>();
                StatesByScene[sceneName] = states;
            }

            states.Add(state);
        }

        /// <summary>
        /// Removes and returns the items recorded for a map that have not expired yet.
        /// </summary>
        /// <param name="sceneName">Name of the map scene being loaded.</param>
        /// <param name="currentTime">The current <see cref="Time.time"/>, used to discard expired items.</param>
        /// <returns>The items to recreate, empty when there are none.</returns>
        public static IReadOnlyList<DroppedItemState> TakeActive(string sceneName, float currentTime)
        {
            if (!StatesByScene.Remove(sceneName, out var states))
            {
                return Array.Empty<DroppedItemState>();
            }

            states.RemoveAll(state => state.ExpiresAtTime <= currentTime);
            return states;
        }
    }
}
