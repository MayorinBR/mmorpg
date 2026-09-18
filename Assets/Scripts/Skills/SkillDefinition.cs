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

        [Tooltip("Zeny spent, on top of ManaCost, regardless of level — e.g. Mammonite. Zero for every skill without one (the vast majority).")]
        [SerializeField] private int zenyCost;

        [Header("Damage (used if Effect Type is Damage or Zone)")]
        [SerializeField] private SkillDamageType damageType;
        [SerializeField] private float damageMultiplierPerLevel = 1f;
        [SerializeField] private Element element = Element.Neutral;

        [Tooltip("Extra flat Hit rating this skill's damage roll gets on top of the caster's own Hit, scaling with level — e.g. Bash's real 5-50% Ragnarok Online accuracy bonus. Zero for skills without one.")]
        [SerializeField] private int accuracyBonusPerLevel;

        [Tooltip("If true, this Damage skill hits every living IDamageable within AreaRadius of a center point instead of a single selected target. TargetType decides the center: AreaAroundCaster needs no pre-selected target (e.g. Magnum Break), AreaAroundTarget still needs one in range and centers on it instead (e.g. Fire Ball).")]
        [SerializeField] private bool isAreaOfEffect;

        [Tooltip("Radius, in meters, around the caster that an area-of-effect skill damages (only meaningful when IsAreaOfEffect is true), the radius of a Zone skill's spawned zone (only meaningful when Effect Type is Zone), or the radius a Reveal skill scans around the caster for hidden targets (only meaningful when Effect Type is Reveal).")]
        [SerializeField] private float areaRadius = 3f;

        [Tooltip("Status effect this skill has a chance to inflict on a hit target, in addition to its damage — e.g. Envenom's Poison, Frost Driver's Freeze, Stone Fling/Sand Attack's Stun or Blind. None means this skill never inflicts one.")]
        [SerializeField] private StatusEffectType inflictedStatus;

        [Tooltip("How long InflictedStatus lasts, in seconds, when it procs, before any per-level component. Only meaningful when InflictedStatus is not None.")]
        [SerializeField] private float inflictedStatusDurationSeconds;

        [Tooltip("Extra duration in seconds per skill level, added on top of InflictedStatusDurationSeconds. Only meaningful when InflictedStatus is not None.")]
        [SerializeField] private float inflictedStatusDurationPerLevel;

        [Tooltip("Flat damage per tick, per skill level, when InflictedStatus is Poison — e.g. Envenom's poison DoT. Only meaningful when InflictedStatus is Poison.")]
        [SerializeField] private float poisonDamagePerTickPerLevel;

        [Tooltip("Chance per skill level, from 0 to 1, used by two different mechanics depending on EffectType: for a Damage skill, the chance InflictedStatus actually procs on a hit (e.g. Envenom's poison chance); for an Enemy-targeted Buff (i.e. a debuff, e.g. Decrease AGI), the chance the debuff lands at all instead of being resisted. Left at zero, a Damage skill's InflictedStatus never procs and a debuff always lands — matching every skill authored before this field existed.")]
        [SerializeField] private float successChancePerLevel;

        [Tooltip("How far, in meters, this skill pushes something back via KnockbackUtility: the hit target for a Damage skill (e.g. Arrow Repel) or the caster itself for a Displacement skill (e.g. Back Slide). Zero means a Damage skill has no knockback; a Displacement skill needs this set to actually move.")]
        [SerializeField] private float knockbackDistance;

        [Header("Passive (only used if Effect Type is Passive)")]
        [Tooltip("Flat bonus to Status ATK per skill level, applied automatically while this passive skill is learned and (if PassiveRequiredWeaponSubtypes is non-empty) a matching weapon is equipped in the main hand — e.g. Sword Mastery.")]
        [SerializeField] private float passiveAttackBonusPerLevel;

        [Tooltip("If non-empty, this passive's attack bonus only applies while the equipped main-hand weapon's subtype is one of these — e.g. Sword Mastery requires Dagger or One-Hand Sword. Empty means the bonus always applies once learned.")]
        [SerializeField] private WeaponSubtype[] passiveRequiredWeaponSubtypes;

        [Tooltip("Flat DEX bonus per skill level, applied the same way as PassiveAttackBonusPerLevel — e.g. Owl's Eye.")]
        [SerializeField] private float passiveDexBonusPerLevel;

        [Tooltip("Flat Hit rating bonus per skill level, applied the same way as PassiveAttackBonusPerLevel — e.g. Vulture's Eye.")]
        [SerializeField] private float passiveHitBonusPerLevel;

        [Tooltip("Flat attack range bonus, in meters, per skill level, applied the same way as PassiveAttackBonusPerLevel — e.g. Vulture's Eye.")]
        [SerializeField] private float passiveRangeBonusPerLevel;

        [Tooltip("Flat Flee rating bonus per skill level, applied the same way as PassiveAttackBonusPerLevel, but never weapon-gated — e.g. Improve Dodge.")]
        [SerializeField] private float passiveFleeBonusPerLevel;

        [Tooltip("Flat bonus to max carry weight per skill level, never weapon-gated — e.g. Enlarge Weight Limit. Zero for passives without one.")]
        [SerializeField] private float passiveWeightLimitBonusPerLevel;

        [Tooltip("Flat physical damage bonus per skill level against Demon/Undead race targets only — e.g. Demon Bane. Never weapon-gated. Zero for passives without one.")]
        [SerializeField] private float passiveRaceDamageBonusPerLevel;

        [Tooltip("Flat physical defense bonus per skill level against Demon/Undead race attackers only — e.g. Divine Protection. Never weapon-gated. Zero for passives without one.")]
        [SerializeField] private float passiveRaceDefenseBonusPerLevel;

        [Tooltip("If set, this passive gives a chance to stun whenever the referenced skill lands a hit — e.g. Fatal Blow augmenting Bash. Null means this passive doesn't augment any skill.")]
        [SerializeField] private SkillDefinition augmentsSkill;

        [Tooltip("Stun chance per level of the AUGMENTED skill (not this passive's own level) — e.g. Fatal Blow's 5% per Bash level. Only meaningful when AugmentsSkill is set.")]
        [SerializeField] private float stunChancePerAugmentedLevel;

        [Tooltip("How long the stun lasts, in seconds, when it procs. Only meaningful when AugmentsSkill is set.")]
        [SerializeField] private float stunDurationSeconds;

        [Tooltip("Multiplier bonus to natural HP regen per level, e.g. 0.20 for Increase HP Recovery reaching +100% at this project's max level. Zero for passives without one.")]
        [SerializeField] private float passiveRegenMultiplierPerLevel;

        [Tooltip("Multiplier bonus to natural SP regen per level, e.g. 0.20 for Increase SP Recovery reaching +100% at this project's max level. Zero for passives without one.")]
        [SerializeField] private float passiveSpRegenMultiplierPerLevel;

        [Tooltip("If true and learned (any level), removes the reduced-regen-while-moving penalty — e.g. HP Recovery While Moving.")]
        [SerializeField] private bool passiveRemovesMovementRegenPenalty;

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

        [Tooltip("Flat bonus to STR/AGI/VIT/INT/DEX/LUK per skill level, e.g. Blessing's flat STR/DEX/INT. Zero stats for a buff without a stat component.")]
        [SerializeField] private StatModifiers buffStatBonusPerLevel;

        [Tooltip("Attack speed multiplier bonus per skill level, e.g. Increase AGI's ASPD% component.")]
        [SerializeField] private float buffAspdPercentPerLevel;

        [Tooltip("Flat Max HP bonus per skill level, e.g. Angelus's flat Max HP component.")]
        [SerializeField] private int buffMaxHealthPerLevel;

        [Header("Zone (only used if Effect Type is Zone)")]
        [Tooltip("Optional visual spawned at the zone's position. Left empty, the zone is logic-only — still applies its effect, just invisible, useful before a real wall/fire visual is authored.")]
        [SerializeField] private GameObject zonePrefab;

        [Tooltip("How long the zone lasts, in seconds, before it despawns, before any per-level component — e.g. Fire Wall's duration.")]
        [SerializeField] private float zoneDurationSeconds = 15f;

        [Tooltip("Extra duration in seconds per skill level, added on top of ZoneDurationSeconds.")]
        [SerializeField] private float zoneDurationPerLevel;

        [Tooltip("Seconds between damage ticks against anything standing inside the zone, e.g. Fire Wall's repeated damage while a target stays in it. Ignored when BlocksAttacks is true.")]
        [SerializeField] private float zoneTickIntervalSeconds = 1f;

        [Tooltip("If true, this Zone skill's spawned zone blocks attacks of BlockedWeaponType instead of damaging — e.g. Safety Wall blocking melee, Pneuma blocking ranged. False means the zone only damages, the same as every zone skill before this field existed.")]
        [SerializeField] private bool blocksAttacks;

        [Tooltip("Which weapon range this zone blocks when BlocksAttacks is true — e.g. Safety Wall blocks Melee, Pneuma blocks Ranged. Only meaningful when BlocksAttacks is true.")]
        [SerializeField] private WeaponType blockedWeaponType;

        [Header("Toggle Drain (only used if Effect Type is ToggleDrain)")]
        [Tooltip("Incoming-damage multiplier reduction per skill level while this toggle is active, e.g. 0.06 for Energy Coat reaching -30% incoming damage at this project's max level.")]
        [SerializeField] private float toggleDrainIncomingDamageReductionPercentPerLevel;

        [Tooltip("Flat mana drained per second while this toggle stays active, regardless of level — e.g. Energy Coat's SP drain. Zero for a Toggle skill without an ongoing drain (that's plain Toggle, e.g. Hiding, which only pays ManaCost on/off).")]
        [SerializeField] private float toggleDrainManaPerSecond;

        [Header("Crafting (only used if Effect Type is Craft)")]
        [Tooltip("The recipe this skill produces — e.g. Arrow Crafting's ammo recipe, Aqua Benedicta's holy water recipe. Null means this Craft skill can't actually produce anything.")]
        [SerializeField] private CraftingRecipe craftingRecipe;

        [Tooltip("If true, this Craft skill can only be cast near a Water-layer collider (see PlayerSkillCaster's waterLayer) — e.g. Aqua Benedicta. False means it can be cast anywhere, e.g. Arrow Crafting.")]
        [SerializeField] private bool requiresNearWater;

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

        /// <summary>Gets the Zeny cost to cast this skill, on top of <see cref="ManaCost"/>, regardless of its level. Zero for every skill without one — e.g. Mammonite is the only one today.</summary>
        public int ZenyCost => zenyCost;

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
        /// Damage or Zone.
        /// </summary>
        public Element Element => element;

        /// <summary>
        /// Gets whether this Damage skill hits every living
        /// <see cref="Project.Combat.IDamageable"/> within
        /// <see cref="AreaRadius"/> of a center point instead of a single
        /// pre-selected target. <see cref="TargetType"/> decides the
        /// center: <see cref="SkillTargetType.AreaAroundCaster"/> (no
        /// target needed, e.g. Magnum Break) or
        /// <see cref="SkillTargetType.AreaAroundTarget"/> (needs a target
        /// in range, e.g. Fire Ball). Only meaningful when
        /// <see cref="EffectType"/> is Damage.
        /// </summary>
        public bool IsAreaOfEffect => isAreaOfEffect;

        /// <summary>
        /// Gets the radius, in meters, around the caster that an
        /// area-of-effect skill damages. Only meaningful when
        /// <see cref="IsAreaOfEffect"/> is true.
        /// </summary>
        public float AreaRadius => areaRadius;

        /// <summary>
        /// Gets the status effect this Damage skill has a chance to inflict
        /// on a hit target, in addition to its damage (e.g. Envenom's
        /// Poison). <see cref="StatusEffectType.None"/> means it never
        /// inflicts one. Only meaningful when <see cref="EffectType"/> is Damage.
        /// </summary>
        public StatusEffectType InflictedStatus => inflictedStatus;

        /// <summary>Gets the optional visual prefab spawned at a Zone skill's position. Null means the zone is logic-only. Only meaningful when <see cref="EffectType"/> is Zone.</summary>
        public GameObject ZonePrefab => zonePrefab;

        /// <summary>Gets the interval, in seconds, between a Zone skill's damage ticks against anything standing inside it. Ignored when <see cref="BlocksAttacks"/> is true. Only meaningful when <see cref="EffectType"/> is Zone.</summary>
        public float ZoneTickIntervalSeconds => zoneTickIntervalSeconds;

        /// <summary>Gets whether this Zone skill's spawned zone blocks attacks of <see cref="BlockedWeaponType"/> instead of damaging — e.g. Safety Wall, Pneuma. Only meaningful when <see cref="EffectType"/> is Zone.</summary>
        public bool BlocksAttacks => blocksAttacks;

        /// <summary>Gets which weapon range this zone blocks when <see cref="BlocksAttacks"/> is true. Only meaningful when <see cref="BlocksAttacks"/> is true.</summary>
        public WeaponType BlockedWeaponType => blockedWeaponType;

        /// <summary>
        /// Gets the weapon subtypes this passive's attack bonus requires
        /// the equipped main-hand weapon to match. Empty means the bonus
        /// always applies once the skill is learned. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        public IReadOnlyList<WeaponSubtype> PassiveRequiredWeaponSubtypes => passiveRequiredWeaponSubtypes;

        /// <summary>
        /// Gets the skill this passive augments with a stun chance (e.g.
        /// Fatal Blow augmenting Bash), or null if it doesn't augment any
        /// skill. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        public SkillDefinition AugmentsSkill => augmentsSkill;

        /// <summary>Gets how long the stun lasts, in seconds, when it procs. Only meaningful when <see cref="AugmentsSkill"/> is set.</summary>
        public float StunDurationSeconds => stunDurationSeconds;

        /// <summary>Gets whether this passive removes the reduced-regen-while-moving penalty once learned. Only meaningful when <see cref="EffectType"/> is Passive.</summary>
        public bool PassiveRemovesMovementRegenPenalty => passiveRemovesMovementRegenPenalty;

        /// <summary>
        /// Calculates this skill's damage at the given level. Only meaningful when <see cref="EffectType"/> is Damage or Zone.
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
        /// Calculates how long, in seconds, <see cref="InflictedStatus"/>
        /// lasts when it procs at the given level. Only meaningful when
        /// <see cref="InflictedStatus"/> is not <see cref="StatusEffectType.None"/>.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated duration in seconds.</returns>
        public float GetInflictedStatusDuration(int skillLevel)
        {
            return inflictedStatusDurationSeconds + inflictedStatusDurationPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat damage per Poison tick at the given level.
        /// Only meaningful when <see cref="InflictedStatus"/> is
        /// <see cref="StatusEffectType.Poison"/>.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated tick damage.</returns>
        public int GetPoisonDamagePerTick(int skillLevel)
        {
            return Mathf.RoundToInt(poisonDamagePerTickPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the chance, from 0 to 1, for <see cref="InflictedStatus"/>
        /// to proc on a hit (Damage skills) or for an enemy-targeted debuff
        /// to land instead of being resisted (Buff skills), at the given
        /// level. Zero means the effect is unconditional — a Damage skill's
        /// InflictedStatus never procs, or a debuff always lands — matching
        /// every skill authored before this field existed.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated chance, from 0 to 1.</returns>
        public float GetSuccessChance(int skillLevel)
        {
            return successChancePerLevel * skillLevel;
        }

        /// <summary>Gets how far, in meters, this skill pushes something back via <see cref="Combat.KnockbackUtility"/>. Zero for a Damage skill without knockback; meaningful for every Displacement skill.</summary>
        public float KnockbackDistance => knockbackDistance;

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
        /// Calculates the flat DEX bonus this passive skill grants at the
        /// given level, before <see cref="PassiveRequiredWeaponSubtypes"/>
        /// is checked against the equipped weapon. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated DEX bonus, zero for passives without one.</returns>
        public int GetPassiveDexBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveDexBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat Hit rating bonus this passive skill grants
        /// at the given level, before
        /// <see cref="PassiveRequiredWeaponSubtypes"/> is checked against
        /// the equipped weapon. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated Hit bonus, zero for passives without one.</returns>
        public int GetPassiveHitBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveHitBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat attack range bonus, in meters, this passive
        /// skill grants at the given level, before
        /// <see cref="PassiveRequiredWeaponSubtypes"/> is checked against
        /// the equipped weapon. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated range bonus, zero for passives without one.</returns>
        public float GetPassiveRangeBonus(int skillLevel)
        {
            return passiveRangeBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat Flee rating bonus this passive skill grants
        /// at the given level. Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated Flee bonus, zero for passives without one.</returns>
        public int GetPassiveFleeBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveFleeBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat max carry weight bonus this passive skill
        /// grants at the given level (e.g. Enlarge Weight Limit). Never
        /// weapon-gated. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus, zero for passives without one.</returns>
        public float GetPassiveWeightLimitBonus(int skillLevel)
        {
            return passiveWeightLimitBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat physical damage bonus this passive skill
        /// grants at the given level against Demon/Undead race targets
        /// (e.g. Demon Bane). Only meaningful when <see cref="EffectType"/>
        /// is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated damage bonus, zero for passives without one.</returns>
        public int GetPassiveRaceDamageBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveRaceDamageBonusPerLevel * skillLevel);
        }

        /// <summary>
        /// Calculates the flat physical defense bonus this passive skill
        /// grants at the given level against Demon/Undead race attackers
        /// (e.g. Divine Protection). Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated defense bonus, zero for passives without one.</returns>
        public int GetPassiveRaceDefenseBonus(int skillLevel)
        {
            return Mathf.RoundToInt(passiveRaceDefenseBonusPerLevel * skillLevel);
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
        /// Calculates the flat STR/AGI/VIT/INT/DEX/LUK bonus this
        /// buff/debuff skill grants at the given level (e.g. Blessing's
        /// flat STR/DEX/INT). Only meaningful when <see cref="EffectType"/>
        /// is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero stats for a buff without a stat component.</returns>
        public StatModifiers GetBuffStatBonus(int skillLevel)
        {
            return buffStatBonusPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the attack speed multiplier bonus this buff/debuff
        /// skill applies at the given level (e.g. Increase AGI's ASPD%
        /// component). Only meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without an ASPD component.</returns>
        public float GetBuffAspdPercent(int skillLevel)
        {
            return buffAspdPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the flat Max HP bonus this buff/debuff skill grants
        /// at the given level (e.g. Angelus's flat Max HP component). Only
        /// meaningful when <see cref="EffectType"/> is Buff.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a buff without a Max HP component.</returns>
        public int GetBuffMaxHealthBonus(int skillLevel)
        {
            return buffMaxHealthPerLevel * skillLevel;
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

        /// <summary>
        /// Calculates how long, in seconds, this Zone skill's spawned zone
        /// lasts before despawning at the given level. Only meaningful when
        /// <see cref="EffectType"/> is Zone.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated duration in seconds.</returns>
        public float GetZoneDuration(int skillLevel)
        {
            return zoneDurationSeconds + zoneDurationPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the chance for this passive's stun to proc, scaled by
        /// the CURRENT level of the skill it augments (e.g. Fatal Blow's
        /// chance scales with Bash's level, not Fatal Blow's own — Fatal
        /// Blow is single-rank). Only meaningful when
        /// <see cref="AugmentsSkill"/> is set.
        /// </summary>
        /// <param name="augmentedSkillLevel">The current level of the skill being augmented.</param>
        /// <returns>The calculated stun chance, from 0 to 1.</returns>
        public float GetStunChance(int augmentedSkillLevel)
        {
            return stunChancePerAugmentedLevel * augmentedSkillLevel;
        }

        /// <summary>
        /// Calculates this passive's bonus to natural HP regen at the given
        /// level (e.g. 0.4 for Increase HP Recovery at level 2, a +40%
        /// bonus). Summed across every learned regen passive by the
        /// player's passive-skill controller. Only meaningful when
        /// <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a passive without a regen component.</returns>
        public float GetPassiveRegenBonus(int skillLevel)
        {
            return passiveRegenMultiplierPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates this passive's bonus to natural SP regen at the given
        /// level (e.g. 0.4 for Increase SP Recovery at level 2, a +40%
        /// bonus), mirroring <see cref="GetPassiveRegenBonus"/> exactly for
        /// mana instead of health. Only meaningful when <see cref="EffectType"/> is Passive.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated bonus. Zero for a passive without an SP regen component.</returns>
        public float GetPassiveSpRegenBonus(int skillLevel)
        {
            return passiveSpRegenMultiplierPerLevel * skillLevel;
        }

        /// <summary>
        /// Calculates the incoming-damage multiplier reduction this
        /// toggle-drain skill grants while active, at the given level (e.g.
        /// 0.3 for Energy Coat at this project's max level, a 30%
        /// reduction). Only meaningful when <see cref="EffectType"/> is
        /// <see cref="SkillEffectType.ToggleDrain"/>.
        /// </summary>
        /// <param name="skillLevel">The skill's current level (1 or higher).</param>
        /// <returns>The calculated reduction, from 0 to 1.</returns>
        public float GetToggleDrainIncomingDamageReductionPercent(int skillLevel)
        {
            return toggleDrainIncomingDamageReductionPercentPerLevel * skillLevel;
        }

        /// <summary>
        /// Gets the flat mana drained per second while this toggle-drain
        /// skill stays active, regardless of level (e.g. Energy Coat's SP
        /// drain). Only meaningful when <see cref="EffectType"/> is
        /// <see cref="SkillEffectType.ToggleDrain"/>.
        /// </summary>
        public float ToggleDrainManaPerSecond => toggleDrainManaPerSecond;

        /// <summary>
        /// Gets the recipe this Craft skill produces (e.g. Arrow Crafting,
        /// Aqua Benedicta). Null means this skill can't actually produce
        /// anything. Only meaningful when <see cref="EffectType"/> is
        /// <see cref="SkillEffectType.Craft"/>.
        /// </summary>
        public CraftingRecipe CraftingRecipe => craftingRecipe;

        /// <summary>
        /// Gets whether this Craft skill requires the caster be near a
        /// Water-layer collider to succeed (e.g. Aqua Benedicta). Only
        /// meaningful when <see cref="EffectType"/> is
        /// <see cref="SkillEffectType.Craft"/>.
        /// </summary>
        public bool RequiresNearWater => requiresNearWater;
    }
}