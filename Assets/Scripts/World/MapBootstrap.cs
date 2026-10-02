using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Project.CameraSystem;
using Project.Character.Combat;
using Project.Maps;
using Project.UI;

namespace Project.World
{
    /// <summary>
    /// Runs once when a map scene loads and re-wires the persisted player to
    /// this map's local objects (respawn point, combat target rings) and to
    /// the persisted <see cref="IsometricCameraController"/> singleton, which
    /// Unity cannot serialize as a direct Inspector reference across scene
    /// files. Also publishes this scene's own <see cref="MapDefinition"/>
    /// to <see cref="CurrentMapTracker"/>, which is how the World Map
    /// window learns the player changed map.
    /// </summary>
    public class MapBootstrap : MonoBehaviour
    {
        /// <summary>Looked up by this scene's own name to find its <see cref="MapDefinition"/>.</summary>
        [SerializeField] private MapDatabase mapDatabase;

        /// <summary>Where the persisted player respawns after dying on this map.</summary>
        [SerializeField] private Transform defaultRespawnPoint;

        /// <summary>This map's ring indicator for the player's current combat target.</summary>
        [SerializeField] private GroundRingFollower localCurrentTargetRing;

        /// <summary>This map's ring indicator for the skill target picker's hovered enemy.</summary>
        [SerializeField] private GroundRingFollower localSkillPickerRing;

        private IEnumerator Start()
        {
            // Lets a map scene be played directly from the Editor: without the
            // persistent objects it would have no player, so the Bootstrap
            // scene is added on top of it.
            if (PersistentPlayerAnchor.Instance == null && Application.CanStreamedLevelBeLoaded(BootstrapLoader.SceneName))
            {
                yield return SceneManager.LoadSceneAsync(BootstrapLoader.SceneName, LoadSceneMode.Additive);
                yield return null;
            }

            Initialize();

            // Waits a couple of frames so the snapped camera and the warped
            // player have settled before the map is revealed.
            yield return null;
            yield return null;

            LoadingScreen.Hide();

            if (PersistentPlayerAnchor.Instance != null)
            {
                PersistentPlayerAnchor.Instance.SetInputEnabled(true);
            }
        }

        private void Initialize()
        {
            PublishCurrentMap();

            var player = PersistentPlayerAnchor.Instance;
            if (player == null)
            {
                return;
            }

            var activeCamera = IsometricCameraController.Instance;
            if (activeCamera != null)
            {
                activeCamera.SetTarget(player.transform);
                player.MovementController.SetCameraYawSource(activeCamera);

                var viewCamera = activeCamera.GetComponent<Camera>();
                if (viewCamera != null)
                {
                    player.InputRouter.SetWorldCamera(viewCamera);
                    player.HoverDetector.SetWorldCamera(viewCamera);
                    player.StatsCanvasFollower.SetViewCamera(viewCamera);
                    SkillTargetingController.Instance?.SetWorldCamera(viewCamera);
                }
            }

            if (defaultRespawnPoint != null)
            {
                player.DeathHandler.SetRespawnPoint(defaultRespawnPoint);
            }

            if (localCurrentTargetRing != null && localSkillPickerRing != null)
            {
                player.CombatTargetIndicators?.SetRings(localCurrentTargetRing, localSkillPickerRing);
            }

            WarpToPendingSpawnPoint(player);
            WarpToPendingPosition(player);
            EnsureOnNavMesh(player);

            if (activeCamera != null)
            {
                activeCamera.SnapToTarget();
            }
        }

        /// <summary>
        /// Places the persisted player on this map's NavMesh when nothing else
        /// did. The player comes from the Bootstrap scene, which has no
        /// NavMesh, so its agent starts unplaced and stays that way until warped.
        /// </summary>
        /// <param name="player">The persisted player.</param>
        private void EnsureOnNavMesh(PersistentPlayerAnchor player)
        {
            if (player.MovementController.IsOnNavMesh)
            {
                return;
            }

            var position = defaultRespawnPoint != null ? defaultRespawnPoint.position : player.MovementController.transform.position;
            player.MovementController.WarpTo(position);
        }

        private static void WarpToPendingPosition(PersistentPlayerAnchor player)
        {
            var position = MapTransitionService.ConsumePendingPosition();
            if (position == null)
            {
                return;
            }

            player.MovementController.StopMovement();
            player.MovementController.WarpTo(position.Value);
        }

        private void PublishCurrentMap()
        {
            if (mapDatabase == null)
            {
                return;
            }

            var map = mapDatabase.FindById(SceneManager.GetActiveScene().name);

            if (map != null)
            {
                CurrentMapTracker.SetCurrentMap(map);
            }
            else
            {
                Debug.LogWarning($"MapBootstrap: no MapDefinition found for scene '{SceneManager.GetActiveScene().name}'. World Map window won't show a current-map marker.");
            }
        }

        private static void WarpToPendingSpawnPoint(PersistentPlayerAnchor player)
        {
            var spawnPointId = MapTransitionService.ConsumePendingSpawnPointId();
            if (spawnPointId == null)
            {
                return;
            }

            // Clears any path or destination left over from the previous
            // map before repositioning, so nothing can pull the player away
            // from the spawn point on the frames right after warping.
            player.MovementController.StopMovement();

            foreach (var spawnPoint in FindObjectsByType<MapSpawnPoint>(FindObjectsSortMode.None))
            {
                if (spawnPoint.SpawnPointId == spawnPointId)
                {
                    player.MovementController.WarpTo(spawnPoint.Position);
                    return;
                }
            }

            Debug.LogWarning($"MapBootstrap: no MapSpawnPoint with id '{spawnPointId}' found in this scene. Player position was left unchanged.");
        }
    }
}
