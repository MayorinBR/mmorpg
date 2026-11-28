using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Project.World
{
    /// <summary>
    /// Draws a Ragnarok Online style warp portal on the ground: concentric rings of light that pulse at
    /// different rates, spin in alternating directions and slowly collapse from the outer edge to the centre.
    /// A ring that reaches the centre fades out and comes back at the outer edge, so the flow never stops.
    /// </summary>
    /// <remarks>
    /// The rings are drawn with <see cref="Graphics.RenderMesh"/>, so the effect adds no child objects and
    /// also shows in the Scene view. Enable "Always Refresh" in the Scene view to see it animate outside Play Mode.
    /// </remarks>
    [ExecuteAlways]
    public sealed class WarpPortalVfx : MonoBehaviour
    {
        private const int RingSegments = 72;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

        [Tooltip("Additive material whose texture is a soft round dot, such as VfxAdditive.")]
        [SerializeField] private Material material;
        [SerializeField] private Color color = new Color(0.55f, 0.5f, 1f, 1f);
        [SerializeField, Min(0f)] private float brightness = 1f;

        [Tooltip("Kept even so neighbouring rings always spin in opposite directions, even when a ring wraps from the centre back to the edge.")]
        [SerializeField, Min(2)] private int ringCount = 6;
        [SerializeField, Min(0.1f)] private float outerRadius = 1.6f;
        [SerializeField, Min(0f)] private float innerRadius = 0.15f;
        [Tooltip("Width of each ring as a fraction of its radius.")]
        [SerializeField, Range(0.05f, 0.6f)] private float ringWidth = 0.14f;

        [Tooltip("Seconds a ring takes to travel from the outer edge to the centre.")]
        [SerializeField, Min(0.5f)] private float collapseSeconds = 7f;
        [SerializeField] private float spinDegreesPerSecond = 45f;
        [SerializeField] private Vector2 pulseSpeedRange = new Vector2(1.5f, 4.5f);
        [SerializeField, Range(0f, 1f)] private float pulseDepth = 0.65f;

        [SerializeField, Min(0)] private int sparklesPerRing = 1;
        [SerializeField, Min(0f)] private float sparkleSize = 0.14f;
        [SerializeField, Range(0f, 1f)] private float baseGlow = 0.1f;
        [SerializeField] private float groundOffset = 0.04f;
        [SerializeField] private int seed = 7;

        private Ring[] rings;
        private Mesh glowMesh;
        private MaterialPropertyBlock block;
        private bool rebuildPending;

        private struct Ring
        {
            public Mesh Mesh;
            public float StartAngle;
            public float SpinFactor;
            public float PulseSpeed;
            public float PulsePhase;
        }

        private void OnEnable()
        {
            Build();
        }

        private void OnDisable()
        {
            Release();
        }

        private void OnValidate()
        {
            ringCount += ringCount % 2;
            innerRadius = Mathf.Min(innerRadius, outerRadius);
            rebuildPending = true;
        }

        private void Update()
        {
            if (rebuildPending)
            {
                Release();
                Build();
            }

            if (material == null || rings == null)
            {
                return;
            }

            var time = Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
            var origin = transform.position + Vector3.up * groundOffset;
            var renderParams = new RenderParams(material)
            {
                layer = gameObject.layer,
                shadowCastingMode = ShadowCastingMode.Off,
                receiveShadows = false,
                matProps = block
            };

            block.SetColor(BaseColorId, Tint(baseGlow * (0.85f + 0.15f * Mathf.Sin(time * 1.3f))));
            Graphics.RenderMesh(renderParams, glowMesh, 0, Matrix4x4.TRS(origin, transform.rotation, Vector3.one * outerRadius * 1.25f));

            for (var i = 0; i < rings.Length; i++)
            {
                var ring = rings[i];
                var progress = Mathf.Repeat(time / collapseSeconds + i / (float)rings.Length, 1f);
                var radius = Mathf.Lerp(outerRadius, innerRadius, progress);
                var fade = Mathf.SmoothStep(0f, 1f, progress / 0.15f) * (1f - Mathf.SmoothStep(0f, 1f, (progress - 0.75f) / 0.25f));
                var pulse = 1f - pulseDepth + pulseDepth * (0.5f + 0.5f * Mathf.Sin(time * ring.PulseSpeed + ring.PulsePhase));
                var direction = i % 2 == 0 ? 1f : -1f;
                var angle = ring.StartAngle + direction * spinDegreesPerSecond * ring.SpinFactor * time;

                block.SetColor(BaseColorId, Tint(fade * pulse));
                var rotation = transform.rotation * Quaternion.Euler(0f, angle, 0f);
                Graphics.RenderMesh(renderParams, ring.Mesh, 0, Matrix4x4.TRS(origin, rotation, Vector3.one * radius));
            }
        }

        private Color Tint(float alpha)
        {
            return new Color(color.r * brightness, color.g * brightness, color.b * brightness, Mathf.Clamp01(alpha));
        }

        private void Build()
        {
            rebuildPending = false;
            block ??= new MaterialPropertyBlock();
            glowMesh = BuildQuadMesh();

            var random = new System.Random(seed);
            rings = new Ring[ringCount];

            for (var i = 0; i < rings.Length; i++)
            {
                rings[i] = new Ring
                {
                    Mesh = BuildRingMesh(random, ringWidth, sparklesPerRing, sparkleSize / outerRadius),
                    StartAngle = Range(random, 0f, 360f),
                    SpinFactor = Range(random, 0.8f, 1.2f),
                    PulseSpeed = Range(random, pulseSpeedRange.x, pulseSpeedRange.y),
                    PulsePhase = Range(random, 0f, Mathf.PI * 2f)
                };
            }
        }

        private void Release()
        {
            DestroyMesh(glowMesh);
            glowMesh = null;

            if (rings == null)
            {
                return;
            }

            foreach (var ring in rings)
            {
                DestroyMesh(ring.Mesh);
            }

            rings = null;
        }

        private static void DestroyMesh(Mesh mesh)
        {
            if (mesh == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(mesh);
            }
            else
            {
                DestroyImmediate(mesh);
            }
        }

        private static float Range(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        /// <summary>
        /// Builds a flat ring of radius 1 whose brightness peaks in the middle of its width and varies around
        /// its circumference, so its spin is visible. Sparkles sit on the ring and spin with it.
        /// </summary>
        private static Mesh BuildRingMesh(System.Random random, float width, int sparkles, float sparkleSize)
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var triangles = new List<int>();

            var lobeCount = random.Next(2, 5);
            var lobePhase = Range(random, 0f, Mathf.PI * 2f);
            var radii = new[] { 1f - width, 1f - width * 0.5f, 1f };
            var centreUv = new Vector2(0.5f, 0.5f);

            for (var s = 0; s <= RingSegments; s++)
            {
                var angle = s / (float)RingSegments * Mathf.PI * 2f;
                var lobe = 0.35f + 0.65f * Mathf.Pow(0.5f + 0.5f * Mathf.Sin(angle * lobeCount + lobePhase), 2f);
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                for (var k = 0; k < radii.Length; k++)
                {
                    vertices.Add(direction * radii[k]);
                    uvs.Add(centreUv);
                    colors.Add(new Color(1f, 1f, 1f, k == 1 ? lobe : 0f));
                }

                if (s == RingSegments)
                {
                    break;
                }

                for (var k = 0; k < radii.Length - 1; k++)
                {
                    var a = s * radii.Length + k;
                    var b = a + radii.Length;
                    triangles.AddRange(new[] { a, b, a + 1, a + 1, b, b + 1 });
                }
            }

            for (var i = 0; i < sparkles; i++)
            {
                var angle = Range(random, 0f, Mathf.PI * 2f);
                var centre = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * radii[1];
                AddQuad(vertices, uvs, colors, triangles, centre, sparkleSize, sparkleSize);
                AddQuad(vertices, uvs, colors, triangles, centre, sparkleSize * 3f, sparkleSize * 0.3f);
                AddQuad(vertices, uvs, colors, triangles, centre, sparkleSize * 0.3f, sparkleSize * 3f);
            }

            var mesh = new Mesh { name = "WarpPortalRing", hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh BuildQuadMesh()
        {
            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var colors = new List<Color>();
            var triangles = new List<int>();
            AddQuad(vertices, uvs, colors, triangles, Vector3.zero, 1f, 1f);

            var mesh = new Mesh { name = "WarpPortalGlow", hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetColors(colors);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddQuad(List<Vector3> vertices, List<Vector2> uvs, List<Color> colors, List<int> triangles, Vector3 centre, float halfX, float halfZ)
        {
            var start = vertices.Count;
            vertices.Add(centre + new Vector3(-halfX, 0f, -halfZ));
            vertices.Add(centre + new Vector3(-halfX, 0f, halfZ));
            vertices.Add(centre + new Vector3(halfX, 0f, halfZ));
            vertices.Add(centre + new Vector3(halfX, 0f, -halfZ));
            uvs.AddRange(new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) });
            colors.AddRange(new[] { Color.white, Color.white, Color.white, Color.white });
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }
    }
}
