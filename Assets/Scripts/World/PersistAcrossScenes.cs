using System.Collections.Generic;
using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Keeps this GameObject alive across scene loads and prevents a
    /// duplicate from appearing if its scene is loaded again later (for
    /// example, warping back into a map that was visited before). Intended
    /// for shared root objects that have no per-map state of their own, such
    /// as the UI canvas or the event system.
    /// </summary>
    public class PersistAcrossScenes : MonoBehaviour
    {
        private static readonly HashSet<string> PersistedNames = new HashSet<string>();

        private void Awake()
        {
            if (!PersistedNames.Add(gameObject.name))
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
        }
    }
}
