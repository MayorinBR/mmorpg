using System.Collections.Generic;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

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
        [SerializeField] private Element element = Element.Neutral;

        [Tooltip("Extra flat Hit rating this skill's damage roll gets on top of the caster's own Hit, scaling with level — e.g. Bash's real 5-50% Ragnarok Online accuracy bonus. Zero for skills without one.")]
        [SerializeField] private int accuracyBonusPerLevel;

        [Tooltip("If true, this Damage skill needs no pre-selected target at all: it hits every living IDamageable within AreaRadius of the CASTER's own position instead of a single selected target — e.g. Magnum Break.")]
        [SerializeField] private bool isAreaOfEffect;

        [Tooltip("Radius, in meters, around the caster that an area-of-effect skill damages. Only meaningful when IsAreaOfEffect is true.")]
        [SerializeField] private float areaRadius = 3f;

        [Header("Passive (only used if Effect Type is Passive)")]
        [Tooltip("Flat bonus to Status ATK per skill level, applied automatically while this passive skill is learned and (if PassiveRequiredWeaponSubtypes is non-empty) a matching weapon is equipped in the main hand — e.g. Sword Mastery.")]
        [SerializeField] private float passiveAttackBonusPerLevel;

        [Tooltip("If non-empty, this passive's attack bonus only applies while the equipped main-hand weapon's subtype is one of these — e.g. Sword Mastery requires Dagger or One-Hand Sword. Empty means the bonus always applies once learned.")]
        [SerializeField] private WeaponSubtype[] passiveRequiredWeaponSubtypes;

        [Header("Heal (only used if Effect Type is Heal)")]
        [SerializeField] private int healAmount = 10;

        [Header("Buff/Debuff (only used if Effect Type is Buff)")]
        [Tooltip("Attack power multiplier bonus per skill level, e.g. Provoke's +32% at this project's max level.")]
        [SerializeField] private float buffAtkPercentPerLevel;

        [Tooltip("Physical defense multiplier bonus per skill level. Negative for a debuff, e.g. Provoke's -55% DEF at this project's max level.")]
        [SerializeField] private float buffDefPercentPerLevel;

        [Tooltip("Flat magical defense bonus per skill level, e.g. Endure's +10 MDEF at this project's max level.")]
        [SerializeField] private int buffMdefBonusPerLevel;

        [Tooltip("Flat duration in seconds, before any per-level component — e.g. Provoke's fixed 30s (BuffDurationPerLevel left at zero).")]
        [SerializeField] private float buffDurationSeconds;

        [Tooltip("Extra duration in seconds per skill level, added on top of BuffDurationSeconds — e.g. Endure's level-scaling duration, reaching the wiki's 37s at this project's max level.")]
        [SerializeField] private float buffDurationPerLevel;

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

        /// <summary>
        /// Gets the maximum distance from which this skill can be cast.
        /// Ignored for Self-targeted skills, and for an area-of-effect
        /// skill (see <see cref="IsAreaOfEffect"/>), which is always
        /// centered on the caster rather than cast "at" a distant target.
        /// </summary>
        public float Range => range;

        /// <summary>Gets whether this damage skill scales from Status ATK or Status MATK. Only meaningful when <see cref="EffectType"/> is Damage.</summary>
        public SkillDamageType DamageType => damageType;

        /// <summary>
        /// Gets the element this skill's damage carries. Defaults to
        /// <see cref="Element.Neutral"/> (plain physical/magical damage,
        /// still resistable like any other element) unless explicitly set
        /// to something else, e.g. FireBolt's <see cref="Element.Fire"/>.
        /// Read by an optional <see cref="ElementalResistanceComponent"/>
        /// on the target. Only meaningful when <see cref="EffectType"/> is
        /// Damage.
        /// </summary>
        public Element Element => element;

        /// <summary>
        /// Gets whether this Damage skill hits every living
        /// <see cref="Project.Combat.IDamageable"/> within
        /// <see cref="AreaRadius"/> of the caster instead of a single
        /// pre-selected target — see <see cref="SkillTargetType.AreaAroundCaster"/>.
        /// Only meaningful when <see cref="EffectType"/> is Damage.
        /// </summary>
        public bool IsAreaOfEffect => isAreaOfEffect;

        /// <summary>
        /// Gets the radius, in meters, around the caster that an
        /// area-of-effect skill damages. Only meaningful when
        /// <see cref="IsAreaOfEffect"/> is true.
        /// </summary>
        public float AreaRadius => areaRadius;

        /// <summary>
        /// Gets the weapon subtypes this passive's attack bonus requires
        /// the equipped main-hand weapon to match. Empty means the bonus
        /// always applies once the skill is learned. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        public IReadOnlyList<WeaponSubtype> PassiveRequiredWeaponSubtypes => passiveRequiredWeaponSubtypes;

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

        /// <summary>
        /// Calculates the extra Hit rating this skill's damage roll gets on
        /// top of the caster's own Hit, at the given level. Only meaningful
        /// when <see cref="EffectType"/> is Damage.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated accuracy bonus, zero for skills without one.</returns>
        public int GetAccuracyBonus(int skillLevel)
        {
            return accuracyBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat Status ATK bonus this passive skill grants
        /// at the given level, before <see cref="PassiveRequiredWeaponSubtypes"/>
        /// is checked against the equipped weapon. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated attack bonus, zero for passives without one.</returns>
        public int GetPassiveAttackBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveAttackBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the attack power multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. 0.32 for Provoke's +32%
        /// ATK). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an ATK component.</returns>
        public float GetBuffAtkPercent(int skillLevel)
        {
            return buffAtkPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the physical defense multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. -0.55 for Provoke's -55%
        /// DEF). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without a DEF component.</returns>
        public float GetBuffDefPercent(int skillLevel)
        {
            return buffDefPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat magical defense bonus this buff/debuff skill
        /// grants at the given level. Only meaningful when
        /// <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an MDEF component.</returns>
        public int GetBuffMdefBonus(int skillLevel)
        {
            return Mathf.RoundToInt(buffMdefBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates how long, in seconds, this buff/debuff lasts once
        /// applied at the given level. Only meaningful when
        /// <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated duration in seconds.</returns>
        public float GetBuffDuration(int skillLevel)
        {
            return buffDurationSeconds + buffDurationPerLevel * skillLevel;
        }
    }
}