using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Creates the particle systems of the waterfall: the mist that hangs over the river, the splash where the water
    /// lands, the drops that fly off the falling sheet and a light haze at the mouth.
    /// </summary>
    public static class WaterfallParticles
    {
        private const float FoamUpOffset = 0.5f;

        /// <summary>
        /// Creates the large soft mist that rises from the landing point.
        /// </summary>
        /// <param name="parent">Parent of the new object.</param>
        /// <param name="landing">World position of the landing point.</param>
        /// <param name="width">Width of the area that receives the water, in meters.</param>
        /// <param name="downstream">Horizontal direction of the river at the landing point.</param>
        /// <param name="material">Material of the particles.</param>
        /// <returns>The particle system.</returns>
        public static ParticleSystem CreateMist(Transform parent, Vector3 landing, float width, Vector2 downstream, Material material)
        {
            var system = CreateSystem("Mist", parent, landing + Vector3.up * FoamUpOffset, Quaternion.Euler(-90f, 0f, 0f), material);

            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 6f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 1f);
            main.startSize = new ParticleSystem.MinMaxCurve(4f, 7f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(0.95f, 0.98f, 1f, 0.16f);
            main.gravityModifier = -0.03f;
            main.maxParticles = 220;

            var emission = system.emission;
            emission.rateOverTime = 26f;

            ShapeOverWidth(system, width, 30f);
            Drift(system, downstream, 1.2f);
            FadeInOut(system, 0.25f);

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.6f, 1f, 1.5f));

            var rotation = system.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);

            system.Play();
            return system;
        }

        /// <summary>
        /// Creates the burst of droplets thrown up where the water hits the river.
        /// </summary>
        /// <param name="parent">Parent of the new object.</param>
        /// <param name="landing">World position of the landing point.</param>
        /// <param name="width">Width of the area that receives the water, in meters.</param>
        /// <param name="material">Material of the particles.</param>
        /// <returns>The particle system.</returns>
        public static ParticleSystem CreateImpactSpray(Transform parent, Vector3 landing, float width, Material material)
        {
            var system = CreateSystem("ImpactSpray", parent, landing + Vector3.up * 0.1f, Quaternion.Euler(-90f, 0f, 0f), material);

            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.7f, 1.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startColor = new Color(1f, 1f, 1f, 0.75f);
            main.gravityModifier = 1.2f;
            main.maxParticles = 400;

            var emission = system.emission;
            emission.rateOverTime = 110f;

            ShapeOverWidth(system, width, 40f);

            Stretch(system, 0.06f, 1.5f);
            FadeInOut(system, 0.1f);

            system.Play();
            return system;
        }

        /// <summary>
        /// Creates the dense white puffs that billow up from the impact and drift downstream, like the churn of a plunge pool.
        /// </summary>
        /// <param name="parent">Parent of the new object.</param>
        /// <param name="landing">World position of the landing point.</param>
        /// <param name="width">Width of the area that receives the water, in meters.</param>
        /// <param name="downstream">Horizontal direction of the river at the landing point.</param>
        /// <param name="material">Material of the particles.</param>
        /// <returns>The particle system.</returns>
        public static ParticleSystem CreateSplashPuffs(Transform parent, Vector3 landing, float width, Vector2 downstream, Material material)
        {
            var system = CreateSystem("SplashPuffs", parent, landing + Vector3.up * 0.3f, Quaternion.Euler(-90f, 0f, 0f), material);

            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.2f, 2.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(1.6f, 3f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(1f, 1f, 1f, 0.5f);
            main.gravityModifier = -0.05f;
            main.maxParticles = 160;

            var emission = system.emission;
            emission.rateOverTime = 22f;

            ShapeOverWidth(system, width, 22f);
            Drift(system, downstream, 1.8f);
            FadeInOut(system, 0.2f);

            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.5f, 1f, 1.6f));

            system.Play();
            return system;
        }

        /// <summary>
        /// Creates the drops that leave the lip and follow the same arc as the sheet.
        /// </summary>
        /// <param name="parent">Parent of the new object.</param>
        /// <param name="trajectory">Path of the water.</param>
        /// <param name="sheetWidth">Width of the sheet at the lip, in meters.</param>
        /// <param name="material">Material of the particles.</param>
        /// <returns>The particle system.</returns>
        public static ParticleSystem CreateFallSpray(Transform parent, WaterfallTrajectory trajectory, float sheetWidth, Material material)
        {
            var origin = new Vector3(trajectory.CenterX, trajectory.LipY + 0.2f, trajectory.LipZ);
            var system = CreateSystem("FallSpray", parent, origin, Quaternion.Euler(0f, 180f, 0f), material);

            var main = system.main;
            main.startLifetime = trajectory.FallTime;
            main.startSpeed = new ParticleSystem.MinMaxCurve(trajectory.OutwardSpeed * 0.85f, trajectory.OutwardSpeed * 1.1f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.45f);
            main.startColor = new Color(1f, 1f, 1f, 0.55f);
            main.gravityModifier = trajectory.Gravity / Mathf.Abs(Physics.gravity.y);
            main.maxParticles = 200;

            var emission = system.emission;
            emission.rateOverTime = 28f;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(sheetWidth, 0.2f, 0.2f);
            shape.randomDirectionAmount = 0.06f;

            Stretch(system, 0.05f, 1.6f);
            FadeInOut(system, 0.1f);

            system.Play();
            return system;
        }

        /// <summary>
        /// Creates a light haze that drifts out of the cave.
        /// </summary>
        /// <param name="parent">Parent of the new object.</param>
        /// <param name="trajectory">Path of the water.</param>
        /// <param name="material">Material of the particles.</param>
        /// <returns>The particle system.</returns>
        public static ParticleSystem CreateMouthHaze(Transform parent, WaterfallTrajectory trajectory, Material material)
        {
            var origin = new Vector3(trajectory.CenterX, trajectory.LipY + 1.5f, trajectory.LipZ);
            var system = CreateSystem("MouthHaze", parent, origin, Quaternion.Euler(0f, 180f, 0f), material);

            var main = system.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(3f, 4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(3f, 5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = new Color(1f, 1f, 1f, 0.14f);
            main.maxParticles = 40;

            var emission = system.emission;
            emission.rateOverTime = 5f;

            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 40f;
            shape.radius = 2.4f;

            FadeInOut(system, 0.3f);

            system.Play();
            return system;
        }

        private static void ShapeOverWidth(ParticleSystem system, float width, float angle)
        {
            const float BaseRadius = 2.5f;
            var shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = angle;
            shape.radius = BaseRadius;
            shape.scale = new Vector3(width / (BaseRadius * 2f), 0.6f, 1f);
        }

        private static void Drift(ParticleSystem system, Vector2 downstream, float speed)
        {
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = downstream.x * speed;
            velocity.y = 0f;
            velocity.z = downstream.y * speed;
        }

        private static ParticleSystem CreateSystem(string name, Transform parent, Vector3 position, Quaternion rotation, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, rotation);

            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = system.main;
            main.duration = 5f;
            main.loop = true;
            main.prewarm = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }

        private static void Stretch(ParticleSystem system, float velocityScale, float lengthScale)
        {
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = velocityScale;
            renderer.lengthScale = lengthScale;
        }

        private static void FadeInOut(ParticleSystem system, float fadeInEnd)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, fadeInEnd), new GradientAlphaKey(0f, 1f) });

            var color = system.colorOverLifetime;
            color.enabled = true;
            color.color = gradient;
        }
    }
}
