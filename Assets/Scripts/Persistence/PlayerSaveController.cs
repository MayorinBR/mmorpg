using System.IO;
using System.Linq;
using UnityEngine;

namespace Project.Persistence
{
    /// <summary>
    /// Coordinates saving and loading player progress. Every
    /// <see cref="ISaveParticipant"/> in the loaded scene is discovered
    /// automatically — regardless of which GameObject it lives on, since
    /// persistable state now spans both the Player hierarchy (stats,
    /// skills, inventory) and the UI hierarchy (window layout) — so adding
    /// a new persistable system only requires implementing that interface
    /// on it; nothing here or in the Inspector needs to change. Inactive
    /// objects are included in the search, since a closed window (hidden
    /// via <c>SetActive(false)</c>) still needs its layout captured and
    /// restored. Loads automatically on <see cref="Start"/> by default,
    /// which is enough for the current single-player prototyping phase; an
    /// explicit save is triggered by calling <see cref="Save"/> (wire this
    /// to a keybind or a UI button).
    /// </summary>
    /// <remarks>
    /// Targets one fixed <see cref="saveFileName"/> under
    /// <see cref="Application.persistentDataPath"/> by default — enough
    /// when the scene is entered directly (e.g. pressing Play in the
    /// Editor without going through the menu flow). When the Character
    /// Selection screen was used instead, <see cref="Character.Combat.CharacterSessionBootstrap"/>
    /// calls <see cref="ConfigureCharacter"/> during its own <c>Awake</c> —
    /// guaranteed by Unity to complete before this component's own
    /// <see cref="Start"/> runs, regardless of GameObject order — to
    /// redirect this controller at that specific character's own save file
    /// (see <see cref="CharacterSaveLookup"/>) instead, and to trigger an
    /// immediate <see cref="Save"/> the first time a brand-new character is
    /// entered, so its save file exists on disk right away rather than only
    /// at <see cref="OnApplicationQuit"/>.
    /// </remarks>
    public class PlayerSaveController : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private string saveFileName = "playersave.json";

        private ISaveRepository repository;
        private ISaveParticipant[] participants;
        private string characterNameOverride;
        private bool isNewCharacterOverride;

        /// <summary>Gets whether a save file currently exists on disk.</summary>
        public bool HasSaveData => repository.HasSaveData();

        private void Awake()
        {
            repository = new JsonFileSaveRepository(Path.Combine(Application.persistentDataPath, saveFileName));
            participants = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<ISaveParticipant>()
                .ToArray();
        }

        /// <summary>
        /// Redirects this controller at a specific character's own save
        /// file instead of the fixed <see cref="saveFileName"/> default.
        /// Must be called during another component's <c>Awake</c> (never
        /// later) so it's guaranteed to land before this component's own
        /// <see cref="Start"/> reads it.
        /// </summary>
        /// <param name="characterName">The character whose save file this controller should target.</param>
        /// <param name="isNewCharacter">
        /// True for a character that doesn't have a save yet — <see cref="Start"/>
        /// will then save immediately after the (harmless, no-op) load
        /// attempt, so the identity already applied by
        /// <see cref="Character.Combat.CharacterSessionBootstrap"/> is
        /// persisted right away instead of waiting for
        /// <see cref="OnApplicationQuit"/>.
        /// </param>
        public void ConfigureCharacter(string characterName, bool isNewCharacter)
        {
            characterNameOverride = characterName;
            isNewCharacterOverride = isNewCharacter;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(characterNameOverride))
            {
                repository = new JsonFileSaveRepository(CharacterSaveLookup.SaveFilePath(characterNameOverride));
            }

            if (loadOnStart)
            {
                Load();
            }

            if (isNewCharacterOverride)
            {
                Save();
            }
        }

        /// <summary>
        /// Saves automatically when the game closes, so prototype sessions
        /// aren't lost if nothing else ever calls <see cref="Save"/>. This is
        /// a minimal default for single-player testing; a real quit/logout
        /// flow should still call <see cref="Save"/> explicitly so it also
        /// covers alt-tab kills and editor Play Mode exits on some platforms.
        /// </summary>
        private void OnApplicationQuit()
        {
            Save();
        }

        /// <summary>
        /// Captures state from every save-aware component in the scene and
        /// writes it to disk, replacing any previous save.
        /// </summary>
        public void Save()
        {
            var data = new PlayerSaveData();

            foreach (var participant in participants)
            {
                participant.CaptureState(data);
            }

            repository.Save(data);
        }

        /// <summary>
        /// Loads saved data from disk, if any, and restores it into every
        /// save-aware component in the scene. Does nothing if no save file
        /// exists yet (e.g. the very first run).
        /// </summary>
        public void Load()
        {
            if (!repository.TryLoad(out var data))
            {
                return;
            }

            foreach (var participant in participants)
            {
                participant.RestoreState(data);
            }
        }
    }
}
