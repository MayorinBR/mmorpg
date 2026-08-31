using System.IO;
using UnityEngine;

namespace Project.Persistence
{
    /// <summary>
    /// Coordinates saving and loading player progress. Every sibling
    /// component on the same GameObject that implements
    /// <see cref="ISaveParticipant"/> is discovered automatically via
    /// <see cref="GameObject.GetComponents{T}()"/>, so adding a new
    /// persistable system only requires implementing that interface on it —
    /// nothing here or in the Inspector needs to change. Loads automatically
    /// on <see cref="Start"/> by default, which is enough for the current
    /// single-player prototyping phase; an explicit save is triggered by
    /// calling <see cref="Save"/> (wire this to a keybind or a UI button).
    /// </summary>
    public class PlayerSaveController : MonoBehaviour
    {
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private string saveFileName = "playersave.json";

        private ISaveRepository repository;
        private ISaveParticipant[] participants;

        /// <summary>Gets whether a save file currently exists on disk.</summary>
        public bool HasSaveData => repository.HasSaveData();

        private void Awake()
        {
            repository = new JsonFileSaveRepository(Path.Combine(Application.persistentDataPath, saveFileName));
            participants = GetComponents<ISaveParticipant>();
        }

        private void Start()
        {
            if (loadOnStart)
            {
                Load();
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
        /// Captures state from every save-aware sibling component and
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
        /// save-aware sibling component. Does nothing if no save file
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
