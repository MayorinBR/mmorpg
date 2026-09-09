namespace Project.Character.Stats
{
    /// <summary>
    /// Holds the "Status" component of each sub-stat, derived purely from 
    /// the six base stats and character level. The equipment-driven component 
    /// is added later by the equipment/combat system and is intentionally 
    /// out of scope here.
    /// </summary>
    public readonly struct SubStats
    {
        /// <summary>
        /// Initializes a new set of status-derived sub-stats.
        /// </summary>
        public SubStats(int statusAtk, int statusMatk, int statusDef, int statusMDef, int hit, int flee, float criticalRate, int aspd)
        {
            StatusAtk = statusAtk;
            StatusMatk = statusMatk;
            StatusDef = statusDef;
            StatusMDef = statusMDef;
            Hit = hit;
            Flee = flee;
            CriticalRate = criticalRate;
            Aspd = aspd;
        }

        /// <summary>Gets the physical attack rating contributed by base stats alone.</summary>
        public int StatusAtk { get; }

        /// <summary>Gets the magic attack rating contributed by base stats alone.</summary>
        public int StatusMatk { get; }

        /// <summary>Gets the physical defense contributed by base stats alone.</summary>
        public int StatusDef { get; }

        /// <summary>Gets the magic defense contributed by base stats, VIT, DEX and level.</summary>
        public int StatusMDef { get; }

        /// <summary>Gets the accuracy rating used to resolve hit chance.</summary>
        public int Hit { get; }

        /// <summary>Gets the dodge rating contributed by AGI and level.</summary>
        public int Flee { get; }

        /// <summary>Gets the critical hit chance, expressed as a percentage.</summary>
        public float CriticalRate { get; }

        /// <summary>
        /// Gets the attack speed rating (higher attacks faster), on
        /// Ragnarok Online's classic 0-190 display scale — matching the
        /// original game's own status window, per Victor's request to keep
        /// every sub-stat's calculation and presentation consistent with
        /// it. Computed from the real, sourced iRO Wiki Classic AGI/DEX
        /// formula — see <see cref="SubStatsCalculator"/>'s remarks. One
        /// piece is still a placeholder: the real per-weapon, per-job
        /// weapon delay table isn't modeled yet, so every weapon and class
        /// currently share one flat value (see
        /// <see cref="SubStatsCalculator"/>'s remarks). Feeds
        /// directly into
        /// <see cref="Combat.AttackSpeedCalculator.GetAttackIntervalSeconds"/>
        /// (auto-attack cooldown), which
        /// <see cref="Character.Animation.PlayerAnimatorController.SetAttackDuration"/>
        /// also uses to keep the attack swing animation's playback speed in
        /// sync with it — skills are unaffected.
        /// </summary>
        public int Aspd { get; }
    }
}