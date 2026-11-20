using System.Collections.Generic;
using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Darkens and tints unlit effects, such as a waterfall's mist, spray and foam, with the same ambient
    /// tint the map's lighting uses, so they do not glow white at night. Particle start colours are
    /// multiplied for new particles, and the foam colours of mesh renderers are set through a property
    /// block, which leaves the shared materials untouched.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DayNightTint : MonoBehaviour
    {
        private static readonly int[] FoamColorProperties =
        {
            Shader.PropertyToID("_FoamColor"),
            Shader.PropertyToID("_PoolColor")
        };

        [Tooltip("The profile whose ambient tint is applied. Use the same one as the map's lighting.")]
        [SerializeField] private DayNightProfile profile;

        [Tooltip("Seconds between updates of the tint.")]
        [SerializeField, Min(0.05f)] private float refreshSeconds = 0.5f;

        private readonly List<ParticleSystem> systems = new List<ParticleSystem>();
        private readonly List<Color> systemColors = new List<Color>();
        private readonly List<Renderer> foamRenderers = new List<Renderer>();
        private readonly List<Color[]> foamColors = new List<Color[]>();
        private MaterialPropertyBlock block;
        private float nextRefreshTime;

        private void Awake()
        {
            if (profile == null)
            {
                enabled = false;
                return;
            }

            block = new MaterialPropertyBlock();
            CaptureParticles();
            CaptureFoam();
        }

        private void Update()
        {
            if (Time.time < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = Time.time + refreshSeconds;
            Apply(profile.AmbientTint(WorldClock.Normalized));
        }

        private void CaptureParticles()
        {
            foreach (var system in GetComponentsInChildren<ParticleSystem>(true))
            {
                systems.Add(system);
                systemColors.Add(system.main.startColor.color);
            }
        }

        private void CaptureFoam()
        {
            foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>(true))
            {
                var material = meshRenderer.sharedMaterial;

                if (material == null || !material.HasProperty(FoamColorProperties[0]))
                {
                    continue;
                }

                var colors = new Color[FoamColorProperties.Length];

                for (var i = 0; i < colors.Length; i++)
                {
                    colors[i] = material.HasProperty(FoamColorProperties[i]) ? material.GetColor(FoamColorProperties[i]) : Color.white;
                }

                foamRenderers.Add(meshRenderer);
                foamColors.Add(colors);
            }
        }

        private void Apply(Color tint)
        {
            for (var i = 0; i < systems.Count; i++)
            {
                var main = systems[i].main;
                var baseColor = systemColors[i];
                main.startColor = new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, baseColor.a);
            }

            for (var i = 0; i < foamRenderers.Count; i++)
            {
                foamRenderers[i].GetPropertyBlock(block);

                for (var p = 0; p < FoamColorProperties.Length; p++)
                {
                    var baseColor = foamColors[i][p];
                    block.SetColor(FoamColorProperties[p], new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, baseColor.a));
                }

                foamRenderers[i].SetPropertyBlock(block);
            }
        }
    }
}
