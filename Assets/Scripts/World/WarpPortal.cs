using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Project.World
{
    /// <summary>
    /// Triggers a map transition when the player gets close enough. Uses a
    /// per-frame distance check instead of a physics trigger because the
    /// player has no collider in this project. Only the horizontal distance
    /// counts, so the portal height never prevents activation.
    /// </summary>
    /// <remarks>
    /// The player arrives at the <see cref="MapSpawnPoint"/> of the destination
    /// map whose id equals the name of the scene this portal lives in, unless
    /// an explicit id is typed in the Inspector.
    /// </remarks>
    public class WarpPortal : MonoBehaviour
    {
#if UNITY_EDITOR
        [SerializeField] private SceneAsset destinationScene;
#endif
        [SerializeField] private string destinationSceneName = "Prototype_City1";
        [Tooltip("Optional. Leave empty to arrive at the spawn point named after this scene.")]
        [SerializeField] private string destinationSpawnPointId = "";
        [SerializeField] private float activationDistance = 1.5f;
        [SerializeField] private float reactivationCooldownSeconds = 1f;

        private float cooldownRemaining;

        /// <summary>Scene this portal leads to.</summary>
        public string DestinationSceneName => destinationSceneName;

        /// <summary>Spawn point id requested in the destination map.</summary>
        public string ArrivalId => string.IsNullOrEmpty(destinationSpawnPointId) ? gameObject.scene.name : destinationSpawnPointId;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (destinationScene != null)
            {
                destinationSceneName = destinationScene.name;
            }
        }
#endif

        private void Update()
        {
            if (cooldownRemaining > 0f)
            {
                cooldownRemaining -= Time.deltaTime;
                return;
            }

            var player = PersistentPlayerAnchor.Instance;
            if (player == null)
            {
                return;
            }

            var offset = player.transform.position - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude <= activationDistance * activationDistance)
            {
                cooldownRemaining = reactivationCooldownSeconds;

                if (!Application.CanStreamedLevelBeLoaded(destinationSceneName))
                {
                    Debug.LogError($"WarpPortal '{name}': scene '{destinationSceneName}' is not enabled in Build Settings.", this);
                    return;
                }

                MapTransitionService.WarpTo(destinationSceneName, ArrivalId);
            }
        }
    }
}
