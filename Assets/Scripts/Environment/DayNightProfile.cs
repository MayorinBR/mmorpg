using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Describes how a map's lighting changes over the day and night cycle. Every curve and gradient is read
    /// over the normalized cycle (0 sunrise, 0.25 noon, 0.5 sunset, 0.75 midnight) and works as a multiplier
    /// on the values authored in the scene, so each map keeps its own daytime look and the profile only adds
    /// the dawn, dusk and night mood on top of it.
    /// </summary>
    [CreateAssetMenu(fileName = "DayNightProfile", menuName = "Project/Environment/Day Night Profile")]
    public sealed class DayNightProfile : ScriptableObject
    {
        [Tooltip("Colour multiplier of the main light: white by day, bluish for the moon.")]
        [SerializeField] private Gradient sunColor = BuildSunColor();

        [Tooltip("Intensity multiplier of the main light.")]
        [SerializeField] private AnimationCurve sunIntensity = BuildSunIntensity();

        [Tooltip("Colour multiplier of the ambient light, the fog and the camera background.")]
        [SerializeField] private Gradient ambientTint = BuildAmbientTint();

        [Tooltip("Pitch of the main light, in degrees, when it sits at the horizon.")]
        [SerializeField, Range(0f, 30f)] private float horizonPitch = 4f;

        [Tooltip("Degrees the main light turns sideways between rising and setting.")]
        [SerializeField, Range(0f, 180f)] private float yawSweep = 140f;

        /// <summary>Gets the pitch of the main light at the horizon, in degrees.</summary>
        public float HorizonPitch => horizonPitch;

        /// <summary>Gets the total sideways sweep of the main light between rising and setting, in degrees.</summary>
        public float YawSweep => yawSweep;

        /// <summary>
        /// Gets the colour multiplier of the main light.
        /// </summary>
        /// <param name="normalized">The normalized position in the cycle.</param>
        /// <returns>The colour multiplier.</returns>
        public Color SunColor(float normalized)
        {
            return sunColor.Evaluate(normalized);
        }

        /// <summary>
        /// Gets the intensity multiplier of the main light.
        /// </summary>
        /// <param name="normalized">The normalized position in the cycle.</param>
        /// <returns>The intensity multiplier.</returns>
        public float SunIntensity(float normalized)
        {
            return sunIntensity.Evaluate(normalized);
        }

        /// <summary>
        /// Gets the colour multiplier of the ambient light, the fog and the camera background.
        /// </summary>
        /// <param name="normalized">The normalized position in the cycle.</param>
        /// <returns>The colour multiplier.</returns>
        public Color AmbientTint(float normalized)
        {
            return ambientTint.Evaluate(normalized);
        }

        private static Gradient BuildSunColor()
        {
            var day = new Color(1f, 0.97f, 0.92f);
            var moon = new Color(0.75f, 0.82f, 1f);
            var twilight = Color.Lerp(day, moon, 0.5f);

            return BuildGradient(
                (0f, twilight), (0.06f, day), (0.44f, day), (0.5f, twilight), (0.56f, moon), (0.94f, moon), (1f, twilight));
        }

        private static Gradient BuildAmbientTint()
        {
            var day = Color.white;
            var night = new Color(0.62f, 0.7f, 0.9f);
            var twilight = Color.Lerp(day, night, 0.5f);

            return BuildGradient(
                (0f, twilight), (0.06f, day), (0.44f, day), (0.5f, twilight), (0.56f, night), (0.94f, night), (1f, twilight));
        }

        private static AnimationCurve BuildSunIntensity()
        {
            return new AnimationCurve(
                new Keyframe(0f, 0.4f),
                new Keyframe(0.08f, 1f),
                new Keyframe(0.42f, 1f),
                new Keyframe(0.5f, 0.4f),
                new Keyframe(0.56f, 0.45f),
                new Keyframe(0.75f, 0.65f),
                new Keyframe(0.94f, 0.45f),
                new Keyframe(1f, 0.4f));
        }

        private static Gradient BuildGradient(params (float time, Color color)[] stops)
        {
            var colorKeys = new GradientColorKey[stops.Length];

            for (var i = 0; i < stops.Length; i++)
            {
                colorKeys[i] = new GradientColorKey(stops[i].color, stops[i].time);
            }

            var gradient = new Gradient();
            gradient.SetKeys(colorKeys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return gradient;
        }
    }
}
