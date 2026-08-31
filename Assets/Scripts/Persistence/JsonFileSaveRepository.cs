using System;
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
            if (!File.Exists(filePath))
            {
                data = null;
                return false;
            }

            try
            {
                var json = File.ReadAllText(filePath);
                data = JsonUtility.FromJson<PlayerSaveData>(json);
                return data != null;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Failed to load save data from '{filePath}': {exception.Message}");
                data = null;
                return false;
            }
        }

        /// <inheritdoc />
        public void Save(PlayerSaveData data)
        {
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);
        }
    }
}
