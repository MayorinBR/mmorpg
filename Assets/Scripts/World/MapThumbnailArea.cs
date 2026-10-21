using UnityEngine;

namespace Project.World
{
    /// <summary>
    /// Marks the square region of a map scene that is photographed from above for its World Map thumbnail. Sits
    /// on the scene's <see cref="MapBootstrap"/> object; its position is the center of the shot and its gizmo
    /// shows the region in the Scene view.
    /// </summary>
    public class MapThumbnailArea : MonoBehaviour
    {
        [Tooltip("Side length in meters of the square region captured around this object's X and Z position.")]
        [SerializeField, Min(1f)] private float sideLength = 100f;

        [Tooltip("Height of the camera above this object's Y position. Set it below a ceiling to see inside a cave or building.")]
        [SerializeField] private float cameraHeight = 100f;

        [Tooltip("How far below the camera things are still drawn. Raise it if the ground is cut off.")]
        [SerializeField, Min(1f)] private float captureDepth = 300f;

        /// <summary>Gets the side length in meters of the captured square.</summary>
        public float SideLength => sideLength;

        /// <summary>Gets the world position the capture camera is placed at, looking straight down.</summary>
        public Vector3 CameraPosition => transform.position + Vector3.up * cameraHeight;

        /// <summary>Gets how far below the camera things are still drawn.</summary>
        public float CaptureDepth => captureDepth;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(sideLength, 0.1f, sideLength));
            Gizmos.DrawWireCube(CameraPosition, new Vector3(sideLength, 0.1f, sideLength));
            Gizmos.DrawLine(transform.position, CameraPosition);
        }
    }
}
