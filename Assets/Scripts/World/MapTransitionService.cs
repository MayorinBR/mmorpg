using Project.CameraSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.World
{
    /// <summary>
    /// Moves the player between map scenes. Loading is synchronous: for a
    /// prototype with small maps this keeps the flow simple, at the cost of
    /// a brief hitch on transition.
    /// </summary>
    public static class MapTransitionService
    {
        /// <summary>
        /// Identifier of the spawn point the next loaded map's
        /// <see cref="MapBootstrap"/> should place the persisted player at.
        /// Set by <see cref="WarpTo"/> and consumed once that map's own
        /// <c>Start()</c> runs — resolving it there, instead of right after
        /// <see cref="SceneManager.LoadScene"/> returns here, guarantees the
        /// destination scene's objects actually exist by the time the
        /// search runs.
        /// </summary>
        public static string PendingSpawnPointId { get; private set; }

        /// <summary>
        /// Loads the destination scene and records <paramref name="spawnPointId"/>
        /// for that scene's <see cref="MapBootstrap"/> to warp the persisted
        /// player to once it starts.
        /// </summary>
        /// <param name="sceneName">Destination scene name, must be registered in Build Settings.</param>
        /// <param name="spawnPointId">Identifier of the spawn point to arrive at in the destination scene.</param>
        public static void WarpTo(string sceneName, string spawnPointId)
        {
            PendingSpawnPointId = spawnPointId;
            BeginLoading();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// World-space position the next loaded map's <see cref="MapBootstrap"/>
        /// should place the persisted player at, set by <see cref="WarpToPosition"/>.
        /// </summary>
        public static Vector3? PendingPosition { get; private set; }

        /// <summary>
        /// Loads the destination scene and records an exact world position
        /// for that scene's <see cref="MapBootstrap"/> to warp the persisted
        /// player to, used when resuming a saved session mid-map.
        /// </summary>
        /// <param name="sceneName">Destination scene name, must be registered in Build Settings.</param>
        /// <param name="position">World-space position to arrive at.</param>
        public static void WarpToPosition(string sceneName, Vector3 position)
        {
            PendingPosition = position;
            BeginLoading();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Covers the screen with the loading panel and blocks the player's
        /// input. <see cref="MapBootstrap"/> reverses both once the destination
        /// map is ready.
        /// </summary>
        private static void BeginLoading()
        {
            LoadingScreen.Show();

            if (PersistentPlayerAnchor.Instance != null)
            {
                PersistentPlayerAnchor.Instance.SetInputEnabled(false);
            }
        }

        /// <summary>
        /// Clears <see cref="PendingPosition"/> and returns its previous
        /// value, so a stale position is never reapplied on a later scene load.
        /// </summary>
        public static Vector3? ConsumePendingPosition()
        {
            var position = PendingPosition;
            PendingPosition = null;
            return position;
        }

        /// <summary>
        /// Clears <see cref="PendingSpawnPointId"/> and returns its previous
        /// value. Called once by each map's <see cref="MapBootstrap"/> so a
        /// stale id can never be reapplied on a later, unrelated scene load.
        /// </summary>
        public static string ConsumePendingSpawnPointId()
        {
            var spawnPointId = PendingSpawnPointId;
            PendingSpawnPointId = null;
            return spawnPointId;
        }

        /// <summary>
        /// Leaves gameplay entirely for a menu scene (e.g. Character
        /// Creation), destroying the persisted player and UI roots first.
        /// Unlike <see cref="WarpTo"/> — where the persisted objects are
        /// meant to survive into the next map — the destination here has no
        /// player or HUD of its own, so those roots would otherwise leak
        /// into it and fight its own EventSystem. Also clears
        /// <see cref="PersistAcrossScenes"/>'s tracked names, so a later
        /// return to gameplay persists a fresh set instead of finding them
        /// already claimed.
        /// </summary>
        /// <param name="sceneName">Destination menu scene, must be registered in Build Settings.</param>
        public static void ReturnToMenu(string sceneName)
        {
            if (PersistentPlayerAnchor.Instance != null)
            {
                Object.Destroy(PersistentPlayerAnchor.Instance.gameObject);
            }

            foreach (var persisted in Object.FindObjectsByType<PersistAcrossScenes>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            {
                Object.Destroy(persisted.gameObject);
            }

            DestroyPersistentCameras();
            PersistAcrossScenes.ClearTracking();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Destroys the self-persisting gameplay cameras. They do not use
        /// <see cref="PersistAcrossScenes"/>, so without this they would leak
        /// into the menu scene (a second audio listener) and survive into the
        /// next gameplay session, leaving the freshly created UI pointing at
        /// the duplicate camera that destroyed itself.
        /// </summary>
        private static void DestroyPersistentCameras()
        {
            if (IsometricCameraController.Instance != null)
            {
                Object.Destroy(IsometricCameraController.Instance.gameObject);
            }

            if (MinimapCameraFollow.Instance != null)
            {
                Object.Destroy(MinimapCameraFollow.Instance.gameObject);
            }
        }
    }
}
