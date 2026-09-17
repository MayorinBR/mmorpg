using UnityEngine;
using Project.Persistence;

namespace Project.Maps
{
    /// <summary>
    /// Saves the player's current map id as its own dedicated
    /// <see cref="ISaveParticipant"/> slice, separate from window, stat and
    /// skill state. Restoring it does not yet move the player to a
    /// different scene on load: every session still starts at the scene
    /// chosen by the Character Selection flow, and
    /// <see cref="Project.World.MapBootstrap"/> is the authority on the
    /// current map for whichever scene actually loaded — so this only
    /// records history for now.
    /// </summary>
    /// <remarks>
    /// ponytail: resuming into the saved map on login needs the Character
    /// Selection flow to load a scene by saved id instead of the fixed
    /// "Prototype_Map01" — add that when cross-session map resume is needed.
    /// </remarks>
    public class CurrentMapSaveParticipant : MonoBehaviour, ISaveParticipant
    {
        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.currentMapId = CurrentMapTracker.Current != null ? CurrentMapTracker.Current.MapId : string.Empty;
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            // Intentionally not applied yet — see class remarks.
        }
    }
}
