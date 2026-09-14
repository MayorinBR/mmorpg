using System;
using UnityEngine;

namespace Project.Items
{
    /// <summary>
    /// A flat bonus to each of the six base stats. Used both to describe
    /// what a single equipment item grants and to represent the combined
    /// total across all currently equipped items.
    /// </summary>
    [Serializable]
    public struct StatModifiers
    {
        [SerializeField] private int strength;
        [SerializeField] private int agility;
        [SerializeField] private int vitality;
        [SerializeField] private int intelligence;
        [SerializeField] private int dexterity;
        [SerializeField] private int luck;

        /// <summary>Gets the bonus to Strength.</summary>
        public int Strength => strength;

        /// <summary>Gets the bonus to Agility.</summary>
        public int Agility => agility;

        /// <summary>Gets the bonus to Vitality.</summary>
        public int Vitality => vitality;

        /// <summary>Gets the bonus to Intelligence.</summary>
        public int Intelligence => intelligence;

        /// <summary>Gets the bonus to Dexterity.</summary>
        public int Dexterity => dexterity;

        /// <summary>Gets the bonus to Luck.</summary>
        public int Luck => luck;

        /// <summary>
        /// Combines two sets of modifiers by summing each stat.
        /// </summary>
        public static StatModifiers operator +(StatModifiers a, StatModifiers b)
        {
            return new StatModifiers(
                a.strength + b.strength,
                a.agility + b.agility,
                a.vitality + b.vitality,
                a.intelligence + b.intelligence,
                a.dexterity + b.dexterity,
                a.luck + b.luck);
        }

        /// <summary>
        /// Scales every stat by a flat integer factor, e.g. turning a
        /// per-level bonus into its total at a given skill level.
        /// </summary>
        public static StatModifiers operator *(StatModifiers a, int factor)
        {
            return new StatModifiers(
                a.strength * factor,
                a.agility * factor,
                a.vitality * factor,
                a.intelligence * factor,
                a.dexterity * factor,
                a.luck * factor);
        }

        /// <summary>
        /// Initializes a new set of stat modifiers.
        /// </summary>
        public StatModifiers(int strength, int agility, int vitality, int intelligence, int dexterity, int luck)
        {
            this.strength = strength;
            this.agility = agility;
            this.vitality = vitality;
            this.intelligence = intelligence;
            this.dexterity = dexterity;
            this.luck = luck;
        }
    }
}
