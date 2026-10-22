using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Quests
{
    /// <summary>
    /// Pairs a first class with the trial quest a Novice must complete to become it.
    /// </summary>
    [Serializable]
    public class ClassTrial
    {
        [SerializeField] private CharacterClass targetClass;
        [SerializeField] private QuestDefinition quest;

        /// <summary>Gets the class the player becomes when the trial is completed.</summary>
        public CharacterClass TargetClass => targetClass;

        /// <summary>Gets the quest that must be completed to become <see cref="TargetClass"/>.</summary>
        public QuestDefinition Quest => quest;
    }
}
