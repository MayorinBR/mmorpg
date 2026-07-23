using System;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the player's display name. Local-only for now (just a
    /// serialized default) — when online play exists, this becomes the
    /// place a networked name (from character selection or the server)
    /// gets written into, without the HUD needing any changes.
    /// </summary>
    public class PlayerNameProvider : MonoBehaviour
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
    }
}