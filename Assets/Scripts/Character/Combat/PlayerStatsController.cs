using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using Project.Persistence;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Owns the player's base stats and exposes calculated sub-stats for
    /// combat. Acts as the composition root connecting the plain C# stat
    /// classes to the Unity component world. When an
    /// <see cref="EquipmentManager"/> is assigned, equipped item bonuses are
    /// included in the sub-stats calculation. Also implements
    /// <see cref="IMaxHealthBonusProvider"/> and <see cref="IMaxManaBonusProvider"/>
    /// so VIT and INT give the player extra HP and SP, the same way they do
    /// in Ragnarok Online, without <see cref="Project.Combat.HealthComponent"/>
    /// or <see cref="Project.Combat.ManaComponent"/> needing to know stats exist.
    /// Also implements <see cref="IDefensiveStatsProvider"/> so the player's
    /// DEX/AGI/VIT/INT-derived physical defense, magical defense and flee
    /// rating (see <see cref="CurrentSubStats"/>) feed into
    /// <see cref="Project.Combat.HealthComponent"/> the same optional-hook way.
    /// When <see cref="health"/>/<see cref="mana"/> are assigned, <see cref="TryIncreaseStat"/>
    /// and equipment changes both push a refresh to them, so their max
    /// value (and any UI bound to it) picks up a VIT/INT change immediately
    /// instead of waiting for the next damage, heal or mana spend. When
    /// <see cref="passiveSkills"/> is assigned, <see cref="CurrentSubStats"/>
    /// also folds in any flat Status ATK bonus from learned passive
    /// skills whose weapon requirement matches the equipped main-hand
    /// weapon (e.g. Sword Mastery). When <see cref="buffs"/> is assigned,
    /// its combined <see cref="Project.Combat.BuffController.Total"/>
    /// payload (e.g. Berserk's persistent ATK/DEF, or a future Blessing's
    /// flat STR/DEX/INT) flows through <see cref="CurrentSubStats"/> and
    /// <see cref="GetMaxHealthBonus"/> the same way equipment and Job
    /// Level bonuses do: its STR/AGI/VIT/INT/DEX/LUK component is folded
    /// into <see cref="effectiveStats"/> through another
    /// <see cref="JobBonusStatsView"/> layer, its ATK%/ASPD% components
    /// multiply <see cref="CurrentSubStats"/>, and its flat Max HP adds to
    /// <see cref="GetMaxHealthBonus"/>. When <see cref="classController"/>,
    /// <see cref="jobProgress"/> and <see cref="jobLevelBonusLookup"/> are
    /// all assigned, the class's Job Level stat bonus (see
    /// <see cref="ClassJobLevelBonusLookup"/>) is folded into every derived
    /// stat the same way equipment is, through <see cref="JobBonusStatsView"/>.
    /// </summary>
    public class PlayerStatsController : MonoBehaviour, IPlayerLevelProvider, ISaveParticipant, IMaxHealthBonusProvider, IMaxManaBonusProvider, IDefensiveStatsProvider
    {
        // Real Ragnarok Online value (source: iRO Wiki Classic — Stats,
        // consulted September 2026): a fresh level-1 character starts with
        // 48 unspent stat points, not the flat placeholder of 10 used
        // before this was researched.
        private const int RealRagnarokStartingStatPoints = 48;

        // Real Ragnarok Online per-point bonus: max HP/SP scale up by 1%
        // for every point of VIT/INT respectively (source: iRO Wiki
        // Classic — Stats, consulted September 2026).
        private const float MaxHealthBonusPerVit = 0.01f;
        private const float MaxManaBonusPerInt = 0.01f;

        // Used only if statsHolder (or its Stats asset) isn't wired, so a
        // missing reference degrades gracefully instead of throwing.
        // Matches CharacterStatsDefinition.BaseAttackSpeed's own default.
        private const int FallbackBaseAttackSpeed = 140;

        [SerializeField] private int startingLevel = 1;
        [SerializeField] private int startingStatPoints = RealRagnarokStartingStatPoints;

        [Tooltip("Source of the player's base Aspd (see CharacterStatsDefinition.BaseAttackSpeed) and other shared base stats.")]
        [SerializeField] private CharacterStatsHolder statsHolder;

        [SerializeField] private EquipmentManager equipment;

        [Tooltip("Optional. When assigned, refreshed automatically whenever a stat point is spent or equipment changes, so the HP/SP bars pick up VIT/INT's bonus without waiting for the next damage, heal or mana spend.")]
        [SerializeField] private HealthComponent health;
        [SerializeField] private ManaComponent mana;

        [Tooltip("Optional. Refreshed automatically whenever a skill levels up (alongside health/mana), so a learned Enlarge Weight Limit passive's carry-weight bonus applies immediately. Left empty, or when passiveSkills isn't wired, carry weight never benefits from a passive.")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Source of flat Status ATK bonuses from learned passive skills (e.g. Sword Mastery), added on top of the stat-derived value in CurrentSubStats whenever the equipped weapon matches.")]
        [SerializeField] private PlayerPassiveSkillController passiveSkills;

        [Tooltip("Optional. Source of temporary/persistent buffs (e.g. Berserk), folded into CurrentSubStats and GetMaxHealthBonus. Left empty, the player is never affected by one.")]
        [SerializeField] private BuffController buffs;

        [Tooltip("Optional, all three required together. Source of the class's automatic Job Level stat bonus (e.g. Swordman's +7 STR at Job 50), folded into every derived stat alongside equipment.")]
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerJobProgress jobProgress;
        [SerializeField] private ClassJobLevelBonusLookup jobLevelBonusLookup;

        private CharacterBaseStats baseStats;
        private ISubStatsCalculator subStatsCalculator;
        private IStatProvider effectiveStats;

        /// <summary>
        /// Raised whenever a stat point is spent, stats are reset, a save is
        /// restored, or equipment changes — anything that can move
        /// <see cref="CurrentSubStats"/>. Stat-derived UI (the stat
        /// allocation panel, the sub-stats panel) subscribes to this instead
        /// of each caller having to remember to refresh it manually.
        /// </summary>
        public event Action StatsChanged;

        /// <summary>Gets the player's base stat block (STR, AGI, VIT, INT, DEX, LUK).</summary>
        public CharacterBaseStats BaseStats => baseStats;

        /// <summary>Gets or sets the player's current base level, driven by the experience system.</summary>
        public int BaseLevel { get; set; }

        /// <summary>Gets the base level a fresh character starts at, e.g. for <see cref="PlayerExperience.ResetProgress"/> to reset back to.</summary>
        public int StartingLevel => startingLevel;

        /// <summary>Gets the sub-stats calculated from the current effective stats, level and equipped weapon type.</summary>
        public SubStats CurrentSubStats
        {
            get
            {
                EnsureInitialized();
                var subStats = subStatsCalculator.Calculate(effectiveStats, BaseLevel, equipment != null && equipment.IsMainHandWeaponRanged(), GetBaseAttackSpeed());
                var statusAtk = subStats.StatusAtk + GetPassiveAttackBonus();
                var hit = subStats.Hit + GetPassiveHitBonus();
                var flee = subStats.Flee + GetPassiveFleeBonus();
                var aspd = subStats.Aspd;

                if (buffs != null)
                {
                    statusAtk = Mathf.RoundToInt(statusAtk * buffs.AttackMultiplier);
                    aspd = Mathf.RoundToInt(aspd * buffs.AspdMultiplier);
                }

                return statusAtk == subStats.StatusAtk && hit == subStats.Hit && flee == subStats.Flee && aspd == subStats.Aspd
                    ? subStats
                    : new SubStats(statusAtk, subStats.StatusMatk, subStats.StatusDef, subStats.StatusMDef, hit, flee, subStats.CriticalRate, aspd);
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void OnDestroy()
        {
            if (equipment != null)
            {
                equipment.EquipmentChanged -= HandleEquipmentChanged;
            }

            if (jobProgress != null)
            {
                jobProgress.JobLeveledUp -= HandleJobLeveledUp;
            }

            if (passiveSkills != null && passiveSkills.SkillBook != null)
            {
                passiveSkills.SkillBook.SkillLeveledUp -= HandleSkillLeveledUp;
            }
        }

        /// <summary>
        /// Sets up the stat block on first use. Called from <see cref="Awake"/>
        /// for the normal startup path, but also guarded at the top of every
        /// other public member: <see cref="Project.Combat.HealthComponent"/>
        /// and <see cref="Project.Combat.ManaComponent"/> read their max
        /// value (and so call <see cref="GetMaxHealthBonus"/>/
        /// <see cref="GetMaxManaBonus"/>) from their own <c>Awake</c>, and
        /// Unity does not guarantee that this component's <c>Awake</c> runs
        /// first.
        /// </summary>
        private void EnsureInitialized()
        {
            if (baseStats != null)
            {
                return;
            }

            BaseLevel = startingLevel;
            baseStats = new CharacterBaseStats(new RagnarokStatPointCostStrategy());
            baseStats.GrantPoints(startingStatPoints);
            subStatsCalculator = new SubStatsCalculator();

            IStatProvider derivedStats = equipment != null ? new EquippedStatsView(baseStats, equipment) : baseStats;
            var canApplyJobBonus = classController != null && jobProgress != null && jobLevelBonusLookup != null;
            IStatProvider jobStats = canApplyJobBonus
                ? new JobBonusStatsView(derivedStats, () => jobLevelBonusLookup.GetBonus(classController.CurrentClass, jobProgress.JobLevel))
                : derivedStats;
            effectiveStats = buffs != null
                ? new JobBonusStatsView(jobStats, () => ToStatModifiers(buffs.Total))
                : jobStats;

            if (passiveSkills != null && equipment != null)
            {
                effectiveStats = new JobBonusStatsView(effectiveStats, () => new StatModifiers(0, 0, 0, 0, GetPassiveDexBonus(), 0));
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged += HandleEquipmentChanged;
            }

            if (jobProgress != null)
            {
                jobProgress.JobLeveledUp += HandleJobLeveledUp;
            }

            if (passiveSkills != null && passiveSkills.SkillBook != null)
            {
                passiveSkills.SkillBook.SkillLeveledUp += HandleSkillLeveledUp;
            }
        }

        private void HandleEquipmentChanged()
        {
            RefreshDependentMaxValues();
            StatsChanged?.Invoke();
        }

        private void HandleJobLeveledUp(int newJobLevel)
        {
            RefreshDependentMaxValues();
            StatsChanged?.Invoke();
        }

        /// <summary>
        /// Refreshes derived stats whenever a learned skill changes level —
        /// covers a passive's stat bonus changing (e.g. Sword Mastery,
        /// Owl's Eye) and <see cref="PlayerSkillBook.ResetLearnedSkills"/>
        /// resetting every skill back to level 0.
        /// </summary>
        /// <param name="skill">The skill that changed level.</param>
        /// <param name="level">The skill's new level (0 when reset).</param>
        private void HandleSkillLeveledUp(SkillDefinition skill, int level)
        {
            RefreshDependentMaxValues();
            StatsChanged?.Invoke();
        }

        /// <summary>
        /// Attempts to spend one available point raising the given stat.
        /// Prefer this over reaching into <see cref="BaseStats"/> directly
        /// (<see cref="CharacterBaseStats.TryIncreaseStat"/>) since VIT and
        /// INT feed <see cref="GetMaxHealthBonus"/>/<see cref="GetMaxManaBonus"/>,
        /// and nothing else notifies <see cref="Project.Combat.HealthComponent"/>/
        /// <see cref="Project.Combat.ManaComponent"/> that a stat just changed —
        /// going through here keeps the HP/SP bars in sync automatically.
        /// </summary>
        /// <param name="stat">The stat to raise.</param>
        /// <returns>True if the stat was raised; false if it's already at its maximum or there aren't enough available points.</returns>
        public bool TryIncreaseStat(StatType stat)
        {
            EnsureInitialized();

            if (!baseStats.TryIncreaseStat(stat))
            {
                return false;
            }

            RefreshDependentMaxValues();
            StatsChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Resets every base stat back to its minimum and refunds all spent
        /// points, then refreshes and fully restores HP/SP — resetting VIT/INT
        /// can only shrink their max value, so topping up avoids leaving the
        /// player above their new maximum. A simplified respec: unlike
        /// Ragnarok Online's own NPC-driven reset, this has no Zeny or item
        /// cost today (see FUTURE_IMPROVEMENTS.md for that possible follow-up).
        /// Publishes a <see cref="PlayerFeedbackChannel"/> message reporting
        /// how many points were refunded.
        /// </summary>
        /// <returns>The total number of points refunded.</returns>
        public int ResetStats()
        {
            EnsureInitialized();

            var refunded = baseStats.ResetToMinimum();
            RefreshDependentMaxValues();
            health?.ResetHealth();
            mana?.ResetMana();

            PlayerFeedbackChannel.Publish($"Stats reset: {refunded} points refunded.");
            StatsChanged?.Invoke();
            return refunded;
        }

        private void RefreshDependentMaxValues()
        {
            health?.RefreshMaxHealth();
            mana?.RefreshMaxMana();

            if (inventory != null && inventory.Items != null && passiveSkills != null)
            {
                inventory.Items.RefreshWeightCapacityBonus(passiveSkills.GetWeightLimitBonus());
            }
        }

        /// <summary>
        /// Reads the base Aspd from <see cref="statsHolder"/>'s
        /// <see cref="CharacterStatsDefinition"/>, falling back to
        /// <see cref="FallbackBaseAttackSpeed"/> if either isn't wired.
        /// </summary>
        private int GetBaseAttackSpeed()
        {
            return statsHolder != null && statsHolder.Stats != null
                ? statsHolder.Stats.BaseAttackSpeed
                : FallbackBaseAttackSpeed;
        }

        /// <summary>
        /// Reads the flat Status ATK bonus from learned passive skills
        /// (see <see cref="PlayerPassiveSkillController"/>) that apply to
        /// the currently equipped main-hand weapon. Zero if either
        /// <see cref="passiveSkills"/> or <see cref="equipment"/> isn't wired.
        /// </summary>
        private int GetPassiveAttackBonus()
        {
            if (passiveSkills == null || equipment == null)
            {
                return 0;
            }

            return passiveSkills.GetAttackBonus(equipment.GetMainHandWeaponSubtype());
        }

        /// <summary>
        /// Reads the flat DEX bonus from learned passive skills (e.g. Owl's
        /// Eye) that apply to the currently equipped main-hand weapon.
        /// Zero if either <see cref="passiveSkills"/> or <see cref="equipment"/>
        /// isn't wired.
        /// </summary>
        private int GetPassiveDexBonus()
        {
            if (passiveSkills == null || equipment == null)
            {
                return 0;
            }

            return passiveSkills.GetDexBonus(equipment.GetMainHandWeaponSubtype());
        }

        /// <summary>
        /// Reads the flat Hit bonus from learned passive skills (e.g.
        /// Vulture's Eye) that apply to the currently equipped main-hand
        /// weapon. Zero if either <see cref="passiveSkills"/> or
        /// <see cref="equipment"/> isn't wired.
        /// </summary>
        private int GetPassiveHitBonus()
        {
            if (passiveSkills == null || equipment == null)
            {
                return 0;
            }

            return passiveSkills.GetHitBonus(equipment.GetMainHandWeaponSubtype());
        }

        /// <summary>
        /// Reads the flat Flee bonus from learned passive skills (e.g.
        /// Improve Dodge). Zero if <see cref="passiveSkills"/> isn't wired
        /// — unlike the other passive bonuses, this one isn't weapon-gated,
        /// but still routes through <see cref="PlayerPassiveSkillController"/>
        /// for a consistent single place that sums learned passives.
        /// </summary>
        private int GetPassiveFleeBonus()
        {
            if (passiveSkills == null)
            {
                return 0;
            }

            return passiveSkills.GetFleeBonus(equipment != null ? equipment.GetMainHandWeaponSubtype() : WeaponSubtype.Unarmed);
        }

        /// <summary>
        /// Gets the flat attack range bonus, in meters, granted by learned
        /// passive skills (e.g. Vulture's Eye) for the currently equipped
        /// main-hand weapon. Read by
        /// <see cref="Character.Combat.PlayerCombatController"/> to extend
        /// the player's effective attack range.
        /// </summary>
        /// <returns>The calculated range bonus, zero if no passive applies.</returns>
        public float GetPassiveRangeBonus()
        {
            if (passiveSkills == null || equipment == null)
            {
                return 0f;
            }

            return passiveSkills.GetRangeBonus(equipment.GetMainHandWeaponSubtype());
        }

        /// <summary>
        /// Converts a <see cref="Project.Combat.BuffPayload"/>'s
        /// STR/AGI/VIT/INT/DEX/LUK component into a <see cref="StatModifiers"/>,
        /// so it can be folded into <see cref="effectiveStats"/> through
        /// <see cref="JobBonusStatsView"/> the same way equipment and Job
        /// Level bonuses are. Lives here rather than on
        /// <see cref="Project.Combat.BuffPayload"/> itself since
        /// Project.Combat cannot reference Project.Items (see
        /// <see cref="Project.Combat.BuffPayload"/>'s own remarks).
        /// </summary>
        private static StatModifiers ToStatModifiers(BuffPayload payload)
        {
            return new StatModifiers(payload.Strength, payload.Agility, payload.Vitality, payload.Intelligence, payload.Dexterity, payload.Luck);
        }

        /// <inheritdoc />
        public int GetMaxHealthBonus(int baseMaxHealth)
        {
            EnsureInitialized();
            var bonus = Mathf.RoundToInt(baseMaxHealth * effectiveStats.GetValue(StatType.Vitality) * MaxHealthBonusPerVit);
            return buffs != null ? bonus + buffs.MaxHealthBonus : bonus;
        }

        /// <inheritdoc />
        public int GetMaxManaBonus(int baseMaxMana)
        {
            EnsureInitialized();
            return Mathf.RoundToInt(baseMaxMana * effectiveStats.GetValue(StatType.Intelligence) * MaxManaBonusPerInt);
        }

        /// <inheritdoc />
        public int GetPhysicalDefense()
        {
            EnsureInitialized();
            return CurrentSubStats.StatusDef;
        }

        /// <inheritdoc />
        public int GetMagicalDefense()
        {
            EnsureInitialized();
            return CurrentSubStats.StatusMDef;
        }

        /// <inheritdoc />
        public int GetFleeRating()
        {
            EnsureInitialized();
            return CurrentSubStats.Flee;
        }

        /// <inheritdoc />
        public int GetRaceDefenseBonus(MonsterRace attackerRace)
        {
            return passiveSkills != null ? passiveSkills.GetRaceDefenseBonus(attackerRace) : 0;
        }

        /// <summary>
        /// Gets the flat physical damage bonus from learned "race bane"
        /// passive skills (e.g. Demon Bane) against the given target's
        /// race. Zero if <see cref="passiveSkills"/> isn't wired. Called
        /// per-hit by whoever is dealing the damage (<see cref="PlayerCombatController"/>,
        /// <see cref="PlayerSkillCaster"/>) rather than folded into
        /// <see cref="CurrentSubStats"/>, since — unlike every other
        /// passive bonus here — it depends on which specific target is
        /// being hit, not a flat, always-on modifier.
        /// </summary>
        /// <param name="targetRace">The race of the target being hit.</param>
        public int GetRaceDamageBonus(MonsterRace targetRace)
        {
            return passiveSkills != null ? passiveSkills.GetRaceDamageBonus(targetRace) : 0;
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            EnsureInitialized();
            data.strength = baseStats.GetValue(StatType.Strength);
            data.agility = baseStats.GetValue(StatType.Agility);
            data.vitality = baseStats.GetValue(StatType.Vitality);
            data.intelligence = baseStats.GetValue(StatType.Intelligence);
            data.dexterity = baseStats.GetValue(StatType.Dexterity);
            data.luck = baseStats.GetValue(StatType.Luck);
            data.availableStatPoints = baseStats.AvailablePoints;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Runs from <c>Start()</c> (see <c>PlayerSaveController</c>), after
        /// every component's own <c>Awake()</c> — so unlike the
        /// <see cref="EnsureInitialized"/> Awake-ordering concern, <see cref="health"/>/
        /// <see cref="mana"/> are guaranteed already set up here. Restoring
        /// a saved VIT/INT can raise the max HP/SP bonus above what
        /// <see cref="Project.Combat.HealthComponent"/>/<see cref="Project.Combat.ManaComponent"/>
        /// computed at their own <c>Awake()</c> (before this ran), so both
        /// are healed to full afterward — the save doesn't carry a current
        /// HP/SP value of its own, so appearing at full is the correct
        /// baseline, the same as a fresh level up.
        /// </remarks>
        public void RestoreState(PlayerSaveData data)
        {
            EnsureInitialized();
            baseStats.SetValue(StatType.Strength, data.strength);
            baseStats.SetValue(StatType.Agility, data.agility);
            baseStats.SetValue(StatType.Vitality, data.vitality);
            baseStats.SetValue(StatType.Intelligence, data.intelligence);
            baseStats.SetValue(StatType.Dexterity, data.dexterity);
            baseStats.SetValue(StatType.Luck, data.luck);
            baseStats.SetAvailablePoints(data.availableStatPoints);

            health?.ResetHealth();
            mana?.ResetMana();
            RefreshDependentMaxValues();
            StatsChanged?.Invoke();
        }
    }
}
