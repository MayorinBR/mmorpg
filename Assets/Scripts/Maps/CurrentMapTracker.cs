using System;

namespace Project.Maps
{
    /// <summary>
    /// Publishes which map the player is currently on.
    /// <see cref="Project.World.MapBootstrap"/> sets this once per scene
    /// load; the World Map window (in <c>Project.UI</c>) subscribes to move
    /// its marker. A static class, rather than a scene object either side
    /// would need a direct reference to, is what lets <c>Project.World</c>
    /// and <c>Project.UI</c> communicate without either assembly depending
    /// on the other — both already depend on <c>Project.Maps</c> instead.
    /// </summary>
    public static class CurrentMapTracker
    {
        /// <summary>Gets the map the player is currently on, or null before the first scene load sets one.</summary>
        public static MapDefinition Current { get; private set; }

        /// <summary>Raised whenever <see cref="Current"/> changes, with the new value.</summary>
        public static event Action<MapDefinition> MapChanged;

        /// <summary>
        /// Sets the current map and raises <see cref="MapChanged"/>.
        /// </summary>
        /// <param name="map">The map the player is now on.</param>
        public static void SetCurrentMap(MapDefinition map)
        {
            Current = map;
            MapChanged?.Invoke(map);
        }
    }
}
