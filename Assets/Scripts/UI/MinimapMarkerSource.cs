using UnityEngine;

namespace Project.UI
{
    /// <summary>Category a minimap marker belongs to, driving its icon color.</summary>
    public enum MinimapMarkerType
    {
        Warp = 0,
        Npc = 1,
        Quest = 2,
        Other = 3,
    }

    /// <summary>
    /// Marks a world object (an NPC, a warp portal) as something the
    /// minimap should show as an icon. Has no dependency on whatever it's
    /// attached to — <see cref="MinimapUI"/> finds every instance in the
    /// loaded scene via <c>FindObjectsByType</c> and reads only this
    /// component's own field, the same "sibling component with zero
    /// cross-references" shape already used by <see cref="DamageNumberSpawner"/>
    /// on non-UI GameObjects like Poring/Poporing.
    /// </summary>
    public class MinimapMarkerSource : MonoBehaviour
    {
        [SerializeField] private MinimapMarkerType markerType = MinimapMarkerType.Npc;
        [SerializeField] private Color otherColor = Color.white;

        /// <summary>Gets the color this marker's minimap icon should use, based on <see cref="MinimapMarkerType"/>.</summary>
        public Color IconColor => markerType switch
        {
            MinimapMarkerType.Warp => Color.yellow,
            MinimapMarkerType.Npc => Color.blue,
            MinimapMarkerType.Quest => Color.red,
            MinimapMarkerType.Other => otherColor,
            _ => Color.white,
        };
    }
}
