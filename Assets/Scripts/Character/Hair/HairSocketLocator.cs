using UnityEngine;

namespace Project.Character.Hair
{
    /// <summary>
    /// Finds the transform that hairstyles are attached to.
    /// </summary>
    public static class HairSocketLocator
    {
        /// <summary>
        /// Returns the explicit socket when set, otherwise the first child transform named
        /// <paramref name="socketName"/>.
        /// </summary>
        /// <param name="root">The character root to search under.</param>
        /// <param name="explicitSocket">A socket assigned in the inspector, or null.</param>
        /// <param name="socketName">Name of the head socket bone.</param>
        /// <returns>The socket, or null when none is found.</returns>
        public static Transform Find(Component root, Transform explicitSocket, string socketName)
        {
            if (explicitSocket != null)
            {
                return explicitSocket;
            }

            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == socketName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
