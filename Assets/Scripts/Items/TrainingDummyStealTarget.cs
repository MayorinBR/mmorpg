using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Makes a training dummy stealable for testing the Steal skill: it holds one item that can be taken once,
    /// then becomes stealable again after <see cref="respawnSeconds"/>.
    /// </summary>
    public class TrainingDummyStealTarget : MonoBehaviour, IStealTarget
    {
        [Tooltip("Item handed over by a successful Steal.")]
        [SerializeField] private ItemDefinition item;

        [Tooltip("Seconds after a successful Steal before the dummy can be stolen from again.")]
        [SerializeField] private float respawnSeconds = 30f;

        private float stolenAt = float.NegativeInfinity;

        /// <inheritdoc />
        public bool CanBeStolenFrom => item != null && Time.time - stolenAt >= respawnSeconds;

        /// <inheritdoc />
        public ItemDefinition Steal()
        {
            stolenAt = Time.time;
            return item;
        }
    }
}
