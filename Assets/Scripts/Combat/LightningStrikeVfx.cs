using System.Collections;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Draws one or more jagged lightning bolts falling from the sky onto the ground around the effect's position,
    /// emitting sparks where each one lands. One strike on the effect's center makes a single-target bolt; several
    /// strikes spread across an area make a storm.
    /// </summary>
    public class LightningStrikeVfx : MonoBehaviour
    {
        private const int SegmentCount = 12;

        [Tooltip("How many bolts fall during the effect. The first always lands on the center.")]
        [SerializeField] private int strikeCount = 1;

        [Tooltip("Radius, in meters, around the center that bolts after the first can land in.")]
        [SerializeField] private float areaRadius;

        [Tooltip("Seconds between the first and the last bolt.")]
        [SerializeField] private float totalDurationSeconds = 0.5f;

        [Tooltip("Height, in meters, the bolts start from.")]
        [SerializeField] private float skyHeight = 10f;

        [Tooltip("Seconds each bolt stays visible.")]
        [SerializeField] private float boltLifetimeSeconds = 0.2f;

        [SerializeField] private float boltWidth = 0.18f;

        [Tooltip("Maximum sideways offset, in meters, of the bolt's zigzag.")]
        [SerializeField] private float jitter = 0.6f;

        [SerializeField] private Material boltMaterial;
        [SerializeField] private Color boltColor = new Color(0.75f, 0.88f, 1f, 1f);

        [Tooltip("Optional world-space particle system emitted at each landing point.")]
        [SerializeField] private ParticleSystem groundSparks;

        [SerializeField] private int sparksPerStrike = 14;

        private void Start()
        {
            StartCoroutine(StrikeRoutine());
        }

        private IEnumerator StrikeRoutine()
        {
            var interval = strikeCount > 1 ? totalDurationSeconds / (strikeCount - 1) : 0f;

            for (var i = 0; i < strikeCount; i++)
            {
                var offset = i == 0 ? Vector2.zero : Random.insideUnitCircle * areaRadius;
                Strike(transform.position + new Vector3(offset.x, 0f, offset.y));

                if (i < strikeCount - 1)
                {
                    yield return new WaitForSeconds(interval * Random.Range(0.5f, 1.5f));
                }
            }
        }

        /// <summary>
        /// Draws a single bolt ending at <paramref name="ground"> and emits the landing sparks.
        /// </summary>
        private void Strike(Vector3 ground)
        {
            var bolt = new GameObject("LightningBolt");
            bolt.transform.SetParent(transform, false);

            var line = bolt.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = SegmentCount + 1;
            line.widthMultiplier = boltWidth;
            line.startColor = boltColor;
            line.endColor = boltColor;
            line.sharedMaterial = boltMaterial;
            line.numCapVertices = 2;

            for (var i = 0; i <= SegmentCount; i++)
            {
                var t = i / (float)SegmentCount;
                var sway = Mathf.Sin(t * Mathf.PI) * jitter;
                var sideways = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * sway;
                line.SetPosition(i, ground + Vector3.up * Mathf.Lerp(skyHeight, 0.05f, t) + sideways);
            }

            Destroy(bolt, boltLifetimeSeconds);

            if (groundSparks != null)
            {
                groundSparks.Emit(new ParticleSystem.EmitParams { position = ground + Vector3.up * 0.1f }, sparksPerStrike);
            }
        }
    }
}
