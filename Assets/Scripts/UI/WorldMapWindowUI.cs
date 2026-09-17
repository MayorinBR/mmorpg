using UnityEngine;
using Project.Maps;

namespace Project.UI
{
    /// <summary>
    /// Populates the World Map window with one <see cref="WorldMapEntryUI"/>
    /// node per map in <see cref="mapDatabase"/>, each positioned at its own
    /// authored <see cref="MapDefinition.MapPosition"/>, and keeps
    /// <see cref="marker"/> over whichever node matches the player's
    /// current map, updating it whenever <see cref="CurrentMapTracker"/>
    /// reports a change.
    /// </summary>
    public class WorldMapWindowUI : MonoBehaviour
    {
        [SerializeField] private MapDatabase mapDatabase;
        [SerializeField] private WorldMapEntryUI entryPrefab;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RectTransform marker;

        private void Start()
        {
            Populate();
            CurrentMapTracker.MapChanged += OnMapChanged;
            OnMapChanged(CurrentMapTracker.Current);
        }

        private void OnDestroy()
        {
            CurrentMapTracker.MapChanged -= OnMapChanged;
        }

        private void Populate()
        {
            foreach (var map in mapDatabase.AllMaps)
            {
                if (map == null)
                {
                    continue;
                }

                var entry = Instantiate(entryPrefab, contentRoot);
                entry.Setup(map);
            }
        }

        private void OnMapChanged(MapDefinition map)
        {
            if (map == null || marker == null)
            {
                return;
            }

            foreach (Transform child in contentRoot)
            {
                var entry = child.GetComponent<WorldMapEntryUI>();

                if (entry != null && entry.Map == map)
                {
                    marker.SetParent(entry.RectTransform, false);
                    marker.anchoredPosition = Vector2.zero;
                    return;
                }
            }
        }
    }
}
