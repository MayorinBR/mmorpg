using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Project.Ambience
{
    /// <summary>
    /// Makes whichever map is loaded follow the world's day and night cycle. It lives on a persistent object
    /// and, every time a map becomes the active scene, captures that map's authored main light, ambient
    /// light, fog and camera background as its daytime look. It then moves the main light along a sun and
    /// moon arc and multiplies those values using a <see cref="DayNightProfile"/>. Scenes listed as fixed,
    /// such as caves and interiors, keep their own lighting.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class DayNightLighting : MonoBehaviour
    {
        [Tooltip("How the lighting changes over the cycle.")]
        [SerializeField] private DayNightProfile profile;

        [Tooltip("Names of scenes that must keep their authored lighting, such as caves and interiors.")]
        [SerializeField] private string[] fixedLightingScenes = Array.Empty<string>();

        private Light sun;
        private bool isActive;
        private Color baseSunColor;
        private float baseSunIntensity;
        private float basePitch;
        private float baseYaw;
        private Color baseAmbientSky;
        private Color baseAmbientEquator;
        private Color baseAmbientGround;
        private Color baseAmbientFlat;
        private float baseAmbientIntensity;
        private Color baseFogColor;
        private Camera trackedCamera;
        private Color baseBackground;

        private void Awake()
        {
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void Start()
        {
            Capture(SceneManager.GetActiveScene());
        }

        private void Update()
        {
            if (!isActive)
            {
                return;
            }

            if (sun == null)
            {
                isActive = false;
                return;
            }

            var normalized = WorldClock.Normalized;
            ApplySun(normalized);
            ApplyTint(profile.AmbientTint(normalized));
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene == SceneManager.GetActiveScene())
            {
                Capture(scene);
            }
        }

        private void Capture(Scene scene)
        {
            sun = RenderSettings.sun;
            trackedCamera = null;
            isActive = profile != null && sun != null && Array.IndexOf(fixedLightingScenes, scene.name) < 0;

            if (!isActive)
            {
                return;
            }

            baseSunColor = sun.color;
            baseSunIntensity = sun.intensity;
            basePitch = Mathf.DeltaAngle(0f, sun.transform.eulerAngles.x);
            baseYaw = sun.transform.eulerAngles.y;
            baseAmbientSky = RenderSettings.ambientSkyColor;
            baseAmbientEquator = RenderSettings.ambientEquatorColor;
            baseAmbientGround = RenderSettings.ambientGroundColor;
            baseAmbientFlat = RenderSettings.ambientLight;
            baseAmbientIntensity = RenderSettings.ambientIntensity;
            baseFogColor = RenderSettings.fogColor;
        }

        private void ApplySun(float normalized)
        {
            var progress = DayNightCycle.ProgressInPeriod(normalized);
            var elevation = Mathf.Sin(progress * Mathf.PI);
            var pitch = Mathf.Lerp(profile.HorizonPitch, basePitch, elevation);
            var yaw = baseYaw + (progress - 0.5f) * profile.YawSweep;

            sun.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
            sun.color = baseSunColor * profile.SunColor(normalized);
            sun.intensity = baseSunIntensity * profile.SunIntensity(normalized);
        }

        private void ApplyTint(Color tint)
        {
            switch (RenderSettings.ambientMode)
            {
                case AmbientMode.Trilight:
                    RenderSettings.ambientSkyColor = baseAmbientSky * tint;
                    RenderSettings.ambientEquatorColor = baseAmbientEquator * tint;
                    RenderSettings.ambientGroundColor = baseAmbientGround * tint;
                    break;
                case AmbientMode.Flat:
                    RenderSettings.ambientLight = baseAmbientFlat * tint;
                    break;
                default:
                    RenderSettings.ambientIntensity = baseAmbientIntensity * tint.grayscale;
                    break;
            }

            if (RenderSettings.fog)
            {
                RenderSettings.fogColor = baseFogColor * tint;
            }

            ApplyCameraBackground(tint);
        }

        private void ApplyCameraBackground(Color tint)
        {
            var camera = Camera.main;

            if (camera != trackedCamera)
            {
                trackedCamera = camera;
                baseBackground = camera != null ? camera.backgroundColor : Color.black;
            }

            if (camera != null && camera.clearFlags == CameraClearFlags.SolidColor)
            {
                camera.backgroundColor = baseBackground * tint;
            }
        }
    }
}
