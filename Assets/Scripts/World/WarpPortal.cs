using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Triggers a map transition when the player gets close enough. Uses a
    /// per-frame distance check instead of a physics trigger because the
    /// player has no collider in this project.
    /// </summary>
    public class WarpPortal : MonoBehaviour
    {
        [SerializeField] private string destinationSceneName = "Prototype_Map02";
        [SerializeField] private string destinationSpawnPointId = "Default";
        [SerializeField] private float activationDistance = 1.5f;
        [SerializeField] private float reactivationCooldownSeconds = 1f;

        private float cooldownRemaining;

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

            var distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= activationDistance)
            {
                cooldownRemaining = reactivationCooldownSeconds;
                MapTransitionService.WarpTo(destinationSceneName, destinationSpawnPointId);
            }
        }
    }
}
