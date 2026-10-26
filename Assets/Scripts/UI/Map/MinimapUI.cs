using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Project.Maps;
using Project.CameraSystem;

namespace Project.UI
{
    /// <summary>
    /// Always-visible HUD radar centered on the player, rotating with the
    /// camera's yaw (<see cref="IsometricCameraController.CurrentYaw"/>) so
    /// it works as a compass: a "North" icon placed as a fixed child of
    /// <see cref="mapContent"/> orbits the rim as the camera turns, sitting
    /// at the top when the camera is at its default yaw. Distinct from
    /// <see cref="WorldMapWindowUI"/>, which is the toggleable, non-rotating
    /// full-map grid. Marker icons come from every
    /// <see cref="MinimapMarkerSource"/> in the currently loaded map and are
    /// positioned relative to the player, re-collected on every
    /// <see cref="CurrentMapTracker.MapChanged"/> since a map change fully
    /// reloads the scene (see <c>MapTransitionService</c>).
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private RectTransform mapContent;
        [SerializeField] private Image markerPrefab;
        [SerializeField] private float worldUnitsToPixels = 10f;
        [SerializeField] private float radiusPixels = 90f;

        private readonly List<(MinimapMarkerSource source, RectTransform icon)> markers = new();

        private void Start()
        {
            Populate();
            CurrentMapTracker.MapChanged += OnMapChanged;
        }

        private void OnDestroy()
        {
            CurrentMapTracker.MapChanged -= OnMapChanged;
        }

        private void OnMapChanged(MapDefinition map)
        {
            Populate();
        }

        private void Populate()
        {
            foreach (var (_, icon) in markers)
            {
                if (icon != null)
                {
                    Destroy(icon.gameObject);
                }
            }
            markers.Clear();

            foreach (var source in FindObjectsByType<MinimapMarkerSource>(FindObjectsSortMode.None))
            {
                var icon = Instantiate(markerPrefab, mapContent);
                icon.color = source.IconColor;
                markers.Add((source, icon.rectTransform));
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

            foreach (var (source, icon) in markers)
            {
                if (source == null || icon == null)
                {
                    continue;
                }

                var offset = source.transform.position - player.position;
                var local = new Vector2(offset.x, offset.z) * worldUnitsToPixels;
                icon.gameObject.SetActive(local.magnitude <= radiusPixels);
                icon.anchoredPosition = local;
            }
        }
    }
}
