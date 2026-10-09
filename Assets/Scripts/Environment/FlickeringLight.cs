using UnityEngine;

namespace Project.Ambience
{
    /// <summary>
    /// Makes a light flicker like a flame by varying its intensity with smooth Perlin noise.
    /// Each instance starts at a random point of the noise, so neighbouring flames do not flicker in sync.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public sealed class FlickeringLight : MonoBehaviour
    {
        [Tooltip("Maximum fraction the intensity moves above or below its starting value.")]
        [SerializeField, Range(0f, 1f)] private float intensityVariation = 0.25f;

        [Tooltip("How fast the flame flickers.")]
        [SerializeField, Min(0f)] private float speed = 3f;

        private Light flameLight;
        private float baseIntensity;
        private float noiseOffset;

        private void Awake()
        {
            flameLight = GetComponent<Light>();
            baseIntensity = flameLight.intensity;
            noiseOffset = Random.value * 100f;
        }

        private void Update()
        {
            var noise = Mathf.PerlinNoise(noiseOffset, Time.time * speed);
            flameLight.intensity = baseIntensity * (1f + (noise - 0.5f) * 2f * intensityVariation);
        }
    }
}
