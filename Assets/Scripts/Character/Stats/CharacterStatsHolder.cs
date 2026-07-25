using UnityEngine;

namespace Project.Character.Stats
{
    /// <summary>
    /// Holds a single <see cref="CharacterStatsDefinition"/> reference for a
    /// character, shared by any component that needs it (health, mana,
    /// enemy AI, etc.) via <c>GetComponent</c> instead of each requiring
    /// the same asset dragged into its own field.
    /// </summary>
    public class CharacterStatsHolder : MonoBehaviour
    {
        [SerializeField] private CharacterStatsDefinition stats;

        /// <summary>Gets the character's stats definition.</summary>
        public CharacterStatsDefinition Stats => stats;
    }
}