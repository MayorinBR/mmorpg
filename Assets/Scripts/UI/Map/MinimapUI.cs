using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Maps;
using Project.CameraSystem;

namespace Project.UI
{
    /// <summary>
    /// Always-visible HUD minimap. Shows the current map's captured picture (<see cref="MapDefinition.Thumbnail"/>)
    /// scrolling under a fixed player cursor at the center, so the player is always in the middle and nothing live
    /// (other players, enemies, spells) is drawn. The picture is placed from the world area it was captured over
    /// (<see cref="MapDefinition.ThumbnailCenter"/> and <see cref="MapDefinition.ThumbnailSideLength"/>). Warp portals
    /// (<see cref="MinimapMarkerType.Warp"/>) are drawn as fixed icons on the picture, since they are part of the map.
    /// The whole
    /// <see cref="mapContent"/> rotates with the camera's yaw
    /// (<see cref="IsometricCameraController.CurrentYaw"/>) so it works as a compass: a "North" icon placed as a fixed
    /// child of <see cref="mapContent"/> orbits the rim as the camera turns. Distinct from
    /// <see cref="WorldMapWindowUI"/>, which is the toggleable, non-rotating full-map grid. The picture is reloaded on
    /// every <see cref="CurrentMapTracker.MapChanged"/>, since a map change fully reloads the scene (see
    /// <c>MapTransitionService</c>).
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private RectTransform mapContent;
        [SerializeField] private RawImage mapImage;
        [SerializeField] private float worldUnitsToPixels = 10f;

        [SerializeField] private float warpIconSize = 12f;

        private readonly List<GameObject> warpIcons = new();
        private MapDefinition map;
        private bool hasPicture;

        private void Start()
        {
            ApplyMap(CurrentMapTracker.Current);
            CurrentMapTracker.MapChanged += ApplyMap;
        }

        private void OnDestroy()
        {
            CurrentMapTracker.MapChanged -= ApplyMap;
        }

        /// <summary>
        /// Shows the given map's picture, or hides the picture when the map has none or its captured area is unknown.
        /// </summary>
        /// <param name="newMap">The map the player is now on.</param>
        private void ApplyMap(MapDefinition newMap)
        {
            map = newMap;
            hasPicture = map != null && map.Thumbnail != null && map.ThumbnailSideLength > 0f;
            mapImage.enabled = hasPicture;

            if (hasPicture)
            {
                mapImage.texture = map.Thumbnail.texture;
                mapImage.rectTransform.sizeDelta = Vector2.one * (map.ThumbnailSideLength * worldUnitsToPixels);
            }

            PlaceWarpIcons();
        }

        /// <summary>
        /// Replaces the warp icons with one per warp marker in the loaded scene, positioned on the picture.
        /// </summary>
        private void PlaceWarpIcons()
        {
            foreach (var icon in warpIcons)
            {
                Destroy(icon);
            }

            warpIcons.Clear();

            if (!hasPicture)
            {
                return;
            }

            foreach (var source in FindObjectsByType<MinimapMarkerSource>(FindObjectsSortMode.None))
            {
                if (source.MarkerType != MinimapMarkerType.Warp)
                {
                    continue;
                }

                var icon = new GameObject("WarpIcon", typeof(RectTransform), typeof(Image));
                icon.transform.SetParent(mapImage.rectTransform, false);
                var image = icon.GetComponent<Image>();
                image.color = source.IconColor;
                image.raycastTarget = false;

                var rect = image.rectTransform;
                rect.sizeDelta = Vector2.one * warpIconSize;
                var position = source.transform.position;
                rect.anchoredPosition = new Vector2(position.x - map.ThumbnailCenter.x, position.z - map.ThumbnailCenter.y) * worldUnitsToPixels;
                warpIcons.Add(icon);
            }
        }

        private void Update()
        {
            if (player == null || mapContent == null)
            {
                return;
            }

            // ponytail: camera-yaw sign is a guess; flip to
            // -IsometricCameraController.Instance.CurrentYaw here if the
            // compass turns the wrong way once you see it in Play Mode.
            var camera = IsometricCameraController.Instance;
            if (camera != null)
            {
                mapContent.localEulerAngles = new Vector3(0f, 0f, camera.CurrentYaw);
            }

            if (!hasPicture)
            {
                return;
            }

            var offset = new Vector2(player.position.x - map.ThumbnailCenter.x, player.position.z - map.ThumbnailCenter.y);
            mapImage.rectTransform.anchoredPosition = -offset * worldUnitsToPixels;
        }
    }
}
