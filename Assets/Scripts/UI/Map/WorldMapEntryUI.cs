using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Project.Maps;

namespace Project.UI
{
    /// <summary>
    /// One map node in the World Map window: shows the map's top-down thumbnail inset from its edges so
    /// neighbouring nodes keep a gap, and shows the map's name on hover via
    /// <see cref="MapTooltipUI"/>. <see cref="WorldMapWindowUI"/> reads
    /// <see cref="RectTransform"/> to place the current-map marker on it.
    /// </summary>
    public class WorldMapEntryUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private const float TilePadding = 2f;

        private static readonly Color FallbackTileColor = new Color(0.55f, 0.62f, 0.72f, 1f);

        [SerializeField] private RectTransform rectTransform;

        /// <summary>Gets the map this node represents.</summary>
        public MapDefinition Map { get; private set; }

        /// <summary>Gets this node's own RectTransform, used to position the current-map marker.</summary>
        public RectTransform RectTransform => rectTransform;

        /// <summary>
        /// Configures this node for a specific map and builds the tile
        /// showing the map's thumbnail.
        /// </summary>
        /// <param name="map">The map this node represents.</param>
        public void Setup(MapDefinition map)
        {
            Map = map;
            BuildTile(map.Thumbnail);
        }

        /// <summary>
        /// Sizes and positions this node inside the window's content area.
        /// </summary>
        /// <param name="size">The node's width and height, including the gap around its tile.</param>
        /// <param name="position">The node's anchored position (top-left corner relative to the content area's center).</param>
        public void Layout(Vector2 size, Vector2 position)
        {
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
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

        private void BuildTile(Sprite thumbnail)
        {
            var hitArea = GetComponent<Image>();
            hitArea.color = Color.clear;
            hitArea.canvasRenderer.cullTransparentMesh = false;

            var tile = new GameObject("Tile", typeof(RectTransform), typeof(Image));
            tile.transform.SetParent(transform, false);
            tile.transform.SetAsFirstSibling();

            var tileRect = (RectTransform)tile.transform;
            tileRect.anchorMin = Vector2.zero;
            tileRect.anchorMax = Vector2.one;
            tileRect.offsetMin = new Vector2(TilePadding, TilePadding);
            tileRect.offsetMax = new Vector2(-TilePadding, -TilePadding);

            var image = tile.GetComponent<Image>();
            image.sprite = thumbnail;
            image.color = thumbnail != null ? Color.white : FallbackTileColor;
            image.raycastTarget = false;
        }
    }
}
