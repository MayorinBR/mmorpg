using UnityEngine;
using UnityEngine.UI;
using Project.Ambience;

namespace Project.UI
{
    /// <summary>
    /// HUD clock for the world's day and night cycle. It rotates a sky disc once per cycle behind a mask that
    /// shows only its upper half, so the sun climbs to the top at noon and the moon at midnight, and tints the
    /// disc with the same colour the world's ambient light uses, bright at noon and dark at midnight.
    /// </summary>
    public sealed class DayNightClockUI : MonoBehaviour
    {
        [Tooltip("The disc that rotates. Its pivot must sit at the centre of the visible half circle's flat edge.")]
        [SerializeField] private RectTransform dial;

        [Tooltip("The image of the disc, tinted to follow the world's lighting.")]
        [SerializeField] private Image dialImage;

        [Tooltip("Gives the tint of the world's lighting. Leave empty to keep the disc untinted.")]
        [SerializeField] private DayNightProfile profile;

        [Tooltip("How much of the world's tint is applied to the disc, from none to all of it.")]
        [SerializeField, Range(0f, 1f)] private float tintStrength = 1f;

        [Tooltip("Extra rotation of the disc in degrees, to line the artwork up with the window.")]
        [SerializeField] private float angleOffset;

        private void Update()
        {
            var normalized = WorldClock.Normalized;
            dial.localEulerAngles = new Vector3(0f, 0f, angleOffset - normalized * 360f);

            if (profile != null)
            {
                dialImage.color = Color.Lerp(Color.white, profile.AmbientTint(normalized), tintStrength);
            }
        }
    }
}
