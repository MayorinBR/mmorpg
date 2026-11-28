using UnityEngine;
using UnityEngine.Rendering;

namespace Project.Character.Movement
{
    /// <summary>
    /// Shows an animated marker on the ground point the player clicked to walk
    /// to: a ring that pops in and pulses, and an arrow that drops in, bobs and
    /// spins above it. Only plain ground clicks show it (not chasing an enemy,
    /// walking to loot or to an NPC). It hides once the character arrives, the
    /// path is cancelled (directional input, combat, warp) or the agent heads
    /// somewhere else.
    /// </summary>
    public class MoveDestinationMarker : MonoBehaviour
    {
        private const int RingSegments = 40;
        private const int ConeSegments = 12;
        private const float RingInnerRatio = 0.78f;
        private const float DestinationTolerance = 2.5f;
        private const float ArrowDropHeight = 1.2f;
        private const int PathSettleFrames = 2;

        private static Mesh ringMesh;
        private static Mesh coneMesh;

        [SerializeField] private PlayerInputRouter inputRouter;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private Material markerMaterial;

        [Tooltip("Outer radius of the ground ring, in world units.")]
        [SerializeField] private float ringRadius = 0.45f;

        [Tooltip("Extra ring scale added and removed by the idle pulse, as a fraction of the radius.")]
        [SerializeField] private float ringPulse = 0.08f;

        [Tooltip("Width and height of the arrow, in world units.")]
        [SerializeField] private float arrowSize = 0.3f;

        [Tooltip("Height of the arrow tip above the ground at rest, in world units.")]
        [SerializeField] private float arrowHeight = 0.6f;

        [Tooltip("Up and down travel of the idle bob, in world units.")]
        [SerializeField] private float arrowBob = 0.12f;

        [SerializeField] private float arrowSpinDegreesPerSecond = 180f;

        [Tooltip("Seconds the ring and the arrow take to settle after a click.")]
        [SerializeField] private float popDuration = 0.25f;

        [Tooltip("Height of the ring above the clicked point, so the ground does not hide it.")]
        [SerializeField] private float groundOffset = 0.05f;

        private Transform root;
        private Transform ring;
        private Transform arrow;
        private Vector3 destination;
        private float shownAt;
        private int shownFrame;

        private void Awake()
        {
            root = new GameObject("MoveDestinationMarker").transform;
            DontDestroyOnLoad(root.gameObject);
            ring = CreatePart("Ring", GetRingMesh());
            arrow = CreatePart("Arrow", GetConeMesh());
            arrow.localScale = new Vector3(arrowSize, arrowSize * 1.4f, arrowSize);
            root.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            inputRouter.GroundClicked += Show;
        }

        private void OnDisable()
        {
            inputRouter.GroundClicked -= Show;

            if (root != null)
            {
                root.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (root != null)
            {
                Destroy(root.gameObject);
            }
        }

        private void LateUpdate()
        {
            if (!root.gameObject.activeSelf)
            {
                return;
            }

            var pathSettled = Time.frameCount - shownFrame >= PathSettleFrames;

            if (pathSettled && !movementController.IsHeadingTo(destination, DestinationTolerance))
            {
                root.gameObject.SetActive(false);
                return;
            }

            var elapsed = Time.time - shownAt;
            var pop = Mathf.Clamp01(elapsed / popDuration);
            var ease = 1f - (1f - pop) * (1f - pop) * (1f - pop);

            var ringScale = ringRadius * (Mathf.Lerp(1.8f, 1f, ease) + ringPulse * Mathf.Sin(elapsed * 6f) * pop);
            ring.localScale = new Vector3(ringScale, 1f, ringScale);

            var arrowY = Mathf.Lerp(arrowHeight + ArrowDropHeight, arrowHeight, ease) + arrowBob * Mathf.Sin(elapsed * 5f) * pop;
            arrow.localPosition = new Vector3(0f, arrowY, 0f);
            arrow.localRotation = Quaternion.Euler(0f, elapsed * arrowSpinDegreesPerSecond, 0f);
        }

        private void Show(Vector3 point)
        {
            destination = point;
            shownAt = Time.time;
            shownFrame = Time.frameCount;
            root.position = point + Vector3.up * groundOffset;
            root.gameObject.SetActive(true);
            LateUpdate();
        }

        private Transform CreatePart(string partName, Mesh mesh)
        {
            var part = new GameObject(partName);
            part.transform.SetParent(root, false);
            part.AddComponent<MeshFilter>().sharedMesh = mesh;

            var meshRenderer = part.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = markerMaterial;
            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            return part.transform;
        }

        private static Mesh GetRingMesh()
        {
            if (ringMesh != null)
            {
                return ringMesh;
            }

            var vertices = new Vector3[(RingSegments + 1) * 2];
            var triangles = new int[RingSegments * 6];

            for (var i = 0; i <= RingSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / RingSegments;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[i * 2] = direction;
                vertices[i * 2 + 1] = direction * RingInnerRatio;
            }

            for (var i = 0; i < RingSegments; i++)
            {
                var outer = i * 2;
                var t = i * 6;
                triangles[t] = outer;
                triangles[t + 1] = outer + 1;
                triangles[t + 2] = outer + 2;
                triangles[t + 3] = outer + 1;
                triangles[t + 4] = outer + 3;
                triangles[t + 5] = outer + 2;
            }

            ringMesh = new Mesh { name = "MoveMarkerRing", vertices = vertices, triangles = triangles };
            ringMesh.RecalculateNormals();
            return ringMesh;
        }

        /// <summary>Cone pointing down: tip at the origin, unit-radius base at y = 1.</summary>
        private static Mesh GetConeMesh()
        {
            if (coneMesh != null)
            {
                return coneMesh;
            }

            var vertices = new Vector3[ConeSegments + 2];
            var triangles = new int[ConeSegments * 6];
            var cap = ConeSegments + 1;
            vertices[cap] = Vector3.up;

            for (var i = 0; i < ConeSegments; i++)
            {
                var angle = i * Mathf.PI * 2f / ConeSegments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * 0.5f, 1f, Mathf.Sin(angle) * 0.5f);
            }

            for (var i = 0; i < ConeSegments; i++)
            {
                var current = i + 1;
                var next = (i + 1) % ConeSegments + 1;
                var t = i * 6;
                triangles[t] = 0;
                triangles[t + 1] = next;
                triangles[t + 2] = current;
                triangles[t + 3] = cap;
                triangles[t + 4] = current;
                triangles[t + 5] = next;
            }

            coneMesh = new Mesh { name = "MoveMarkerArrow", vertices = vertices, triangles = triangles };
            coneMesh.RecalculateNormals();
            return coneMesh;
        }
    }
}
