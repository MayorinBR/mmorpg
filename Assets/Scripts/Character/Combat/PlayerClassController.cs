using System;
using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Persistence;

namespace Project.Character.Combat
{
    /// <summary>
    /// Holds the player's class history, oldest first (e.g. Novice, then
    /// Swordman). The last entry is the <see cref="CurrentClass"/>. A
    /// Novice becomes a first class through <see cref="TryEvolve"/>, the
    /// entry point for the class-change NPC; <see cref="ChangeClass"/>
    /// instead replaces the whole history and is meant for creating or
    /// loading a character.
    /// </summary>
    public class PlayerClassController : MonoBehaviour, IPlayerClassProvider, ISaveParticipant
    {
        [SerializeField] private CharacterClass startingClass = CharacterClass.Novice;
        [SerializeField] private PlayerJobProgress jobProgress;

        private readonly List<CharacterClass> classPath = new List<CharacterClass>();

        /// <summary>Raised whenever the player's class changes, including when a character is loaded.</summary>
        public event Action<CharacterClass> ClassChanged;

        /// <summary>Raised only when the player evolves, with (previousClass, newClass), before <see cref="ClassChanged"/>.</summary>
        public event Action<CharacterClass, CharacterClass> ClassEvolved;

        /// <summary>Gets the player's current class.</summary>
        public CharacterClass CurrentClass => classPath.Count > 0 ? classPath[classPath.Count - 1] : startingClass;

        /// <summary>Gets every class the player has held, oldest first, ending with <see cref="CurrentClass"/>.</summary>
        public IReadOnlyList<CharacterClass> ClassPath => classPath;

        /// <summary>Gets whether the player is a Novice who has reached the required Job Level and spent every skill point.</summary>
        public bool CanEvolve => CurrentClass == CharacterClass.Novice
            && jobProgress != null
            && jobProgress.JobLevel >= ClassProgression.EvolutionJobLevel
            && jobProgress.AvailableSkillPoints == 0;

        private void Awake()
        {
            if (classPath.Count == 0)
            {
                classPath.Add(startingClass);
            }
        }

        /// <summary>
        /// Replaces the class history with a single class, raising <see cref="ClassChanged"/>.
        /// </summary>
        /// <param name="newClass">The class to start from.</param>
        public void ChangeClass(CharacterClass newClass)
        {
            classPath.Clear();
            classPath.Add(newClass);
            ClassChanged?.Invoke(newClass);
        }

        /// <summary>
        /// Evolves a Novice into a first class. Learned skills are kept; Job
        /// Level and skill points are reset by <see cref="PlayerJobProgress"/>.
        /// </summary>
        /// <param name="target">The first class to evolve into.</param>
        /// <returns>True if the player evolved; false if not a Novice, the Job Level is too low, skill points are left unspent, or <paramref name="target"/> is Novice.</returns>
        public bool TryEvolve(CharacterClass target)
        {
            if (target == CharacterClass.Novice || CurrentClass != CharacterClass.Novice)
            {
                return false;
            }

            if (!CanEvolve)
            {
                var message = jobProgress != null && jobProgress.JobLevel >= ClassProgression.EvolutionJobLevel
                    ? "Spend all skill points on Novice skills before changing class."
                    : $"Job Level {ClassProgression.EvolutionJobLevel} required to change class.";
                PlayerFeedbackChannel.Publish(message);
                return false;
            }

            var previousClass = CurrentClass;
            classPath.Add(target);
            ClassEvolved?.Invoke(previousClass, target);
            ClassChanged?.Invoke(target);
            return true;
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            data.characterClassIndex = (int)CurrentClass;
            data.classPathIndices.Clear();

            foreach (var characterClass in classPath)
            {
                data.classPathIndices.Add((int)characterClass);
            }
        }

        /// <inheritdoc />
        public void RestoreState(PlayerSaveData data)
        {
            classPath.Clear();

            if (data.classPathIndices.Count == 0)
            {
                classPath.Add((CharacterClass)data.characterClassIndex);
            }
            else
            {
                foreach (var index in data.classPathIndices)
                {
                    classPath.Add((CharacterClass)index);
                }
            }

            ClassChanged?.Invoke(CurrentClass);
        }
    }
}
