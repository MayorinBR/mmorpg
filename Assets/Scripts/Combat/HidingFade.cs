using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Makes the character model, its hair and its equipped items semi-transparent while it is hidden
    /// (Hiding). Drives the <c>_Fade</c> property of the project's toon shaders, which discard a share of the
    /// pixels in a screen-door pattern, through a <see cref="MaterialPropertyBlock"/>, so the shared
    /// materials are never modified and a tint applied by <see cref="StatusTintController"/> is preserved.
    /// </summary>
    public sealed class HidingFade : MonoBehaviour
    {
        private const float RefreshInterval = 0.25f;

        private static readonly int FadeId = Shader.PropertyToID("_Fade");

        [Tooltip("The hidden character whose Hiding state drives the fade.")]
        [SerializeField] private StatusEffectController status;

        [Tooltip("Root to search for renderers under. Left empty, searches this GameObject and its children.")]
        [SerializeField] private Transform modelRoot;

        [Tooltip("Opacity while hidden, from 0 (invisible) to 1 (fully visible).")]
        [SerializeField, Range(0.05f, 1f)] private float hiddenOpacity = 0.4f;

        [Tooltip("How fast the opacity changes, in opacity per second.")]
        [SerializeField, Min(0.1f)] private float fadeSpeed = 3f;

        private readonly List<Renderer> renderers = new List<Renderer>();
        private MaterialPropertyBlock block;
        private float opacity = 1f;
        private float nextRefresh;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
        }

        private void OnDisable()
        {
            opacity = 1f;
            Apply(true);
        }

        private void Update()
        {
            var target = status != null && status.IsHidden ? hiddenOpacity : 1f;
            var changing = !Mathf.Approximately(opacity, target);

            if (!changing && Mathf.Approximately(opacity, 1f))
            {
                return;
            }

            opacity = Mathf.MoveTowards(opacity, target, fadeSpeed * Time.deltaTime);
            Apply(changing || Time.time >= nextRefresh);
        }

        /// <summary>
        /// Writes the current opacity to every renderer, re-collecting them when asked since equipment and hair are
        /// rebuilt at runtime.
        /// </summary>
        /// <param name="refresh">True to look the renderers up again before writing.</param>
        private void Apply(bool refresh)
        {
            if (refresh)
            {
                Collect();
                nextRefresh = Time.time + RefreshInterval;
            }

            var restore = Mathf.Approximately(opacity, 1f);

            foreach (var target in renderers)
            {
                if (target == null)
                {
                    continue;
                }

                target.GetPropertyBlock(block);
                block.SetFloat(FadeId, opacity);
                target.SetPropertyBlock(block);
            }

            if (restore)
            {
                renderers.Clear();
            }
        }

        private void Collect()
        {
            renderers.Clear();
            var root = modelRoot != null ? modelRoot : transform;

            foreach (var candidate in root.GetComponentsInChildren<Renderer>())
            {
                var material = candidate.sharedMaterial;

                if (material != null && material.HasProperty(FadeId))
                {
                    renderers.Add(candidate);
                }
            }
        }
    }
}
