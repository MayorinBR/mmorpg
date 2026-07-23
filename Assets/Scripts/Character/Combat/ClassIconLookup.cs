using System;
using UnityEngine;

namespace Project.Character.Stats
{
    /// <summary>
    /// Maps each <see cref="CharacterClass"/> to the sprite shown for it
    /// (e.g. below the player's name on the HUD). Authored as a single
    /// asset so adding a class icon later doesn't require touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "ClassIconLookup", menuName = "Project/Character/Class Icon Lookup")]
    public class ClassIconLookup : ScriptableObject
    {
        [SerializeField] private ClassIconEntry[] icons;

        /// <summary>
        /// Gets the sprite associated with the given class.
        /// </summary>
        /// <param name="characterClass">The class to look up.</param>
        /// <returns>The class's sprite, or null if none is configured.</returns>
        public Sprite GetIcon(CharacterClass characterClass)
        {
            foreach (var entry in icons)
            {
                if (entry.CharacterClass == characterClass)
                {
                    return entry.Icon;
                }
            }

            return null;
        }

        [Serializable]
        private struct ClassIconEntry
        {
            [SerializeField] private CharacterClass characterClass;
            [SerializeField] private Sprite icon;

            public CharacterClass CharacterClass => characterClass;
            public Sprite Icon => icon;
        }
    }
}