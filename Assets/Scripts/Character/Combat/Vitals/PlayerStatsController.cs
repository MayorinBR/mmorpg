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
    /// Also implements <see cref="Project.Combat.IStatusResistanceProvider"/>
    /// so the player's VIT gives a chance to fully resist an incoming
    /// status effect, the same optional-hook way, feeding into
    /// <see cref="Project.Combat.StatusEffectController"/>.
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
    public class PlayerStatsController : MonoBehaviour, IPlayerLevelProvider, ISaveParticipant, IMaxHealthBonusProvider, IMaxManaBonusProvider, IDefensiveStatsProvider, IStatusResistanceProvider
    {
        // Real Ragnarok Online value (source: iRO Wiki Classic — Stats,
        // consulted September 2026): a fresh level-1 character starts with
        // 48 unspent stat points, not the flat placeholder of 10 used
        // before this was researched.
        private const int RealRagnarokStartingStatPoints = 48;

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

        [Tooltip("Scales the Ragnarok Online carry weight (2000 + 30 * base STR + class modifier) down to this project's item weights, which are roughly a twentieth of Ragnarok's.")]
        [SerializeField] private float carryWeightScale = 0.05f;

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
        private SubStats cachedSubStats;
        private int cachedSubStatsFrame = -1;
        private int baseLevel;

        private PassiveStatBonuses Passives => new PassiveStatBonuses(passiveSkills, equipment);

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
        public int BaseLevel
        {
            get => baseLevel;
            set
            {
                baseLevel = value;
                InvalidateSubStatsCache();
            }
        }

        /// <summary>Gets the base level a fresh character starts at, e.g. for <see cref="PlayerExperience.ResetProgress"/> to reset back to.</summary>
        public int StartingLevel => startingLevel;

        /// <summary>Gets the extra Fire damage, as a fraction of each hit, that the player's active buffs currently add to every attack (0 when none).</summary>
        public float FireDamagePercent => buffs != null ? buffs.FireDamagePercent : 0f;

        /// <summary>Gets the Aspd rating after applying the equipped main-hand weapon's attack speed multiplier, matching the real auto-attack interval.</summary>
        public int EffectiveAspd
        {
            get
            {
                var weaponMultiplier = equipment != null ? equipment.GetMainHandAttackSpeedMultiplier() : 1f;
                return AttackSpeedCalculator.GetEffectiveAspd(CurrentSubStats.Aspd, weaponMultiplier);
            }
        }

        /// <summary>Gets the sub-stats calculated from the current effective stats, level and equipped weapon type.</summary>
        public SubStats CurrentSubStats
        {
            get
            {
                if (!Application.isPlaying)
                {
                    return ComputeSubStats();
                }

                if (cachedSubStatsFrame != Time.frameCount)
                {
                    cachedSubStats = ComputeSubStats();
                    cachedSubStatsFrame = Time.frameCount;
                }

                return cachedSubStats;
            }
        }

        /// <summary>
        /// Computes the sub-stats from the current stats, level, passives, weapon and buffs. Buffs expire on the clock
        /// without raising an event, so <see cref="CurrentSubStats"/> reuses this result for one frame only.
        /// </summary>
        /// <returns>The freshly computed sub-stats.</returns>
        private SubStats ComputeSubStats()
        {
            EnsureInitialized();
            var raw = subStatsCalculator.Calculate(effectiveStats, BaseLevel, equipment != null && equipment.IsMainHandWeaponRanged(), GetBaseAttackSpeed());
            return SubStatsAdjuster.Apply(raw, equipment, Passives, buffs);
        }

        /// <summary>
        /// Gets the Def shown to the player: the status Def with active buff multipliers
        /// (e.g. Provoke) applied, matching what incoming physical hits are mitigated by.
        /// </summary>
        public int EffectiveDef
        {
            get
            {
                var statusDef = CurrentSubStats.StatusDef;
                return buffs != null ? Mathf.RoundToInt(statusDef * buffs.DefenseMultiplier) : statusDef;
            }
        }

        /// <summary>
        /// Gets the MDef shown to the player: the status MDef plus the flat bonus of active buffs
        /// (e.g. Endure), matching what incoming magical hits are mitigated by.
        /// <see cref="CurrentSubStats"/> deliberately excludes it, because the health component adds the buff bonus itself.
        /// </summary>
        public int EffectiveMDef
        {
            get
            {
                var statusMDef = CurrentSubStats.StatusMDef;
                return buffs != null ? statusMDef + buffs.MagicalDefenseBonus : statusMDef;
            }
        }

        /// <summary>Gets the effective value of a base stat, including equipment, job, buff and passive bonuses.</summary>
        /// <param name="stat">The stat to read.</param>
        /// <returns>The effective stat value.</returns>
        public int GetEffectiveStat(StatType stat)
        {
            EnsureInitialized();
            return effectiveStats.GetValue(stat);
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            RefreshDependentMaxValues();
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

            if (classController != null)
            {
                classController.ClassChanged -= HandleClassChanged;
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
                effectiveStats = new JobBonusStatsView(effectiveStats, () => new StatModifiers(0, 0, 0, 0, Passives.DexBonus, 0));
            }

            if (equipment != null)
            {
                equipment.EquipmentChanged += HandleEquipmentChanged;
            }

            if (jobProgress != null)
            {
                jobProgress.JobLeveledUp += HandleJobLeveledUp;
            }

            if (classController != null)
            {
                classController.ClassChanged += HandleClassChanged;
            }

            if (passiveSkills != null && passiveSkills.SkillBook != null)
            {
                passiveSkills.SkillBook.SkillLeveledUp += HandleSkillLeveledUp;
            }
        }

        /// <summary>Discards the cached <see cref="CurrentSubStats"/> so the next read recomputes it.</summary>
        private void InvalidateSubStatsCache()
        {
            cachedSubStatsFrame = -1;
        }

        /// <summary>Discards the cached sub-stats and raises <see cref="StatsChanged"/>.</summary>
        private void NotifyStatsChanged()
        {
            InvalidateSubStatsCache();
            StatsChanged?.Invoke();
        }

        private void HandleEquipmentChanged()
        {
            RefreshDependentMaxValues();
            NotifyStatsChanged();
        }

        private void HandleClassChanged(CharacterClass newClass)
        {
            RefreshDependentMaxValues();
            NotifyStatsChanged();
        }

        private void HandleJobLeveledUp(int newJobLevel)
        {
            RefreshDependentMaxValues();
            NotifyStatsChanged();
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
            NotifyStatsChanged();
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
            NotifyStatsChanged();
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
            NotifyStatsChanged();
            return refunded;
        }

        private void RefreshDependentMaxValues()
        {
            health?.RefreshMaxHealth();
            mana?.RefreshMaxMana();

            if (inventory == null || inventory.Items == null)
            {
                return;
            }

            inventory.Items.RefreshWeightCapacityBonus(Passives.WeightLimitBonus * carryWeightScale);
            inventory.Items.SetBaseMaxCarryWeight(GetBaseCarryWeight());
        }

        /// <summary>
        /// Calculates the base carry weight from base STR (equipment and
        /// buffs do not count, as in Ragnarok Online) and the current
        /// class, scaled by <see cref="carryWeightScale"/>.
        /// </summary>
        private float GetBaseCarryWeight()
        {
            var jobModifier = classController != null ? WeightCapacityCalculator.GetJobModifier(classController.CurrentClass) : 0f;
            return WeightCapacityCalculator.Calculate(baseStats.GetValue(StatType.Strength), jobModifier) * carryWeightScale;
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
        /// Gets the flat attack range bonus, in meters, granted by learned
        /// passive skills (e.g. Vulture's Eye) for the currently equipped
        /// main-hand weapon. Read by
        /// <see cref="Character.Combat.PlayerCombatController"/> to extend
        /// the player's effective attack range.
        /// </summary>
        /// <returns>The calculated range bonus, zero if no passive applies.</returns>
        public float GetPassiveRangeBonus()
        {
            return Passives.RangeBonus;
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
            var bonus = DerivedStatFormulas.MaxHealthBonus(baseMaxHealth, effectiveStats.GetValue(StatType.Vitality));
            return buffs != null ? bonus + buffs.MaxHealthBonus : bonus;
        }

        /// <inheritdoc />
        public int GetMaxManaBonus(int baseMaxMana)
        {
            EnsureInitialized();
            return DerivedStatFormulas.MaxManaBonus(baseMaxMana, effectiveStats.GetValue(StatType.Intelligence));
        }

        /// <summary>
        /// Gets the factor DEX and INT apply to a skill's variable cast time: (DEX x 2 + INT) / 530 of it is
        /// shaved off, down to instant.
        /// </summary>
        /// <returns>A multiplier between 0 (instant) and 1 (no reduction).</returns>
        public float GetCastTimeMultiplier()
        {
            EnsureInitialized();
            return DerivedStatFormulas.CastTimeMultiplier(effectiveStats.GetValue(StatType.Dexterity), effectiveStats.GetValue(StatType.Intelligence));
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
            return Passives.GetRaceDefenseBonus(attackerRace);
        }

        /// <inheritdoc />
        public float GetStatusResistChance()
        {
            EnsureInitialized();
            return DerivedStatFormulas.StatusResistChance(effectiveStats.GetValue(StatType.Vitality));
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
            return Passives.GetRaceDamageBonus(targetRace);
        }

        /// <summary>
        /// Gets the chance, from 0 to 1, that a basic attack with the equipped main-hand weapon deals double damage, from
        /// learned passives such as Double Attack. Zero if <see cref="passiveSkills"/> or <see cref="equipment"/> isn't wired.
        /// </summary>
        /// <returns>The double damage chance.</returns>
        public float GetDoubleAttackChance()
        {
            return Passives.DoubleAttackChance;
        }

        /// <inheritdoc />
        public void CaptureState(PlayerSaveData data)
        {
            EnsureInitialized();
            PlayerStatsSaveMapper.Capture(baseStats, data);
            data.currentHealth = health != null ? health.CurrentHealth : -1;
            data.currentMana = mana != null ? mana.CurrentMana : -1;
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
            PlayerStatsSaveMapper.Restore(baseStats, data);

            health?.ResetHealth();
            mana?.ResetMana();
            RefreshDependentMaxValues();
            NotifyStatsChanged();
            StartCoroutine(RestoreVitalsAfterLoad(data.currentHealth, data.currentMana));
        }

        /// <summary>
        /// Applies the saved current health and mana once every save participant has restored, so the maximums
        /// they feed (equipment, passives) are final before the values are clamped against them.
        /// </summary>
        /// <param name="savedHealth">The saved health, or a value below one to start at full health.</param>
        /// <param name="savedMana">The saved mana, or a negative value to start at full mana.</param>
        /// <returns>The coroutine enumerator.</returns>
        private System.Collections.IEnumerator RestoreVitalsAfterLoad(int savedHealth, int savedMana)
        {
            yield return null;

            if (health != null && savedHealth > 0)
            {
                health.SetCurrentHealth(savedHealth);
            }

            if (mana != null && savedMana >= 0)
            {
                mana.SetCurrentMana(savedMana);
            }
        }
    }
}
