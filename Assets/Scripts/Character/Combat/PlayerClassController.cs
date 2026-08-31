using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the player's current <see cref="CharacterClass"/>. Starts as
    /// one of the six base classes. <see cref="ChangeClass"/> is
    /// intentionally the only way to change it, so that when class
    /// evolution (promotions like Swordman to Knight) is designed later,
    /// it plugs in here without other systems needing to change — they
    /// already react to <see cref="ClassChanged"/> rather than reading the
    /// class once and caching it.
    /// </summary>
    public class PlayerClassController : MonoBehaviour, IPlayerClassProvider, ISaveParticipant
    {
        [SerializeField] private CharacterClass startingClass = CharacterClass.Swordman;

        /// <summary>Raised whenever the player's class changes.</summary>
        public event Action<CharacterClass> ClassChanged;

        /// <summary>Gets the player's current class.</summary>
        public CharacterClass CurrentClass { get; private set; }

        private void Awake()
        {
            CurrentClass = startingClass;
        }

        /// <summary>
        /// Changes the player's class, raising <see cref="ClassChanged"/>.
        /// The intended entry point for future class evolution/promotion.
        /// </summary>
        /// <param name="newClass">The class to change to.</param>
        public void ChangeClass(CharacterClass newClass)
        {
            CurrentClass = newClass;
            ClassChanged?.Invoke(newClass);
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.characterClassIndex = (int)CurrentClass;
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            ChangeClass((CharacterClass)data.characterClassIndex);
        }
    }
}
