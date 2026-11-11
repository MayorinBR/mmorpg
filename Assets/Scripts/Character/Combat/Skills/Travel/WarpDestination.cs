using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// A place a travel skill can take the player to: a scene and a world position in it.
    /// </summary>
    public readonly struct WarpDestination
    {
        /// <summary>Initializes the destination.</summary>
        /// <param name="label">The name shown to the player.</param>
        /// <param name="sceneName">The scene the destination is in.</param>
        /// <param name="position">The world position in that scene.</param>
        public WarpDestination(string label, string sceneName, Vector3 position)
        {
            Label = label;
            SceneName = sceneName;
            Position = position;
        }

        /// <summary>Gets the name shown to the player.</summary>
        public string Label { get; }

        /// <summary>Gets the scene the destination is in.</summary>
        public string SceneName { get; }

        /// <summary>Gets the world position in <see cref="SceneName"/>.</summary>
        public Vector3 Position { get; }
    }
}
