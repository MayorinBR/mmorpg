namespace Project.Persistence
{
    /// <summary>
    /// Abstracts where and how <see cref="PlayerSaveData"/> is stored, so
    /// <see cref="PlayerSaveController"/> doesn't depend on a specific
    /// storage mechanism (a local JSON file today; a cloud save or a
    /// server-side record are both drop-in replacements later, per the
    /// project's planned authoritative-server migration).
    /// </summary>
    public interface ISaveRepository
    {
        /// <summary>Gets whether save data currently exists.</summary>
        /// <returns>True if a save can be loaded.</returns>
        bool HasSaveData();

        /// <summary>
        /// Attempts to load previously-saved data.
        /// </summary>
        /// <param name="data">The loaded data, or null if none exists or it couldn't be read.</param>
        /// <returns>True if data was loaded successfully.</returns>
        bool TryLoad(out PlayerSaveData data);

        /// <summary>
        /// Persists the given data, replacing whatever was previously saved.
        /// </summary>
        /// <param name="data">The data to save.</param>
        void Save(PlayerSaveData data);
    }
}
