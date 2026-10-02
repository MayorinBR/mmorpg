using UnityEngine;
using UnityEngine.SceneManagement;
using Project.Persistence;

namespace Project.World
{
    /// <summary>
    /// Saves which map the player is on and where on it, so a character
    /// resumes exactly there on the next session. The map id is the active
    /// scene's name, the same key <see cref="MapBootstrap"/> uses to look up
    /// its <c>MapDefinition</c>. The player object only exists in the first
    /// gameplay scene, so when the saved map is a different scene, restoring
    /// loads that scene through <see cref="MapTransitionService"/> and the
    /// saved position is applied there once it has loaded.
    /// </summary>
    public class PlayerLocationSaveParticipant : MonoBehaviour, ISaveParticipant
    {
        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            var sceneName = SceneManager.GetActiveScene().name;
            if (sceneName == BootstrapLoader.SceneName)
            {
                return;
            }

            data.currentMapId = sceneName;

            var player = PersistentPlayerAnchor.Instance;
            if (player == null)
            {
                return;
            }

            var position = player.MovementController.transform.position;
            data.hasSavedPosition = true;
            data.positionX = position.x;
            data.positionY = position.y;
            data.positionZ = position.z;
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            var player = PersistentPlayerAnchor.Instance;
            if (player == null || !data.hasSavedPosition || string.IsNullOrEmpty(data.currentMapId))
            {
                return;
            }

            var savedPosition = new Vector3(data.positionX, data.positionY, data.positionZ);

            if (data.currentMapId != SceneManager.GetActiveScene().name)
            {
                if (Application.CanStreamedLevelBeLoaded(data.currentMapId))
                {
                    MapTransitionService.WarpToPosition(data.currentMapId, savedPosition);
                }

                return;
            }

            player.MovementController.StopMovement();
            player.MovementController.WarpTo(savedPosition);
        }
    }
}
