using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Controls the smoke of every chimney of one building. Pick the level on the building (Off, Light or Heavy)
    /// or change it from code with <see cref="SetLevel"/>. The particle values of each level come from a shared
    /// <see cref="ChimneySmokeProfile"/>. The level is applied when the building starts, so a change made in the
    /// Inspector shows in Play Mode.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class ChimneySmoke : MonoBehaviour
    {
        [Tooltip("Particle values of each level, shared by all buildings.")]
        [SerializeField] private ChimneySmokeProfile profile;

        [Tooltip("Smoke level of this building.")]
        [SerializeField] private ChimneySmokeLevel level = ChimneySmokeLevel.Light;

        [Tooltip("One emitter per chimney. Filled automatically from the children when left empty.")]
        [SerializeField] private ParticleSystem[] emitters;

        /// <summary>Current smoke level of the building.</summary>
        public ChimneySmokeLevel Level => level;

        private void Awake()
        {
            if (emitters == null || emitters.Length == 0)
            {
                emitters = GetComponentsInChildren<ParticleSystem>(true);
            }

            Apply();
        }

        /// <summary>
        /// Changes the smoke level. Off stops new puffs and lets the smoke in the air fade out.
        /// </summary>
        /// <param name="newLevel">Level to show.</param>
        public void SetLevel(ChimneySmokeLevel newLevel)
        {
            level = newLevel;
            Apply();
        }

        private void Apply()
        {
            if (emitters == null)
            {
                return;
            }

            var look = profile != null ? profile.GetLook(level) : null;
            foreach (var emitter in emitters)
            {
                if (emitter == null)
                {
                    continue;
                }

                var emission = emitter.emission;
                emission.enabled = look != null;
                if (look == null)
                {
                    continue;
                }

                var main = emitter.main;
                main.startColor = look.Color;
                main.startSize = new ParticleSystem.MinMaxCurve(look.Size.x, look.Size.y);
                main.startLifetime = new ParticleSystem.MinMaxCurve(look.Lifetime.x, look.Lifetime.y);
                main.startSpeed = new ParticleSystem.MinMaxCurve(look.Speed.x, look.Speed.y);
                emission.rateOverTime = look.RatePerSecond;
                main.maxParticles = Mathf.CeilToInt(look.RatePerSecond * look.Lifetime.y * 1.25f) + 1;

                var noise = emitter.noise;
                noise.strength = look.Drift;

                if (!emitter.isPlaying)
                {
                    emitter.Play();
                }
            }
        }
    }
}
