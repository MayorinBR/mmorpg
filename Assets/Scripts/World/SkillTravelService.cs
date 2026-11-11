using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Project.CameraSystem;
using Project.Character.Combat;

namespace Project.World
{
    /// <summary>
    /// Moves the player for the travel skills (Teleport, Warp Portal) using the map transition system.
    /// Registers itself as <see cref="SkillTravel.Service"/> when the game starts.
    /// </summary>
    public sealed class SkillTravelService : ISkillTravelService
    {
        private const float MinimumTeleportDistance = 8f;
        private const float NavMeshSampleDistance = 2f;

        /// <inheritdoc />
        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            SkillTravel.Service = new SkillTravelService();
        }

        /// <inheritdoc />
        public bool TryTeleportRandomly()
        {
            var player = PersistentPlayerAnchor.Instance;

            if (player == null)
            {
                return false;
            }

            var mesh = NavMesh.CalculateTriangulation();

            if (!NavMeshTrianglePicker.TryPick(mesh.vertices, mesh.indices, player.transform.position, MinimumTeleportDistance, () => Random.value, out var point))
            {
                return false;
            }

            if (!NavMesh.SamplePosition(point, out var hit, NavMeshSampleDistance, NavMesh.AllAreas))
            {
                return false;
            }

            MoveWithinScene(player, hit.position);
            return true;
        }

        /// <inheritdoc />
        public void TravelTo(WarpDestination destination)
        {
            var player = PersistentPlayerAnchor.Instance;

            if (player == null)
            {
                return;
            }

            if (destination.SceneName == CurrentSceneName)
            {
                MoveWithinScene(player, destination.Position);
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(destination.SceneName))
            {
                Debug.LogError($"SkillTravelService: scene '{destination.SceneName}' is not enabled in Build Settings.");
                return;
            }

            MapTransitionService.WarpToPosition(destination.SceneName, destination.Position);
        }

        /// <inheritdoc />
        public void ConfigurePortal(GameObject portalObject, WarpDestination destination, float activationRadius)
        {
            var portal = portalObject.GetComponent<SkillWarpPortal>();

            if (portal == null)
            {
                portal = portalObject.AddComponent<SkillWarpPortal>();
            }

            portal.Initialize(this, destination, activationRadius);
        }

        private static void MoveWithinScene(PersistentPlayerAnchor player, Vector3 position)
        {
            player.MovementController.StopMovement();
            player.MovementController.WarpTo(position);
            IsometricCameraController.Instance?.SnapToTarget();
        }
    }
}
