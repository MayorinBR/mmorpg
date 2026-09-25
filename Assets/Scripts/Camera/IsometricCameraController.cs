using UnityEngine;

namespace Project.CameraSystem
{
    /// <summary>
    /// Follows a target using a fixed pitch angle (classic isometric look)
    /// while allowing horizontal orbit (yaw) and zoom. Position and rotation
    /// are smoothed independently from whatever reads player input, so the
    /// camera behaves the same whether driven by mouse, gamepad, or code.
    /// Persists across scene loads exactly like <see cref="Project.World.PersistentPlayerAnchor"/>,
    /// so a per-map <c>MapBootstrap</c> only needs to re-target it, never
    /// recreate it — this is what keeps rotate/zoom input and the click/hover
    /// raycast camera valid after a map switch. If a map scene still has its
    /// own leftover instance, it self-destructs here as a duplicate, the same
    /// guard <see cref="Project.World.PersistentPlayerAnchor"/> uses.
    /// </summary>
    public class IsometricCameraController : MonoBehaviour, ICameraYawProvider
    {
        [SerializeField] private Transform target;
        [SerializeField] private float pitchAngle = 45f;
        [SerializeField] private float distance = 10f;
        [SerializeField] private float minDistance = 5f;
        [SerializeField] private float maxDistance = 15f;
        [SerializeField] private float positionSmoothTime = 0.15f;
        [SerializeField] private float rotationSpeed = 120f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float zoomSmoothTime = 0.15f;

        private float currentYaw;
        private float rotationInput;
        private float zoomInput;
        private float targetDistance;
        private float distanceVelocity;
        private Vector3 smoothedTargetPosition;
        private Vector3 positionVelocity;

        /// <inheritdoc />
        public float CurrentYaw => currentYaw;

        /// <summary>
        /// The single persisted camera instance. Null until the camera's
        /// first Awake has run.
        /// </summary>
        public static IsometricCameraController Instance { get; private set; }

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

        private void Start()
        {
            targetDistance = distance;

            if (target != null)
            {
                smoothedTargetPosition = target.position;
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            currentYaw += rotationInput * rotationSpeed * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance - (zoomInput * zoomSpeed * Time.deltaTime), minDistance, maxDistance);
            distance = Mathf.SmoothDamp(distance, targetDistance, ref distanceVelocity, zoomSmoothTime);

            smoothedTargetPosition = Vector3.SmoothDamp(smoothedTargetPosition, target.position, ref positionVelocity, positionSmoothTime);

            var rotation = Quaternion.Euler(pitchAngle, currentYaw, 0f);
            transform.position = smoothedTargetPosition + (rotation * Vector3.back * distance);
            transform.rotation = rotation;
        }

        /// <summary>
        /// Sets the current rotation input, applied continuously every frame
        /// until changed again. Typically called from an Input System
        /// callback for a stick or mouse delta.
        /// </summary>
        /// <param name="normalizedInput">Rotation input in the range [-1, 1].</param>
        public void SetRotationInput(float normalizedInput)
        {
            rotationInput = normalizedInput;
        }

        /// <summary>
        /// Sets the current zoom input, applied continuously every frame
        /// until changed again. The displayed distance eases toward the
        /// resulting target distance rather than snapping to it.
        /// </summary>
        /// <param name="normalizedInput">Zoom input, positive to zoom in, negative to zoom out.</param>
        public void SetZoomInput(float normalizedInput)
        {
            zoomInput = normalizedInput;
        }

        /// <summary>
        /// Assigns the transform the camera should follow.
        /// </summary>
        /// <param name="newTarget">The target transform, typically the player character.</param>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }
    }
}