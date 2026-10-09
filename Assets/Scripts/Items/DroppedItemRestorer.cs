using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Items
{
    /// <summary>
    /// Recreates the dropped items saved in <see cref="DroppedItemRegistry"/> whenever a map scene
    /// loads. Lives on a persistent object (the player), because the map scenes are replaced on
    /// every warp.
    /// </summary>
    public sealed class DroppedItemRestorer : MonoBehaviour
    {
        [SerializeField] private ItemPickup pickupPrefab;

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single)
            {
                return;
            }

            foreach (var state in DroppedItemRegistry.TakeActive(scene.name, Time.time))
            {
                var pickup = Instantiate(pickupPrefab, state.Position, Quaternion.identity);
                SceneManager.MoveGameObjectToScene(pickup.gameObject, scene);
                pickup.Initialize(state.Item, state.Quantity);

                if (pickup.TryGetComponent(out DroppedItemLifetime lifetime))
                {
                    lifetime.SetExpiry(state.ExpiresAtTime);
                }
            }
        }
    }
}
