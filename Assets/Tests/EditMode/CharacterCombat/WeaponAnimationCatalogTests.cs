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
        private AnimationClip rodCastClip;

        [SetUp]
        public void SetUp()
        {
            fallbackClip = new AnimationClip();
            bowClip = new AnimationClip();
            rodCastClip = new AnimationClip();
            catalog = ScriptableObject.CreateInstance<WeaponAnimationCatalog>();

            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("fallbackAttackClip").objectReferenceValue = fallbackClip;

            var entries = serialized.FindProperty("entries");
            entries.arraySize = 3;

            var bowEntry = entries.GetArrayElementAtIndex(0);
            bowEntry.FindPropertyRelative("subtype").enumValueIndex = (int)WeaponSubtype.Bow;
            bowEntry.FindPropertyRelative("attackClip").objectReferenceValue = bowClip;

            var emptyEntry = entries.GetArrayElementAtIndex(1);
            emptyEntry.FindPropertyRelative("subtype").enumValueIndex = (int)WeaponSubtype.Dagger;

            var rodEntry = entries.GetArrayElementAtIndex(2);
            rodEntry.FindPropertyRelative("subtype").enumValueIndex = (int)WeaponSubtype.Rod;
            rodEntry.FindPropertyRelative("castClip").objectReferenceValue = rodCastClip;

            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(catalog);
            Object.DestroyImmediate(fallbackClip);
            Object.DestroyImmediate(bowClip);
            Object.DestroyImmediate(rodCastClip);
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
        public void GetCastClip_SubtypeWithCastClip_ReturnsItsClip()
        {
            Assert.AreSame(rodCastClip, catalog.GetCastClip(WeaponSubtype.Rod));
        }

        [Test]
        public void GetCastClip_SubtypeWithoutCastClip_ReturnsNull()
        {
            Assert.IsNull(catalog.GetCastClip(WeaponSubtype.Bow));
            Assert.IsNull(catalog.GetCastClip(WeaponSubtype.Mace));
        }

        [Test]
        public void GetAttackClip_EntryWithoutClip_ReturnsFallback()
        {
            Assert.AreSame(fallbackClip, catalog.GetAttackClip(WeaponSubtype.Dagger));
        }
    }
}
