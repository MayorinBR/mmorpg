using UnityEngine;
using Project.CameraSystem;

namespace Project.UI
{
    /// <summary>
    /// Rotates this Transform every frame to face the active camera, leaving
    /// position untouched. For objects whose position is already handled by
    /// ordinary Unity parenting (e.g. <c>WarpLabel</c>, a true child of its
    /// <c>WarpPortal</c>) rather than a follower script. Resolves
    /// <see cref="IsometricCameraController.Instance"/> itself, so it works
    /// with no external wiring on scene-authored objects and prefab
    /// instances alike.
    /// </summary>
    public class BillboardToCamera : MonoBehaviour
    {
        private void LateUpdate()
        {
            var activeCamera = IsometricCameraController.Instance;
            if (activeCamera == null)
            {
                return;
            }

            transform.forward = activeCamera.transform.forward;
        }
    }
}
