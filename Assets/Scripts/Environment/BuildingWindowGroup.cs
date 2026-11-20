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

        [Tooltip("Longest random wait, in seconds, before this building switches at dusk or dawn, so the town does not change all at once.")]
        [SerializeField, Min(0f)] private float transitionSpreadSeconds = 240f;

        private int[] order;
        private Coroutine pendingTransition;

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
            CancelPendingTransition();
            SetLitRatio(lit ? litRatio : 0f);
        }

        /// <summary>
        /// Switches the building after a random wait of up to the configured spread, so neighbouring houses
        /// light up and go dark at different moments.
        /// </summary>
        /// <param name="lit">True at dusk, when windows light up; false at dawn.</param>
        public void TransitionTo(bool lit)
        {
            CancelPendingTransition();
            pendingTransition = StartCoroutine(SwitchAfterDelay(lit, Random.value * transitionSpreadSeconds));
        }

        private System.Collections.IEnumerator SwitchAfterDelay(bool lit, float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            pendingTransition = null;
            SetLitRatio(lit ? litRatio : 0f);
        }

        private void CancelPendingTransition()
        {
            if (pendingTransition != null)
            {
                StopCoroutine(pendingTransition);
                pendingTransition = null;
            }
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
