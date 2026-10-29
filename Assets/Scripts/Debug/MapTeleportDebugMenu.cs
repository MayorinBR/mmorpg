#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Project.World;

namespace Project.DebugTools
{
    /// <summary>
    /// Debug overlay that teleports the player to any map scene under
    /// <c>Scenes/Maps</c>, arriving at that map's default respawn point.
    /// Toggled with <see cref="ToggleKey"/> and only available in the Editor
    /// and development builds. Creates itself on startup, so no scene needs
    /// to reference it.
    /// </summary>
    public class MapTeleportDebugMenu : MonoBehaviour
    {
        private const Key ToggleKey = Key.F10;
        private const string MapsFolder = "/Maps/";
        private const string RetiredMapSuffix = "_OLD";
        private const float PanelWidth = 260f;
        private const float PanelMargin = 10f;
        private const float ButtonHeight = 26f;

        private readonly List<string> mapNames = new List<string>();
        private Vector2 scrollPosition;
        private bool visible;

        /// <summary>
        /// Spawns the persistent menu object once, after the first scene loads.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            var host = new GameObject(nameof(MapTeleportDebugMenu));
            DontDestroyOnLoad(host);
            host.AddComponent<MapTeleportDebugMenu>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || !keyboard[ToggleKey].wasPressedThisFrame)
            {
                return;
            }

            visible = !visible;

            if (visible)
            {
                RefreshMapList();
            }
        }

        private void OnGUI()
        {
            if (!visible)
            {
                return;
            }

            GUILayout.BeginArea(new Rect(PanelMargin, PanelMargin, PanelWidth, Screen.height - PanelMargin * 2f), GUI.skin.box);
            GUILayout.Label($"Teleport to map ({ToggleKey})");

            var canTeleport = PersistentPlayerAnchor.Instance != null;
            var currentScene = SceneManager.GetActiveScene().name;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            foreach (var mapName in mapNames)
            {
                GUI.enabled = canTeleport && mapName != currentScene;
                if (GUILayout.Button(mapName, GUILayout.Height(ButtonHeight)))
                {
                    Teleport(mapName);
                }
            }

            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        /// Rebuilds the list from the scenes registered in Build Settings
        /// that live under the maps folder, skipping retired copies.
        /// </summary>
        private void RefreshMapList()
        {
            mapNames.Clear();

            for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                var name = Path.GetFileNameWithoutExtension(path);

                if (path.Contains(MapsFolder) && !name.EndsWith(RetiredMapSuffix))
                {
                    mapNames.Add(name);
                }
            }

            mapNames.Sort();
        }

        /// <summary>
        /// Closes the menu and loads the chosen map with the player at its respawn point.
        /// </summary>
        /// <param name="mapName">Scene name of the destination map.</param>
        private void Teleport(string mapName)
        {
            visible = false;
            MapTransitionService.WarpToRespawnPoint(mapName);
        }
    }
}
#endif
