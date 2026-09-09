using Project.Character.Stats;

namespace Project.Flow
{
    /// <summary>
    /// Static carrier for the character chosen on the Character Selection
    /// screen, read once by <c>Character.Combat.CharacterSessionBootstrap</c>
    /// when the gameplay scene loads. Mirrors
    /// <c>World.MapTransitionService</c>'s own pending-state pattern (set
    /// before a scene load, consumed once the destination scene's own
    /// Awake phase reads it), since Unity's own
    /// <see cref="UnityEngine.SceneManagement.SceneManager.LoadScene(string)"/>
    /// has no way to pass data into the scene it loads.
    /// </summary>
    public static class GameSessionService
    {
        private static string pendingCharacterName;
        private static bool pendingIsNewCharacter;
        private static CharacterClass pendingClass;
        private static CharacterGender pendingGender;
        private static bool hasPendingCharacter;

        /// <summary>
        /// Records a brand-new character to create once the gameplay scene
        /// loads, then load that scene.
        /// </summary>
        /// <param name="characterName">The new character's name (already confirmed not to exist yet).</param>
        /// <param name="characterClass">The class chosen for the new character.</param>
        /// <param name="gender">The gender chosen for the new character.</param>
        public static void BeginNewCharacter(string characterName, CharacterClass characterClass, CharacterGender gender)
        {
            pendingCharacterName = characterName;
            pendingIsNewCharacter = true;
            pendingClass = characterClass;
            pendingGender = gender;
            hasPendingCharacter = true;
        }

        /// <summary>
        /// Records an existing character to continue with once the gameplay
        /// scene loads (its class and gender are read back from its own
        /// save file there, not passed here).
        /// </summary>
        /// <param name="characterName">The existing character's name (already confirmed to exist).</param>
        public static void BeginExistingCharacter(string characterName)
        {
            pendingCharacterName = characterName;
            pendingIsNewCharacter = false;
            hasPendingCharacter = true;
        }

        /// <summary>
        /// Consumes and clears whichever character was pending, so a stale
        /// choice can never be reapplied on a later, unrelated scene load —
        /// the same guard <c>World.MapTransitionService.ConsumePendingSpawnPointId</c>
        /// uses for the same reason.
        /// </summary>
        /// <param name="characterName">The pending character's name.</param>
        /// <param name="isNewCharacter">Whether this character still needs creating.</param>
        /// <param name="characterClass">The chosen class, meaningful only when <paramref name="isNewCharacter"/> is true.</param>
        /// <param name="gender">The chosen gender, meaningful only when <paramref name="isNewCharacter"/> is true.</param>
        /// <returns>
        /// False if nothing was pending — e.g. the gameplay scene was
        /// entered directly (Play pressed in the Editor) rather than
        /// through the menu flow — in which case every out parameter is
        /// left at its default and the caller should leave things alone.
        /// </returns>
        public static bool TryConsumePendingCharacter(out string characterName, out bool isNewCharacter, out CharacterClass characterClass, out CharacterGender gender)
        {
            characterName = pendingCharacterName;
            isNewCharacter = pendingIsNewCharacter;
            characterClass = pendingClass;
            gender = pendingGender;

            var hadPendingCharacter = hasPendingCharacter;

            hasPendingCharacter = false;
            pendingCharacterName = null;

            return hadPendingCharacter;
        }
    }
}
