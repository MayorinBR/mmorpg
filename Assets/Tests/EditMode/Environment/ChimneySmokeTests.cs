using NUnit.Framework;
using Project.Ambience;
using UnityEditor;
using UnityEngine;

namespace Project.Ambience.Tests
{
    /// <summary>
    /// Checks that each smoke level drives the emitters as intended.
    /// </summary>
    public sealed class ChimneySmokeTests
    {
        private GameObject root;
        private ChimneySmokeProfile profile;
        private ParticleSystem emitter;
        private ChimneySmoke smoke;

        /// <summary>Builds a building with one emitter and the default profile.</summary>
        [SetUp]
        public void SetUp()
        {
            profile = ScriptableObject.CreateInstance<ChimneySmokeProfile>();
            root = new GameObject("Building");
            emitter = new GameObject("Smoke_01").AddComponent<ParticleSystem>();
            emitter.transform.SetParent(root.transform, false);
            emitter.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            smoke = root.AddComponent<ChimneySmoke>();

            var serialized = new SerializedObject(smoke);
            serialized.FindProperty("profile").objectReferenceValue = profile;
            var list = serialized.FindProperty("emitters");
            list.arraySize = 1;
            list.GetArrayElementAtIndex(0).objectReferenceValue = emitter;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Removes the objects created by the test.</summary>
        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            Object.DestroyImmediate(profile);
        }

        /// <summary>Off must stop the emission.</summary>
        [Test]
        public void Off_DisablesEmission()
        {
            smoke.SetLevel(ChimneySmokeLevel.Light);
            smoke.SetLevel(ChimneySmokeLevel.Off);

            Assert.IsFalse(emitter.emission.enabled);
        }

        /// <summary>Heavy must emit more and darker smoke than Light.</summary>
        [Test]
        public void Heavy_EmitsMoreAndDarkerThanLight()
        {
            smoke.SetLevel(ChimneySmokeLevel.Light);
            var lightRate = emitter.emission.rateOverTime.constant;
            var lightColor = emitter.main.startColor.color;

            smoke.SetLevel(ChimneySmokeLevel.Heavy);
            var heavyRate = emitter.emission.rateOverTime.constant;
            var heavyColor = emitter.main.startColor.color;

            Assert.IsTrue(emitter.emission.enabled);
            Assert.Greater(heavyRate, lightRate);
            Assert.Less(heavyColor.grayscale, lightColor.grayscale);
        }
    }
}
