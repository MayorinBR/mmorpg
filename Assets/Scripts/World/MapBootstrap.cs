using UnityEngine;
using Project.CameraSystem;
using Project.Character.Combat;

namespace Project.World
{
    /// <summary>
    /// Runs once when a map scene loads and re-wires the persisted player to
    /// this map's local objects. Unity cannot serialize a reference between
    /// two different scene files, so this replaces what would otherwise be a
    /// direct Inspector reference from the player to the camera and respawn
    /// point.
    /// </summary>
    public class MapBootstrap : MonoBehaviour
    {
        /// <summary>This map's local camera rig, retargeted to follow the persisted player.</summary>
        [SerializeField] private IsometricCameraController localCamera;

        /// <summary>
        /// This map's local <see cref="Camera"/> component, pushed into the
        /// persisted player's click and hover raycasters so they stop
        /// pointing at the previous map's now-destroyed camera.
        /// </summary>
        [SerializeField] private Camera localViewCamera;

        /// <summary>Where the persisted player respawns after dying on this map.</summary>
        [SerializeField] private Transform defaultRespawnPoint;

        private void Start()
        {
            var player = PersistentPlayerAnchor.Instance;
            if (player == null)
            {
                return;
            }

            if (localCamera != null)
            {
                localCamera.SetTarget(player.transform);
                player.MovementController.SetCameraYawSource(localCamera);
            }

            if (localViewCamera != null)
            {
                player.InputRouter.SetWorldCamera(localViewCamera);
                player.HoverDetector.SetWorldCamera(localViewCamera);
                player.StatsCanvasFollower.SetViewCamera(localViewCamera);
                SkillTargetingController.Instance?.SetWorldCamera(localViewCamera);
            }

            if (defaultRespawnPoint != null)
            {
                player.DeathHandler.SetRespawnPoint(defaultRespawnPoint);
            }

            WarpToPendingSpawnPoint(player);
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
