using UnityEngine;
using UnityEngine.EventSystems;
using Project.Maps;

namespace Project.UI
{
    /// <summary>
    /// One map node in the World Map window: positions itself from its
    /// <see cref="MapDefinition"/> and shows the map's name on hover via
    /// <see cref="MapTooltipUI"/>. <see cref="WorldMapWindowUI"/> reads
    /// <see cref="RectTransform"/> to place the current-map marker on it.
    /// </summary>
    public class WorldMapEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform rectTransform;

        /// <summary>Gets the map this node represents.</summary>
        public MapDefinition Map { get; private set; }

        /// <summary>Gets this node's own RectTransform, used to position the current-map marker.</summary>
        public RectTransform RectTransform => rectTransform;

        /// <summary>
        /// Configures this node for a specific map and positions it at the
        /// map's authored <see cref="MapDefinition.MapPosition"/>.
        /// </summary>
        /// <param name="map">The map this node represents.</param>
        public void Setup(MapDefinition map)
        {
            Map = map;
            rectTransform.anchoredPosition = map.MapPosition;
        }

        /// <summary>Shows the map's name in the shared <see cref="MapTooltipUI"/> tooltip.</summary>
        /// <param name="eventData">Pointer event data, used for the tooltip's screen position.</param>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (Map != null && MapTooltipUI.Instance != null)
            {
                MapTooltipUI.Instance.Show(Map.DisplayName, eventData.position);
            }
        }

        /// <summary>Hides the shared <see cref="MapTooltipUI"/> tooltip.</summary>
        /// <param name="eventData">Pointer event data (unused).</param>
        public void OnPointerExit(PointerEventData eventData)
        {
            if (MapTooltipUI.Instance != null)
            {
                MapTooltipUI.Instance.Hide();
            }
        }
    }
}
