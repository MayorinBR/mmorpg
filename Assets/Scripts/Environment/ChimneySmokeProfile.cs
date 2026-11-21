using System;
using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Look of the chimney smoke for each intensity level. One shared asset drives every chimney,
    /// so the whole town can be tuned in one place.
    /// </summary>
    [CreateAssetMenu(menuName = "Ambience/Chimney Smoke Profile", fileName = "ChimneySmokeProfile")]
    public sealed class ChimneySmokeProfile : ScriptableObject
    {
        /// <summary>
        /// Particle values of one smoke intensity.
        /// </summary>
        [Serializable]
        public sealed class Look
        {
            [Tooltip("Particle tint. The alpha is the peak opacity of a puff.")]
            [SerializeField] private Color color = Color.white;

            [Tooltip("Smallest and largest size of a new puff, in metres.")]
            [SerializeField] private Vector2 size = new Vector2(0.4f, 0.8f);

            [Tooltip("Shortest and longest life of a puff, in seconds. With the speed it sets how high the smoke climbs.")]
            [SerializeField] private Vector2 lifetime = new Vector2(3f, 4f);

            [Tooltip("Slowest and fastest rise speed of a puff, in metres per second.")]
            [SerializeField] private Vector2 speed = new Vector2(0.6f, 0.9f);

            [Tooltip("Puffs emitted per second.")]
            [SerializeField, Min(0f)] private float ratePerSecond = 4f;

            [Tooltip("Strength of the sideways wobble.")]
            [SerializeField, Min(0f)] private float drift = 0.3f;

            /// <summary>Particle tint; the alpha is the peak opacity.</summary>
            public Color Color => color;

            /// <summary>Smallest and largest start size, in metres.</summary>
            public Vector2 Size => size;

            /// <summary>Shortest and longest particle life, in seconds.</summary>
            public Vector2 Lifetime => lifetime;

            /// <summary>Slowest and fastest start speed, in metres per second.</summary>
            public Vector2 Speed => speed;

            /// <summary>Particles emitted per second.</summary>
            public float RatePerSecond => ratePerSecond;

            /// <summary>Strength of the sideways wobble.</summary>
            public float Drift => drift;

            /// <summary>
            /// Creates a look with the given values.
            /// </summary>
            /// <param name="color">Particle tint, alpha included.</param>
            /// <param name="size">Smallest and largest start size.</param>
            /// <param name="lifetime">Shortest and longest life.</param>
            /// <param name="speed">Slowest and fastest start speed.</param>
            /// <param name="ratePerSecond">Particles per second.</param>
            /// <param name="drift">Sideways wobble strength.</param>
            public Look(Color color, Vector2 size, Vector2 lifetime, Vector2 speed, float ratePerSecond, float drift)
            {
                this.color = color;
                this.size = size;
                this.lifetime = lifetime;
                this.speed = speed;
                this.ratePerSecond = ratePerSecond;
                this.drift = drift;
            }
        }

        [Tooltip("Pale, thin vapour.")]
        [SerializeField] private Look light = new Look(
            new Color(0.95f, 0.95f, 0.97f, 0.45f), new Vector2(0.5f, 0.9f), new Vector2(3f, 4.5f), new Vector2(0.6f, 0.9f), 2.5f, 0.08f);

        [Tooltip("Thick, darker smoke.")]
        [SerializeField] private Look heavy = new Look(
            new Color(0.28f, 0.27f, 0.27f, 0.70f), new Vector2(0.9f, 1.5f), new Vector2(4.5f, 6f), new Vector2(1.1f, 1.6f), 7f, 0.2f);

        /// <summary>
        /// Returns the particle values of a level.
        /// </summary>
        /// <param name="level">Smoke intensity.</param>
        /// <returns>The look of the level, or null for <see cref="ChimneySmokeLevel.Off"/>.</returns>
        public Look GetLook(ChimneySmokeLevel level)
        {
            switch (level)
            {
                case ChimneySmokeLevel.Light:
                    return light;
                case ChimneySmokeLevel.Heavy:
                    return heavy;
                default:
                    return null;
            }
        }
    }
}
