using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// A single window pane that can glow at night. The pane swaps between two shared materials, which keeps
    /// batching intact, and can optionally drive a real <see cref="Light"/> for windows close to the player.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class WindowLight : MonoBehaviour, ILightSwitchable
    {
        [Tooltip("Renderer of the glass pane.")]
        [SerializeField] private Renderer pane;

        [Tooltip("Material used while the window is dark.")]
        [SerializeField] private Material offMaterial;

        [Tooltip("Emissive material used while the window is lit.")]
        [SerializeField] private Material litMaterial;

        [Tooltip("Optional real light enabled together with the pane.")]
        [SerializeField] private Light windowLight;

        [Tooltip("Whether the window starts lit when the scene loads.")]
        [SerializeField] private bool litOnStart;

        /// <inheritdoc />
        public bool IsLit { get; private set; }

        private void Awake()
        {
            SetLit(litOnStart);
        }

        /// <inheritdoc />
        public void SetLit(bool lit)
        {
            IsLit = lit;

            if (pane != null)
            {
                pane.sharedMaterial = lit ? litMaterial : offMaterial;
            }

            if (windowLight != null)
            {
                windowLight.enabled = lit;
            }
        }
    }
}
