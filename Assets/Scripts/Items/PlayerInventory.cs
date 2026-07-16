using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// Owns the player's <see cref="Inventory"/>. Acts as the composition
    /// root connecting the plain C# inventory class to the Unity component
    /// world, mirroring how <see cref="Character.Combat.PlayerStatsController"/>
    /// wraps <see cref="Character.Stats.CharacterBaseStats"/>.
    /// </summary>
    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private int capacity = 20;

        /// <summary>Gets the player's inventory.</summary>
        public Inventory Items { get; private set; }

        private void Awake()
        {
            Items = new Inventory(capacity);
        }
    }
}