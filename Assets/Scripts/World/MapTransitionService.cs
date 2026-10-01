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
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
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

            PersistAcrossScenes.ClearTracking();
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
    }
}
