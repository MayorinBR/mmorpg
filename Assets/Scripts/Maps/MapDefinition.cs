using UnityEngine;

namespace Project.Maps
{
    /// <summary>
    /// Static data describing one map/scene for the World Map window: its
    /// stable id, display name, and its node's position in the window.
    /// Mirrors <see cref="Project.Skills.SkillDefinition"/>'s definition
    /// asset pattern — one asset per map, listed by a <see cref="MapDatabase"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "MapDefinition", menuName = "Project/Maps/Map")]
    public class MapDefinition : ScriptableObject
    {
        [Tooltip("Must exactly match the Unity scene name this map represents.")]
        [SerializeField] private string mapId;

        [SerializeField] private string displayName;

        [Tooltip("Anchored position (pixels) of this map's node in the World Map window's content area. Top-left anchor: X grows right, Y grows downward as a negative value, matching WindowLayoutManager's convention.")]
        [SerializeField] private Vector2 mapPosition;

        /// <summary>Gets the id this map is looked up by — must match the Unity scene name.</summary>
        public string MapId => mapId;

        /// <summary>Gets the name shown to the player.</summary>
        public string DisplayName => displayName;

        /// <summary>Gets this map's node position in the World Map window.</summary>
        public Vector2 MapPosition => mapPosition;
    }
}
