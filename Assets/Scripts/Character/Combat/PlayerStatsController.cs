using System;
using UnityEngine;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using Project.Persistence;

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
    /// <see cref="CurrentSubStats"/> also multiplies the resulting Status
    /// ATK by <see cref="Project.Combat.BuffController.AttackMultiplier"/>
    /// (e.g. Berserk's persistent +32% ATK). When <see cref="classController"/>,
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

        [Tooltip("Optional. Source of flat Status ATK bonuses from learned passive skills (e.g. Sword Mastery), added on top of the stat-derived value in CurrentSubStats whenever the equipped weapon matches.")]
        [SerializeField] private PlayerPassiveSkillController passiveSkills;

        [Tooltip("Optional. Source of temporary/persistent attack buffs (e.g. Berserk), multiplied into Status ATK in CurrentSubStats. Left empty, the player is never affected by one.")]
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

                if (buffs != null)
                {
                    statusAtk = Mathf.RoundToInt(statusAtk * buffs.AttackMultiplier);
                }

                return statusAtk == subStats.StatusAtk
                    ? subStats
                    : new SubStats(statusAtk, subStats.StatusMatk, subStats.StatusDef, subStats.StatusMDef, subStats.Hit, subStats.Flee, subStats.CriticalRate, subStats.Aspd);
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
            effectiveStats = canApplyJobBonus
                ? new JobBonusStatsView(derivedStats, () => jobLevelBonusLookup.GetBonus(classController.CurrentClass, jobProgress.JobLevel))
                : derivedStats;

            if (equipment != null)
            {
                equipment.EquipmentChanged += HandleEquipmentChanged;
            }

            if (jobProgress != null)
            {
                jobProgress.JobLeveledUp += HandleJobLeveledUp;
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

        /// <inheritdoc />
        public int GetMaxHealthBonus(int baseMaxHealth)
        {
            EnsureInitialized();
            return Mathf.RoundToInt(baseMaxHealth * effectiveStats.GetValue(StatType.Vitality) * MaxHealthBonusPerVit);
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
            StatsChanged?.Invoke();
        }
    }
}
