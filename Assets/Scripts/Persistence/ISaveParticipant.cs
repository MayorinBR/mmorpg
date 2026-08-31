namespace Project.Persistence
{
    /// <summary>
    /// Implemented by any component that owns a slice of persistable player
    /// state. <see cref="PlayerSaveController"/> discovers every
    /// implementation on its own GameObject automatically via
    /// <c>GetComponents&lt;ISaveParticipant&gt;()</c>, so adding a new
    /// persistable system only requires implementing this interface — no
    /// wiring in the save controller or the Inspector is needed.
    /// </summary>
    public interface ISaveParticipant
    {
        /// <summary>
        /// Writes this component's current state into the shared save data.
        /// </summary>
        /// <param name="data">The save data being built, shared across every participant.</param>
        void CaptureState(PlayerSaveData data);

        /// <summary>
        /// Restores this component's state from previously-saved data.
        /// </summary>
        /// <param name="data">The save data that was loaded from disk.</param>
        void RestoreState(PlayerSaveData data);
    }
}
