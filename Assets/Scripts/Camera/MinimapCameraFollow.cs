using UnityEngine;

namespace Project.CameraSystem
{
    /// <summary>
    /// Keeps a top-down camera centered above the player on the XZ plane,
    /// rendering the real map (terrain, trees, buildings) into the
    /// minimap's RenderTexture. Rotation is fixed once in the Inspector
    /// (90, 0, 0) so the render stays north-up; <see cref="Project.UI.MinimapUI"/>
    /// spins the whole minimap container to match camera yaw, carrying this
    /// rendered image along with the markers automatically. Persists across
    /// scene loads the same way <see cref="IsometricCameraController"/> does,
    /// since it only needs the persistent player transform to follow.
    /// </summary>
    public class MinimapCameraFollow : MonoBehaviour
    {
        /// <summary>The single persisted minimap camera instance.</summary>
        public static MinimapCameraFollow Instance { get; private set; }

        [SerializeField] private Transform player;
        [SerializeField] private float height = 20f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            transform.position = new Vector3(player.position.x, player.position.y + height, player.position.z);
        }
    }
}
