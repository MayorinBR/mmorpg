using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.World
{
    /// <summary>
    /// Lives in the Bootstrap scene, which only holds the objects that persist
    /// across maps (player, camera, HUD, event system). The scene has no world
    /// content, so nothing can be interacted with while it loads. Once the
    /// persisted player has restored its save, this moves on to the default
    /// map, unless the save already requested a map through
    /// <see cref="MapTransitionService.WarpToPosition"/>.
    /// </summary>
    public class BootstrapLoader : MonoBehaviour
    {
        /// <summary>Name of the Bootstrap scene, as registered in Build Settings.</summary>
        public const string SceneName = "Bootstrap";

        [SerializeField] private string defaultMapSceneName = "Prototype_Map01";

        private IEnumerator Start()
        {
            // Waits one frame so PlayerSaveController.Start has already
            // restored the save and, if it holds a saved location, queued the
            // destination map.
            yield return null;

            if (SceneManager.sceneCount > 1 || MapTransitionService.PendingPosition.HasValue)
            {
                yield break;
            }

            SceneManager.LoadScene(defaultMapSceneName, LoadSceneMode.Single);
        }
    }
}
