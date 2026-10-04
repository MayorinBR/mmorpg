using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Finds the renderer that draws a character's face.
    /// </summary>
    public static class FaceRendererLocator
    {
        /// <summary>
        /// Returns the explicit renderer when set, otherwise the first child renderer
        /// whose name matches <paramref name="meshName"/>.
        /// </summary>
        /// <param name="root">The character root to search under.</param>
        /// <param name="explicitRenderer">A renderer assigned in the inspector, or null.</param>
        /// <param name="meshName">Name of the head mesh object.</param>
        /// <returns>The face renderer, or null when none is found.</returns>
        public static Renderer Find(Component root, Renderer explicitRenderer, string meshName)
        {
            if (explicitRenderer != null)
            {
                return explicitRenderer;
            }

            foreach (Renderer candidate in root.GetComponentsInChildren<Renderer>(true))
            {
                if (candidate.name == meshName)
                {
                    return candidate;
                }
            }

            return null;
        }
    }
}
