using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Skills
{
    /// <summary>
    /// Defines a single skill: which classes can learn it, who it can
    /// affect, its resource cost, cooldown, range, and how its effect
    /// scales. Instances are authored as assets, mirroring how items and
    /// character stats are already defined as data in this project.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSkill", menuName = "Project/Skills/Skill")]
    public class SkillDefinition : ScriptableObject
    {
        [SerializeField] private string skillName;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private CharacterClass[] allowedClasses;
        [SerializeField] private SkillEffectType effectType;
        [SerializeField] private SkillTargetType targetType;
        [SerializeField] private int maxLevel = 5;
        [SerializeField] private int manaCost = 10;
        [SerializeField] private float cooldownSeconds = 2f;
        [SerializeField] private float range = 3f;

        [Header("Damage (only used if Effect Type is Damage)")]
        [SerializeField] private SkillDamageType damageType;
        [SerializeField] private float damageMultiplierPerLevel = 1f;

        [Header("Heal (only used if Effect Type is Heal)")]
        [SerializeField] private int healAmount = 10;

        /// <summary>Gets the skill's display name.</summary>
        public string SkillName => skillName;

        /// <summary>Gets the skill's description text.</summary>
        public string Description => description;

        /// <summary>Gets the icon shown in future skill UI.</summary>
        public Sprite Icon => icon;

        /// <summary>Gets the classes allowed to learn this skill. An empty array means any class can learn it.</summary>
        public IReadOnlyList<CharacterClass> AllowedClasses => allowedClasses;

        /// <summary>Gets whether this skill deals damage or heals.</summary>
        public SkillEffectType EffectType => effectType;

        /// <summary>Gets who this skill can affect.</summary>
        public SkillTargetType TargetType => targetType;

        /// <summary>Gets the maximum level this skill can be leveled to.</summary>
        public int MaxLevel => maxLevel;

        /// <summary>Gets the mana cost to cast this skill, regardless of its level.</summary>
        public int ManaCost => manaCost;

        /// <summary>Gets the cooldown, in seconds, after casting this skill.</summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>Gets the maximum distance from which this skill can be cast. Ignored for Self-targeted skills.</summary>
        public float Range => range;

        /// <summary>Gets whether this damage skill scales from Status ATK or Status MATK. Only meaningful when <see cref="EffectType"/> is Damage.</summary>
        public SkillDamageType DamageType => damageType;

        /// <summary>
        /// Calculates this skill's damage at the given level. Only meaningful when <see cref="EffectType"/> is Damage.
        /// </summary>
        /// <param name="statusAtk">The caster's current Status ATK.</param>
        /// <param name="statusMatk">The caster's current Status MATK.</param>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated damage.</returns>
        public int CalculateDamage(int statusAtk, int statusMatk, int skillLevel)
        {
            var baseStat = damageType == SkillDamageType.Physical ? statusAtk : statusMatk;
            return Mathf.RoundToInt(baseStat * damageMultiplierPerLevel * skillLevel);
        }

        /// <summary>
        /// Gets this skill's heal amount. Currently a flat value regardless
        /// of level; scaling with the caster's INT is a planned future
        /// refinement. Only meaningful when <see cref="EffectType"/> is Heal.
        /// </summary>
        public int CalculateHeal()
        {
            return healAmount;
        }
    }
}