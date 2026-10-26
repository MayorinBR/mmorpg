using UnityEngine;
using Project.Combat;

namespace Project.UI
{
    /// <summary>
    /// Lives on an enemy's root and owns its two ground rings: a white one
    /// shown while the cursor hovers the enemy, and a red one shown while
    /// the enemy is the player's current combat target. The red ring takes
    /// precedence. The rings are children of the root, not of the animated
    /// geometry, so they stay on the ground while the body bounces or
    /// animates. Size and height are tuned per prefab in the Inspector, in
    /// world units.
    /// </summary>
    public class TargetRingMarker : MonoBehaviour
    {
        private const int Segments = 48;
        private const float InnerRatio = 0.85f;
        private const string HoverMaterialPath = "Assets/Materials/UI/TargetRingHover.mat";
        private const string TargetMaterialPath = "Assets/Materials/UI/TargetRingTarget.mat";

        private static Mesh sharedRingMesh;
        private static Material sharedSkillMaterial;

        [Tooltip("Outer radius of the ring, in world units.")]
        [SerializeField] private float radius = 0.75f;

        [Tooltip("Height of the rings above this object's pivot, in world units. Raise it if the ground hides part of the ring.")]
        [SerializeField] private float groundOffset = 0.05f;

        [SerializeField] private Material hoverMaterial;
        [SerializeField] private Material targetMaterial;

        private GameObject hoverRing;
        private GameObject targetRing;
        private GameObject skillRing;
        private bool isSkillPicked;
        private IDamageable damageable;
        private bool isHovered;
        private bool isTargeted;

#if UNITY_EDITOR
        /// <summary>
        /// Fills the default ring materials when the component is added or reset
        /// in the Inspector, so a new enemy prefab works without manual wiring.
        /// </summary>
        private void Reset()
        {
            hoverMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(HoverMaterialPath);
            targetMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(TargetMaterialPath);
        }
#endif

        private void Awake()
        {
            damageable = GetComponentInParent<IDamageable>();
            hoverRing = CreateRing("HoverRing", hoverMaterial);
            targetRing = CreateRing("TargetRing", targetMaterial);
            skillRing = CreateRing("SkillRing", GetSkillMaterial());
        }

        private void OnDisable()
        {
            isHovered = false;
            isTargeted = false;
            isSkillPicked = false;
            Refresh();
        }

        private void LateUpdate()
        {
            if ((isHovered || isTargeted || isSkillPicked) && damageable != null && damageable.IsDead)
            {
                isHovered = false;
                isTargeted = false;
                isSkillPicked = false;
                Refresh();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            var center = transform.position + Vector3.up * groundOffset;
            var previous = center + new Vector3(radius, 0f, 0f);

            for (var i = 1; i <= Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;
                var next = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }

        /// <summary>Shows or hides the white hover ring.</summary>
        /// <param name="value">True while the cursor is over this enemy.</param>
        public void SetHovered(bool value)
        {
            isHovered = value;
            Refresh();
        }

        /// <summary>Shows or hides the red current-target ring.</summary>
        /// <param name="value">True while this enemy is the player's combat target.</param>
        public void SetTargeted(bool value)
        {
            isTargeted = value;
            Refresh();
        }

        /// <summary>Shows or hides the yellow ring used while the player is picking a target for a skill. It wins over the other rings.</summary>
        /// <param name="value">True while the cursor is over this enemy during a skill target pick.</param>
        public void SetSkillPicked(bool value)
        {
            isSkillPicked = value;
            Refresh();
        }

        private void Refresh()
        {
            if (skillRing != null)
            {
                skillRing.SetActive(isSkillPicked);
            }

            if (targetRing != null)
            {
                targetRing.SetActive(isTargeted && !isSkillPicked);
            }

            if (hoverRing != null)
            {
                hoverRing.SetActive(isHovered && !isTargeted && !isSkillPicked);
            }
        }

        private Material GetSkillMaterial()
        {
            if (sharedSkillMaterial != null)
            {
                return sharedSkillMaterial;
            }

            var source = hoverMaterial != null ? hoverMaterial : targetMaterial;

            if (source == null)
            {
                return null;
            }

            var yellow = new Color(1f, 0.85f, 0.1f, 1f);
            sharedSkillMaterial = new Material(source) { name = "TargetRingSkill (runtime)" };

            if (sharedSkillMaterial.HasProperty("_BaseColor"))
            {
                sharedSkillMaterial.SetColor("_BaseColor", yellow);
            }

            if (sharedSkillMaterial.HasProperty("_Color"))
            {
                sharedSkillMaterial.SetColor("_Color", yellow);
            }

            return sharedSkillMaterial;
        }

        private GameObject CreateRing(string ringName, Material material)
        {
            var ring = new GameObject(ringName);
            var ringTransform = ring.transform;
            ringTransform.SetParent(transform, false);

            // Compensates a scaled root so the Inspector values stay in world units.
            var scale = transform.lossyScale;
            ringTransform.localPosition = new Vector3(0f, groundOffset / Mathf.Max(scale.y, 0.0001f), 0f);
            ringTransform.localScale = new Vector3(radius / Mathf.Max(scale.x, 0.0001f), 1f, radius / Mathf.Max(scale.z, 0.0001f));

            ring.AddComponent<MeshFilter>().sharedMesh = GetRingMesh();
            var meshRenderer = ring.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;

            ring.SetActive(false);
            return ring;
        }

        private static Mesh GetRingMesh()
        {
            if (sharedRingMesh != null)
            {
                return sharedRingMesh;
            }

            var vertices = new Vector3[(Segments + 1) * 2];
            var normals = new Vector3[vertices.Length];
            var triangles = new int[Segments * 6];

            for (var i = 0; i <= Segments; i++)
            {
                var angle = i * Mathf.PI * 2f / Segments;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[i * 2] = direction;
                vertices[i * 2 + 1] = direction * InnerRatio;
                normals[i * 2] = Vector3.up;
                normals[i * 2 + 1] = Vector3.up;
            }

            for (var i = 0; i < Segments; i++)
            {
                var outerA = i * 2;
                var innerA = outerA + 1;
                var outerB = outerA + 2;
                var innerB = outerA + 3;
                var t = i * 6;

                triangles[t] = outerA;
                triangles[t + 1] = innerA;
                triangles[t + 2] = outerB;
                triangles[t + 3] = innerA;
                triangles[t + 4] = innerB;
                triangles[t + 5] = outerB;
            }

            sharedRingMesh = new Mesh { name = "TargetRing" };
            sharedRingMesh.vertices = vertices;
            sharedRingMesh.normals = normals;
            sharedRingMesh.triangles = triangles;
            return sharedRingMesh;
        }
    }
}
