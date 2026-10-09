using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Limits how long a dropped <see cref="ItemPickup"/> stays on the map. During the last
    /// <see cref="blinkSeconds"/> the icon blinks, but the pickup stays collectable. When the map
    /// unloads first, the remaining time is saved in <see cref="DroppedItemRegistry"/>.
    /// </summary>
    [RequireComponent(typeof(ItemPickup))]
    public sealed class DroppedItemLifetime : MonoBehaviour
    {
        [Tooltip("Seconds a dropped item stays on the map.")]
        [SerializeField, Min(1f)] private float lifetimeSeconds = 30f;

        [Tooltip("Seconds before expiry during which the icon blinks.")]
        [SerializeField, Min(0f)] private float blinkSeconds = 5f;

        [Tooltip("Seconds the icon stays visible, then hidden, while blinking.")]
        [SerializeField, Min(0.05f)] private float blinkIntervalSeconds = 0.2f;

        private ItemPickup pickup;
        private float expiresAtTime;
        private bool expired;

        private void Awake()
        {
            pickup = GetComponent<ItemPickup>();
            expiresAtTime = Time.time + lifetimeSeconds;
        }

        private void Update()
        {
            var remaining = expiresAtTime - Time.time;

            if (remaining <= 0f)
            {
                expired = true;
                Destroy(gameObject);
                return;
            }

            if (remaining <= blinkSeconds)
            {
                pickup.SetIconVisible(Mathf.FloorToInt(remaining / blinkIntervalSeconds) % 2 == 0);
            }
        }

        private void OnDestroy()
        {
            if (expired || pickup.IsCollected || pickup.Item == null)
            {
                return;
            }

            DroppedItemRegistry.Save(
                gameObject.scene.name,
                new DroppedItemState(pickup.Item, pickup.Quantity, transform.position, expiresAtTime));
        }

        /// <summary>
        /// Replaces the expiry time, used to give a recreated pickup the time it had left.
        /// </summary>
        /// <param name="absoluteTime">Absolute <see cref="Time.time"/> at which the pickup disappears.</param>
        public void SetExpiry(float absoluteTime)
        {
            expiresAtTime = absoluteTime;
        }
    }
}
