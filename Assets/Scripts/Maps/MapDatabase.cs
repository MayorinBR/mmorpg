using System.Collections.Generic;
using UnityEngine;

namespace Project.Maps
{
    /// <summary>
    /// Lists every map in the game (<see cref="AllMaps"/>) and resolves one
    /// by its <see cref="MapDefinition.MapId"/>. A single master instance
    /// covers every map, mirroring <see cref="Project.Skills.SkillDatabase"/>'s
    /// master usage.
    /// </summary>
    [CreateAssetMenu(fileName = "MapDatabase", menuName = "Project/Maps/Map Database")]
    public class MapDatabase : ScriptableObject
    {
        [SerializeField] private MapDefinition[] allMaps;

        /// <summary>Gets every map in the database, in author order.</summary>
        public IReadOnlyList<MapDefinition> AllMaps => allMaps;

        /// <summary>
        /// Finds the map with the given id.
        /// </summary>
        /// <param name="mapId">The id to look up, matching a Unity scene name.</param>
        /// <returns>The matching map, or null if not found or <paramref name="mapId"/> is empty.</returns>
        public MapDefinition FindById(string mapId)
        {
            if (string.IsNullOrEmpty(mapId))
            {
                return null;
            }

            foreach (var map in allMaps)
            {
                if (map != null && map.MapId == mapId)
                {
                    return map;
                }
            }

            return null;
        }
    }
}
