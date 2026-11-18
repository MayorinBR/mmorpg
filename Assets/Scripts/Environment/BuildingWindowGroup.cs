using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Controls all the windows of one building. Switching the group on lights a stable, per-building subset of the
    /// windows, so each house shows a different pattern and the pattern grows consistently as the ratio rises.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class BuildingWindowGroup : MonoBehaviour, ILightSwitchable
    {
        [Tooltip("Windows of the building. Filled automatically from the children when left empty.")]
        [SerializeField] private WindowLight[] windows;

        [Tooltip("Fraction of the windows that light up when the group is switched on.")]
        [SerializeField, Range(0f, 1f)] private float litRatio = 0.7f;

        [Tooltip("Seed of the lighting pattern. Zero derives the seed from the building position.")]
        [SerializeField] private int seed;

        private int[] order;

        /// <inheritdoc />
        public bool IsLit { get; private set; }

        private void Awake()
        {
            if (windows == null || windows.Length == 0)
            {
                windows = GetComponentsInChildren<WindowLight>(true);
            }

            order = BuildOrder(windows.Length, seed != 0 ? seed : PositionSeed());
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
            SetLitRatio(lit ? litRatio : 0f);
        }

        /// <summary>
        /// Lights the given fraction of the windows and turns the rest off.
        /// </summary>
        /// <param name="ratio">Fraction of lit windows, clamped between 0 and 1.</param>
        public void SetLitRatio(float ratio)
        {
            ratio = Mathf.Clamp01(ratio);
            IsLit = ratio > 0f;

            var litCount = Mathf.RoundToInt(windows.Length * ratio);
            for (var i = 0; i < order.Length; i++)
            {
                windows[order[i]].SetLit(i < litCount);
            }
        }

        private int PositionSeed()
        {
            var p = transform.position;
            return Mathf.RoundToInt(p.x * 73f) ^ Mathf.RoundToInt(p.z * 151f) ^ 0x5bd1e995;
        }

        private static int[] BuildOrder(int count, int orderSeed)
        {
            var result = new int[count];
            for (var i = 0; i < count; i++)
            {
                result[i] = i;
            }

            var random = new System.Random(orderSeed);
            for (var i = count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            return result;
        }
    }
}
