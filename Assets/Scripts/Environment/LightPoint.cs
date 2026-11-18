using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// A standalone light point such as a lamp post, torch or lantern. Lamps are always on by default, and the
    /// flag can be cleared on any prefab that should follow the day and night cycle instead.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LightPoint : MonoBehaviour, ILightSwitchable
    {
        [Tooltip("Lights driven by this point. Filled automatically from the children when left empty.")]
        [SerializeField] private Light[] lights;

        [Tooltip("When set, the point stays lit and ignores switch requests.")]
        [SerializeField] private bool alwaysOn = true;

        /// <inheritdoc />
        public bool IsLit { get; private set; }

        private void Awake()
        {
            if (lights == null || lights.Length == 0)
            {
                lights = GetComponentsInChildren<Light>(true);
            }

            if (alwaysOn)
            {
                Apply(true);
            }
            else
            {
                IsLit = lights.Length > 0 && lights[0].enabled;
            }
        }

        private void OnEnable()
        {
            LightRegistry.Register(this);
        }

        private void OnDisable()
        {
            LightRegistry.Unregister(this);
        }

        /// <inheritdoc />
        public void SetLit(bool lit)
        {
            Apply(alwaysOn || lit);
        }

        private void Apply(bool lit)
        {
            IsLit = lit;
            for (var i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = lit;
            }
        }
    }
}
