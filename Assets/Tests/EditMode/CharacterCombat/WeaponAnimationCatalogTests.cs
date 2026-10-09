using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Project.Character.Combat;
using Project.Items;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="WeaponAnimationCatalog"/>.
    /// </summary>
    public class WeaponAnimationCatalogTests
    {
        private WeaponAnimationCatalog catalog;
        private AnimationClip fallbackClip;
        private AnimationClip bowClip;

        [SetUp]
        public void SetUp()
        {
            fallbackClip = new AnimationClip();
            bowClip = new AnimationClip();
            catalog = ScriptableObject.CreateInstance<WeaponAnimationCatalog>();

            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("fallbackAttackClip").objectReferenceValue = fallbackClip;

            var entries = serialized.FindProperty("entries");
            entries.arraySize = 2;

            var bowEntry = entries.GetArrayElementAtIndex(0);
            bowEntry.FindPropertyRelative("subtype").enumValueIndex = (int)WeaponSubtype.Bow;
            bowEntry.FindPropertyRelative("attackClip").objectReferenceValue = bowClip;

            var emptyEntry = entries.GetArrayElementAtIndex(1);
            emptyEntry.FindPropertyRelative("subtype").enumValueIndex = (int)WeaponSubtype.Dagger;

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(fallbackClip);
            Object.DestroyImmediate(bowClip);
        }

        [Test]
        public void GetAttackClip_SubtypeWithEntry_ReturnsItsClip()
        {
            Assert.AreSame(bowClip, catalog.GetAttackClip(WeaponSubtype.Bow));
        }

        [Test]
        public void GetAttackClip_SubtypeWithoutEntry_ReturnsFallback()
        {
            Assert.AreSame(fallbackClip, catalog.GetAttackClip(WeaponSubtype.Mace));
        }

        [Test]
        public void GetAttackClip_EntryWithoutClip_ReturnsFallback()
        {
            Assert.AreSame(fallbackClip, catalog.GetAttackClip(WeaponSubtype.Dagger));
        }
    }
}
