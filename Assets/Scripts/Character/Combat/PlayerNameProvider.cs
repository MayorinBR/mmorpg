using System;
using UnityEngine;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the player's display name. Fixed 2026-09-09: this is now the
    /// place a chosen name (from the Character Selection screen) gets
    /// written into — <see cref="Character.Combat.CharacterSessionBootstrap"/>
    /// calls <see cref="SetName"/> directly for a brand-new character, and
    /// <see cref="RestoreState"/> restores it from disk for an existing one
    /// — without the HUD needing any changes either way.
    /// </summary>
    public class PlayerNameProvider : MonoBehaviour, ISaveParticipant
    {
        [SerializeField] private string playerName = "Player";

        /// <summary>Raised whenever the player's name changes.</summary>
        public event Action<string> NameChanged;

        /// <summary>Gets the player's current display name.</summary>
        public string PlayerName => playerName;

        /// <summary>
        /// Sets the player's display name.
        /// </summary>
        /// <param name="newName">The new display name.</param>
        public void SetName(string newName)
        {
            playerName = newName;
            NameChanged?.Invoke(newName);
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.characterName = playerName;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Ignores an empty <see cref="PlayerSaveData.characterName"/>
        /// rather than overwriting the current name with a blank one — the
        /// field didn't exist before 2026-09-09, so any save written before
        /// then loads back with it empty.
        /// </remarks>
        public void RestoreState(PlayerSaveData data)
        {
            if (!string.IsNullOrEmpty(data.characterName))
            {
                SetName(data.characterName);
            }
        }
    }
}