using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// A standalone light point such as a lamp post, torch or lantern. By default it follows the day and night
    /// cycle, lighting at night and going dark by day; set the always-on flag on lamps, fixtures and interior
    /// lights that must stay lit.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LightPoint : MonoBehaviour, ILightSwitchable
    {
        [Tooltip("Lights driven by this point. Filled automatically from the children when left empty.")]
        [SerializeField] private Light[] lights;

        [Tooltip("When set, the point stays lit and ignores switch requests.")]
        [SerializeField] private bool alwaysOn;

        /// <inheritdoc />
        public bool IsLit { get; private set; }

        [Tooltip("Range multiplier applied while the light is lit at night, so lamps reach further in the dark.")]
        [SerializeField, Min(1f)] private float nightRangeScale = 1.8f;

        private float[] baseRanges;

        private void Awake()
        {
            if (lights == null || lights.Length == 0)
            {
                lights = GetComponentsInChildren<Light>(true);
            }

            baseRanges = new float[lights.Length];
            for (var i = 0; i < lights.Length; i++)
            {
                baseRanges[i] = lights[i].range;
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
            var rangeScale = lit && WorldClock.IsNight ? nightRangeScale : 1f;
            for (var i = 0; i < lights.Length; i++)
            {
                lights[i].enabled = lit;
                lights[i].range = baseRanges[i] * rangeScale;
            }
        }
    }
}
