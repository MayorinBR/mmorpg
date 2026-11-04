using System.IO;
using UnityEngine;

namespace Project.Persistence
{
    /// <summary>
    /// Stores <see cref="PlayerSaveData"/> as a single JSON file on disk.
    /// A plain C# class rather than a MonoBehaviour, so it can be unit
    /// tested independently of a running scene, matching how
    /// <c>Inventory</c> and <c>EquipmentManager</c> keep their own logic
    /// outside MonoBehaviour where practical.
    /// </summary>
    public class JsonFileSaveRepository : ISaveRepository
    {
        private readonly string filePath;

        /// <summary>
        /// Initializes the repository to read from and write to a specific file.
        /// </summary>
        /// <param name="filePath">The full path of the save file.</param>
        public JsonFileSaveRepository(string filePath)
        {
            this.filePath = filePath;
        }

        /// <inheritdoc />
        public bool HasSaveData()
        {
            return File.Exists(filePath);
        }

        /// <inheritdoc />
        public bool TryLoad(out PlayerSaveData data)
        {
            return SafeFile.TryRead(filePath, JsonUtility.FromJson<PlayerSaveData>, out data);
        }

        /// <inheritdoc />
        public void Save(PlayerSaveData data)
        {
            data.saveVersion = PlayerSaveData.CurrentSaveVersion;
            SafeFile.WriteAllText(filePath, JsonUtility.ToJson(data, true));
        }
    }
}
