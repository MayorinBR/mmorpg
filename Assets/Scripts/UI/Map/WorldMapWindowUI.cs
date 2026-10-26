using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Project.Maps;

namespace Project.UI
{
    /// <summary>
    /// Populates the World Map window with one <see cref="WorldMapEntryUI"/>
    /// node per map in <see cref="mapDatabase"/>, placed on a square grid of
    /// <see cref="gridColumns"/> by <see cref="gridRows"/> cells from each
    /// map's authored <see cref="MapDefinition.MapPosition"/>, and keeps
    /// <see cref="marker"/> over whichever node matches the player's current
    /// map, updating it whenever <see cref="CurrentMapTracker"/> reports a
    /// change. Sizes the window to <see cref="screenFraction"/> of the canvas
    /// and the cells to the largest size that fits the whole grid. Closes the
    /// window when the player clicks outside of it.
    /// </summary>
    public class WorldMapWindowUI : MonoBehaviour
    {
        [SerializeField] private MapDatabase mapDatabase;
        [SerializeField] private WorldMapEntryUI entryPrefab;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private RectTransform marker;

        [Tooltip("Fraction of the canvas width and height the window occupies.")]
        [SerializeField, Range(0.3f, 1f)] private float screenFraction = 0.8f;

        [Tooltip("Number of map cells across the grid.")]
        [SerializeField, Min(1)] private int gridColumns = 10;

        [Tooltip("Number of map cells down the grid.")]
        [SerializeField, Min(1)] private int gridRows = 10;

        [Tooltip("Authored MapDefinition position units that make up one grid cell. The existing maps step in 60.")]
        [SerializeField, Min(0.01f)] private float gridUnit = 60f;

        private readonly List<WorldMapEntryUI> entries = new List<WorldMapEntryUI>();
        private Vector2 chromeSize;
        private bool populated;
        private bool positioned;

        private void Start()
        {
            chromeSize = Window.sizeDelta - Content.sizeDelta;
            Populate();
            populated = true;
            ApplyLayout();
            CurrentMapTracker.MapChanged += OnMapChanged;
            OnMapChanged(CurrentMapTracker.Current);
        }

        private void OnEnable()
        {
            if (populated)
            {
                ApplyLayout();
            }
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (IsPointerOverOtherUi())
            {
                return;
            }

            if (!RectTransformUtility.RectangleContainsScreenPoint(Window, mouse.position.ReadValue()))
            {
                GetComponentInParent<WindowPanel>()?.Close();
            }
        }

        /// <summary>
        /// Checks whether the pointer is over any UI element, such as a HUD button. Those clicks must
        /// not close the window here, otherwise the window closes on press and the button then reopens
        /// it on release instead of closing it.
        /// </summary>
        /// <returns>True when the pointer is over a UI element.</returns>
        private static bool IsPointerOverOtherUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void OnDestroy()
        {
            CurrentMapTracker.MapChanged -= OnMapChanged;
        }

        private RectTransform Content => (RectTransform)contentRoot;

        private RectTransform Window => (RectTransform)contentRoot.parent;

        private void ApplyLayout()
        {
            var canvasSize = ((RectTransform)Window.parent).rect.size;
            var contentSize = canvasSize * screenFraction - chromeSize;
            var cell = Mathf.Max(1f, Mathf.Floor(Mathf.Min(contentSize.x / gridColumns, contentSize.y / gridRows)));

            Content.sizeDelta = contentSize;
            Window.sizeDelta = contentSize + chromeSize;
            StretchTitleBar();

            var cellSize = new Vector2(cell, cell);
            var gridTopLeft = new Vector2(-cell * gridColumns * 0.5f, cell * gridRows * 0.5f);

            foreach (var entry in entries)
            {
                var gridPosition = new Vector2(entry.Map.MapPosition.x, -entry.Map.MapPosition.y) / gridUnit;
                entry.Layout(cellSize, gridTopLeft + new Vector2(gridPosition.x, -gridPosition.y) * cell);
            }

            if (!positioned)
            {
                positioned = true;
                CenterWindow(canvasSize);
            }
        }

        private void StretchTitleBar()
        {
            foreach (RectTransform child in Window)
            {
                if (child == Content)
                {
                    continue;
                }

                child.anchorMin = new Vector2(0f, child.anchorMin.y);
                child.anchorMax = new Vector2(1f, child.anchorMax.y);
                child.sizeDelta = new Vector2(0f, child.sizeDelta.y);
                child.anchoredPosition = new Vector2(0f, child.anchoredPosition.y);
            }
        }

        private void CenterWindow(Vector2 canvasSize)
        {
            var position = new Vector2((canvasSize.x - Window.sizeDelta.x) * 0.5f, -(canvasSize.y - Window.sizeDelta.y) * 0.5f);
            Window.anchoredPosition = position;
            GetComponentInParent<WindowPanel>()?.SetCustomPosition(position);
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
                entries.Add(entry);
            }
        }

        private void OnMapChanged(MapDefinition map)
        {
            if (map == null || marker == null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                if (entry.Map == map)
                {
                    marker.SetParent(entry.RectTransform, false);
                    marker.anchoredPosition = Vector2.zero;
                    return;
                }
            }
        }
    }
}
