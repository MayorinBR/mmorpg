using System.Collections.Generic;
using UnityEngine;
using Project.Skills;
using Project.Character.Animation;
using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Casts skills: checks the skill is known, off cooldown, and
    /// affordable, then damages the current enemy target (in range) —
    /// with a flat bonus against Demon/Undead targets from any learned
    /// "race bane" passive (e.g. Demon Bane) — heals the caster (or, cast
    /// on an Undead-race target, damages it instead, matching real
    /// Ragnarok Online's Heal), applies a timed buff/debuff (see
    /// <see cref="ownBuffs"/>) to the caster, the current enemy target, or
    /// every Demon/Undead target around the caster (e.g. Signum Crucis),
    /// spawns a persistent damaging zone (see <see cref="TryCastSkillAtPosition"/>)
    /// at a ground position the player picked, flips a status toggle (e.g.
    /// Hiding) on the caster, or reveals (and optionally damages) hidden
    /// targets around the caster (e.g. Sight, Ruwach), depending on the
    /// skill's effect and target type.
    /// </summary>
    public class PlayerSkillCaster : MonoBehaviour
    {
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerStatsController statsController;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private HealthComponent ownHealth;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private PlayerAnimatorController animatorController;

        [Tooltip("Optional. Source of active status effects (see StatusEffectController) affecting the caster — Stun/Freeze/Petrify block casting entirely, Silence blocks it alone. Left empty, the caster is never blocked by one.")]
        [SerializeField] private StatusEffectController statusEffects;

        [Tooltip("Optional. Source of the equipped main-hand weapon's subtype, used to scale a Physical damage skill's damage against the target's size (see WeaponSizeModifiers). Left empty, Physical skills deal full damage regardless of target size.")]
        [SerializeField] private EquipmentManager equipment;

        [Tooltip("The caster's own buff modifiers, for a Self-targeted Buff skill (e.g. Endure). Left empty, a Self-targeted Buff skill can't be cast.")]
        [SerializeField] private BuffController ownBuffs;

        [Tooltip("Layer containing enemy colliders, used by an area-of-effect skill (see SkillDefinition.IsAreaOfEffect, e.g. Magnum Break) to find every target within range of the caster, and by a Zone skill's spawned SkillZoneController (e.g. Fire Wall) to find who's standing inside it. Should be set to the same layer as SkillTargetingController's own enemyLayer.")]
        [SerializeField] private LayerMask enemyLayer;

        private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();

        /// <summary>
        /// Attempts to cast the given skill. For a Damage skill with no
        /// valid current target, this hands off to
        /// <see cref="SkillTargetingController"/> to let the player pick
        /// one (showing a targeting ring, per its own docs) instead of
        /// just failing — so this can return false either because the
        /// skill genuinely can't be cast (not learned, on cooldown) or
        /// because it just started waiting for a target pick.
        /// </summary>
        /// <param name="skill">The skill to cast.</param>
        /// <returns>True if the skill was successfully cast; false if any requirement wasn't met, or a target pick was started instead.</returns>
        public bool TryCastSkill(SkillDefinition skill)
        {
            if (ownHealth.IsDead || IsCastBlocked())
            {
                return false;
            }

            var level = skillBook.GetLevel(skill);

            if (level <= 0 || Time.time < GetCooldownEndTime(skill))
            {
                return false;
            }

            if (skill.EffectType == SkillEffectType.Passive)
            {
                // Passive skills apply their bonus automatically while
                // learned (see PlayerPassiveSkillController) and are
                // never actually cast.
                return false;
            }

            if (skill.TargetType == SkillTargetType.Ground)
            {
                // A Ground skill has no "already selected" position the
                // way an Enemy target can already be selected — every
                // cast needs a fresh pick. TryCastSkillAtPosition does the
                // actual casting once SkillTargetingController confirms one.
                SkillTargetingController.Instance?.BeginPicking(skill);
                return false;
            }

            if (NeedsEnemyTarget(skill) && !HasValidDamageTarget(skill))
            {
                SkillTargetingController.Instance?.BeginPicking(skill);
                return false;
            }

            var cast = skill.EffectType switch
            {
                SkillEffectType.Heal => TryCastHeal(skill),
                SkillEffectType.Buff => TryCastBuff(skill, level),
                SkillEffectType.Toggle => TryCastToggle(skill),
                SkillEffectType.Reveal => TryCastReveal(skill, level),
                _ => TryCastDamage(skill, level)
            };

            if (cast)
            {
                animatorController?.TriggerCast();
                cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
            }

            return cast;
        }

        /// <summary>
        /// Casts a <see cref="SkillTargetType.Ground"/> skill (e.g. Fire
        /// Wall) at a world position the player just picked via
        /// <see cref="SkillTargetingController"/>'s ground-picking mode.
        /// Mirrors <see cref="TryCastSkill"/>'s own learned/cooldown/dead
        /// checks, since a Ground skill's mana cost and cooldown are only
        /// spent once a position has actually been confirmed — picking is
        /// free to cancel.
        /// </summary>
        /// <param name="skill">The Ground-targeted skill being cast.</param>
        /// <param name="position">The world position the skill's zone should spawn at.</param>
        /// <returns>True if the zone was spawned.</returns>
        public bool TryCastSkillAtPosition(SkillDefinition skill, Vector3 position)
        {
            if (ownHealth.IsDead || IsCastBlocked())
            {
                return false;
            }

            var level = skillBook.GetLevel(skill);

            if (level <= 0 || Time.time < GetCooldownEndTime(skill) || !TryCastZone(skill, level, position))
            {
                return false;
            }

            animatorController?.TriggerCast();
            cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
            return true;
        }

        /// <summary>
        /// Checks whether the given skill could be cast right now, without
        /// spending any resources or triggering its cooldown. Intended for UI
        /// (hotbar icon state) rather than the actual cast flow.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>The skill's current availability.</returns>
        public SkillAvailability GetAvailability(SkillDefinition skill)
        {
            if (skill == null || skillBook.GetLevel(skill) <= 0)
            {
                return SkillAvailability.NotLearned;
            }

            if (Time.time < GetCooldownEndTime(skill))
            {
                return SkillAvailability.OnCooldown;
            }

            if (mana.CurrentMana < skill.ManaCost)
            {
                return SkillAvailability.InsufficientMana;
            }

            if (NeedsEnemyTarget(skill) && !HasValidDamageTarget(skill))
            {
                return SkillAvailability.NoValidTarget;
            }

            return SkillAvailability.Ready;
        }

        /// <summary>
        /// Checks whether a skill needs a valid current enemy target before
        /// it can be cast — every Damage skill, plus an Enemy-targeted Buff
        /// skill (e.g. Provoke). A Self-targeted Buff skill (e.g. Endure)
        /// and Heal need no such target.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill requires a targeted enemy in range.</returns>
        private static bool NeedsEnemyTarget(SkillDefinition skill)
        {
            return skill.EffectType == SkillEffectType.Damage
                || (skill.EffectType == SkillEffectType.Buff && skill.TargetType == SkillTargetType.Enemy);
        }

        /// <summary>
        /// Gets the remaining cooldown time for a skill, in seconds.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>Seconds remaining before the skill is off cooldown, or 0 if it's already ready.</returns>
        public float GetCooldownRemaining(SkillDefinition skill)
        {
            return Mathf.Max(0f, GetCooldownEndTime(skill) - Time.time);
        }

        /// <summary>
        /// Checks whether the current combat target is a usable target
        /// for this skill (selected, alive, and within range) — used for
        /// every Damage skill and for an Enemy-targeted Buff skill (see
        /// <see cref="NeedsEnemyTarget"/>), e.g. Provoke. Public so
        /// <see cref="SkillTargetingController"/> can re-check it right
        /// after the player confirms a picked target. A
        /// <see cref="SkillTargetType.AreaAroundCaster"/> skill (see
        /// <see cref="SkillDefinition.IsAreaOfEffect"/>) needs no
        /// pre-selected target at all, so this is always true for one — it
        /// can never fall into <see cref="SkillTargetingController"/>'s
        /// picking flow. A <see cref="SkillTargetType.AreaAroundTarget"/>
        /// skill still needs one in range, same as a single-target skill,
        /// since the burst is centered on it.
        /// </summary>
        /// <param name="skill">The skill to check range against.</param>
        /// <returns>True if the current target can be hit by this skill right now.</returns>
        public bool HasValidDamageTarget(SkillDefinition skill)
        {
            if (skill.IsAreaOfEffect && skill.TargetType == SkillTargetType.AreaAroundCaster)
            {
                return true;
            }

            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            return CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position) <= skill.Range;
        }

        /// <summary>
        /// Casts Heal: normally restores the caster's own health (Self and
        /// Ally both currently resolve to the caster — ally target
        /// selection isn't implemented yet), but real Ragnarok Online's
        /// Heal instead inflicts damage, equal to half the would-be heal
        /// amount, when cast on an Undead-race target — so if the current
        /// selected target is a living Undead in range, this damages it
        /// instead of healing the caster.
        /// </summary>
        /// <param name="skill">The Heal skill being cast.</param>
        /// <returns>True if the cast was committed (mana spent).</returns>
        private bool TryCastHeal(SkillDefinition skill)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var undeadTarget = GetUndeadTargetInRange(skill);

            if (undeadTarget != null)
            {
                // ponytail: ships as plain Neutral damage rather than Holy
                // (real RO's own element for this) — the same open
                // question item 9 of the shared systems plan already
                // tracks for Ruwach's damage, not resolved here either.
                var damage = Mathf.Max(1, skill.CalculateHeal() / 2);
                undeadTarget.TakeDamage(damage, Element.Neutral, DamageCategory.Magical, attacker: transform);
                return true;
            }

            ownHealth.Heal(skill.CalculateHeal());
            return true;
        }

        /// <summary>
        /// Gets the currently selected enemy target, if it's alive, within
        /// this skill's range, and Undead-race — otherwise null. Used by
        /// <see cref="TryCastHeal"/> to opportunistically damage an Undead
        /// target instead of self-healing, without requiring Heal to go
        /// through the usual enemy-target-picking flow (see <see cref="NeedsEnemyTarget"/>,
        /// which deliberately excludes Heal).
        /// </summary>
        /// <param name="skill">The Heal skill being cast.</param>
        private IDamageable GetUndeadTargetInRange(SkillDefinition skill)
        {
            var target = targetSelector.CurrentDamageable;

            if (target == null || target.IsDead || target.Race != MonsterRace.Undead)
            {
                return null;
            }

            return CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position) <= skill.Range ? target : null;
        }

        /// <summary>
        /// Casts a buff/debuff skill (e.g. Provoke, Endure): applies a
        /// timed <see cref="BuffPayload"/> via <see cref="BuffController"/>
        /// to the caster itself (<see cref="SkillTargetType.Self"/>, using
        /// <see cref="ownBuffs"/>) or to the current enemy target
        /// (<see cref="SkillTargetType.Enemy"/>), found through a
        /// <see cref="BuffController"/> on the same
        /// <see cref="IDamageable"/> hierarchy <see cref="targetSelector"/>
        /// already resolved — the same lookup <see cref="HealthComponent"/>
        /// itself reads from. Spends mana as soon as the cast is committed,
        /// the same as a Damage skill; fails without spending anything if
        /// the resolved target has no <see cref="BuffController"/> wired.
        /// </summary>
        /// <param name="skill">The buff/debuff skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True if the buff was applied.</returns>
        private bool TryCastBuff(SkillDefinition skill, int level)
        {
            if (skill.TargetType == SkillTargetType.AreaAroundCaster)
            {
                return TryCastAreaDebuff(skill, level);
            }

            // Ally targeting isn't implemented yet (see SkillTargetType's
            // own doc comment) — an Ally-targeted buff (e.g. Blessing)
            // resolves to the caster itself, same as Self, until real ally
            // selection exists.
            var isSelfOrAlly = skill.TargetType == SkillTargetType.Self || skill.TargetType == SkillTargetType.Ally;
            var buffs = isSelfOrAlly
                ? ownBuffs
                : targetSelector.CurrentTarget?.GetComponentInParent<BuffController>();

            if (buffs == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            if (!isSelfOrAlly)
            {
                // An enemy-targeted Buff is really a debuff (e.g. Decrease
                // AGI) and can be resisted — GetSuccessChance is zero for
                // every skill authored before this field existed (Provoke,
                // Endure), so they keep always landing unchanged. Mana and
                // cooldown are still spent on a resist, same as a missed
                // Damage skill.
                var successChance = skill.GetSuccessChance(level);

                if (successChance > 0f && Random.value >= successChance)
                {
                    return true;
                }
            }

            buffs.ApplyBuff(BuildBuffPayload(skill, level), skill.GetBuffDuration(level));
            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillTargetType.AreaAroundCaster"/> debuff
        /// skill (e.g. Signum Crucis): spends mana as soon as the cast is
        /// committed, then rolls <see cref="SkillDefinition.GetSuccessChance"/>
        /// separately against every distinct, living Demon/Undead
        /// <see cref="IDamageable"/> within <see cref="SkillDefinition.AreaRadius"/>
        /// of the caster, applying the debuff's <see cref="BuffPayload"/>
        /// to each one that fails to resist. ponytail: hardcoded to
        /// Demon/Undead-only since Signum Crucis is the only area debuff
        /// today; generalize with a per-skill race filter field only if a
        /// second one needs a different restriction.
        /// </summary>
        /// <param name="skill">The area debuff skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True once the cast is committed (mana spent), regardless of how many targets were affected.</returns>
        private bool TryCastAreaDebuff(SkillDefinition skill, int level)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var payload = BuildBuffPayload(skill, level);
            var duration = skill.GetBuffDuration(level);
            var successChance = skill.GetSuccessChance(level);
            var hitColliders = Physics.OverlapSphere(transform.position, skill.AreaRadius, enemyLayer);
            var alreadyHit = new HashSet<BuffController>();

            foreach (var hitCollider in hitColliders)
            {
                var target = hitCollider.GetComponentInParent<IDamageable>();

                if (target == null || target.IsDead || (target.Race != MonsterRace.Demon && target.Race != MonsterRace.Undead))
                {
                    continue;
                }

                var targetBuffs = hitCollider.GetComponentInParent<BuffController>();

                if (targetBuffs == null || !alreadyHit.Add(targetBuffs))
                {
                    continue;
                }

                if (successChance > 0f && Random.value >= successChance)
                {
                    continue;
                }

                targetBuffs.ApplyBuff(payload, duration);
            }

            return true;
        }

        /// <summary>
        /// Builds the <see cref="BuffPayload"/> a Buff skill grants at the
        /// given level, from every channel <see cref="SkillDefinition"/>
        /// supports. Shared by <see cref="TryCastBuff"/> and
        /// <see cref="TryCastAreaDebuff"/> so the two don't duplicate the
        /// same field-by-field construction.
        /// </summary>
        /// <param name="skill">The buff/debuff skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        private static BuffPayload BuildBuffPayload(SkillDefinition skill, int level)
        {
            var statBonus = skill.GetBuffStatBonus(level);
            return new BuffPayload(
                skill.GetBuffAtkPercent(level),
                skill.GetBuffDefPercent(level),
                skill.GetBuffMdefBonus(level),
                skill.GetBuffAspdPercent(level),
                skill.GetBuffMaxHealthBonus(level),
                statBonus.Strength,
                statBonus.Agility,
                statBonus.Vitality,
                statBonus.Intelligence,
                statBonus.Dexterity,
                statBonus.Luck);
        }

        /// <summary>
        /// Spends mana and puts the skill on cooldown as soon as the cast is
        /// committed (in range, affordable), regardless of whether the hit
        /// actually lands — matching Ragnarok Online, where a missed skill
        /// still consumes its resources. Delegates to
        /// <see cref="TryCastAreaDamage"/> for a
        /// <see cref="SkillTargetType.AreaAroundCaster"/> skill (see
        /// <see cref="SkillDefinition.IsAreaOfEffect"/>), since that needs
        /// no pre-selected target at all. A
        /// <see cref="SkillTargetType.AreaAroundTarget"/> skill still goes
        /// through the same range/mana gate as a single-target skill, but
        /// bursts via <see cref="ApplyAreaDamage"/> centered on that target
        /// instead of hitting only it. Otherwise the hit is resolved
        /// against the target's Flee via <see cref="HitChanceCalculator"/>,
        /// applying damage as <see cref="DamageCategory.Physical"/> or
        /// <see cref="DamageCategory.Magical"/> depending on the skill's
        /// <see cref="SkillDefinition.DamageType"/> — a Physical skill is
        /// also scaled by <see cref="WeaponSizeModifiers"/> for the
        /// equipped weapon against the target's size, the same as a basic
        /// attack.
        /// </summary>
        private bool TryCastDamage(SkillDefinition skill, int level)
        {
            if (skill.IsAreaOfEffect && skill.TargetType == SkillTargetType.AreaAroundCaster)
            {
                return TryCastAreaDamage(skill, level);
            }

            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return false;
            }

            var distance = CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position);

            if (distance > skill.Range || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            if (skill.IsAreaOfEffect)
            {
                ApplyAreaDamage(skill, level, targetSelector.CurrentTarget.position);
                return true;
            }

            var target = targetSelector.CurrentDamageable;
            var subStats = statsController.CurrentSubStats;

            if (HitChanceCalculator.RollHit(subStats.Hit + skill.GetAccuracyBonus(level), target.FleeRating))
            {
                var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
                var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
                damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                damage += GetRaceDamageBonus(category, target.Race);
                target.TakeDamage(damage, skill.Element, category, attacker: transform);
                var targetStatus = targetSelector.CurrentTarget.GetComponentInParent<StatusEffectController>();
                TryProcStunAugment(skill, level, targetStatus);
                TryProcInflictedStatus(skill, level, targetStatus);
            }
            else
            {
                target.NotifyDodged();
            }

            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillTargetType.AreaAroundCaster"/> damage
        /// skill (e.g. Magnum Break): spends mana as soon as the cast is
        /// committed, the same as <see cref="TryCastDamage"/>, then bursts
        /// via <see cref="ApplyAreaDamage"/> centered on the caster's own
        /// position — unlike a single-target skill, this needs no
        /// pre-selected target and can't fail for lack of one, matching how
        /// the real Magnum Break always fires (and consumes its cost)
        /// whether or not anything was actually standing in range.
        /// </summary>
        /// <param name="skill">The area-of-effect skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True once the cast is committed (mana spent), regardless of how many targets were hit.</returns>
        private bool TryCastAreaDamage(SkillDefinition skill, int level)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            ApplyAreaDamage(skill, level, transform.position);
            return true;
        }

        /// <summary>
        /// Rolls a separate hit check against every distinct, living
        /// <see cref="IDamageable"/> found within
        /// <see cref="SkillDefinition.AreaRadius"/> of <paramref name="center"/>
        /// via <see cref="enemyLayer"/>. Shared by <see cref="TryCastAreaDamage"/>
        /// (centered on the caster) and <see cref="TryCastDamage"/>'s
        /// <see cref="SkillTargetType.AreaAroundTarget"/> branch (centered
        /// on the resolved target) — mana and range/target validity are
        /// already handled by whichever of those called this.
        /// </summary>
        /// <param name="skill">The area-of-effect skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="center">The world position the burst is centered on.</param>
        private void ApplyAreaDamage(SkillDefinition skill, int level, Vector3 center)
        {
            var subStats = statsController.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            var hitColliders = Physics.OverlapSphere(center, skill.AreaRadius, enemyLayer);
            var alreadyHit = new HashSet<IDamageable>();

            foreach (var hitCollider in hitColliders)
            {
                var target = hitCollider.GetComponentInParent<IDamageable>();

                if (target == null || target.IsDead || !alreadyHit.Add(target))
                {
                    continue;
                }

                if (HitChanceCalculator.RollHit(accuracy, target.FleeRating))
                {
                    var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
                    damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                    damage += GetRaceDamageBonus(category, target.Race);
                    target.TakeDamage(damage, skill.Element, category, attacker: transform);
                    var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();
                    TryProcStunAugment(skill, level, targetStatus);
                    TryProcInflictedStatus(skill, level, targetStatus);
                }
                else
                {
                    target.NotifyDodged();
                }
            }
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Zone"/> skill (e.g. Fire
        /// Wall): spends mana as soon as the cast is committed, the same as
        /// a Damage skill, then spawns a <see cref="SkillZoneController"/>
        /// at <paramref name="position"/> that deals this skill's damage,
        /// on its own tick interval, to every living enemy standing inside
        /// it until its duration runs out. Damage and accuracy are fixed at
        /// cast time from the caster's current stats, the same as every
        /// other skill here — the zone itself never re-reads them.
        /// </summary>
        /// <param name="skill">The zone skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="position">The world position the zone spawns at.</param>
        /// <returns>True if the zone was spawned.</returns>
        private bool TryCastZone(SkillDefinition skill, int level, Vector3 position)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var subStats = statsController.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;

            var zone = SkillZoneController.Spawn(skill.ZonePrefab, position, skill.AreaRadius, skill.GetZoneDuration(level), skill.ZoneTickIntervalSeconds);
            zone.Initialize(enemyLayer, accuracy, damage, skill.Element, category, transform);
            return true;
        }

        /// <summary>
        /// Rolls every learned passive that augments <paramref name="castSkill"/>
        /// with a stun chance (see <see cref="SkillDefinition.AugmentsSkill"/>,
        /// e.g. Fatal Blow augmenting Bash) and applies a stun to
        /// <paramref name="targetStatus"/> on a successful proc. No-ops
        /// safely if the target has no <see cref="StatusEffectController"/> wired.
        /// </summary>
        /// <param name="castSkill">The skill that was just cast and landed a hit.</param>
        /// <param name="castSkillLevel">The cast skill's current level, used to scale the augmenting passive's stun chance.</param>
        /// <param name="targetStatus">The hit target's status effects, or null if it has none wired.</param>
        private void TryProcStunAugment(SkillDefinition castSkill, int castSkillLevel, StatusEffectController targetStatus)
        {
            if (targetStatus == null)
            {
                return;
            }

            foreach (var entry in skillBook.LearnedSkills)
            {
                var passive = entry.Key;

                if (passive.EffectType != SkillEffectType.Passive || entry.Value <= 0 || passive.AugmentsSkill != castSkill)
                {
                    continue;
                }

                if (Random.value < passive.GetStunChance(castSkillLevel))
                {
                    targetStatus.ApplyStun(passive.StunDurationSeconds);
                }
            }
        }

        /// <summary>
        /// Rolls the cast skill's own <see cref="SkillDefinition.InflictedStatus"/>
        /// (see <see cref="SkillDefinition.GetSuccessChance"/>, e.g. Envenom's
        /// poison chance) and applies it to <paramref name="targetStatus"/>
        /// on a successful proc. No-ops safely if the skill inflicts nothing
        /// or the target has no <see cref="StatusEffectController"/> wired.
        /// </summary>
        /// <param name="skill">The skill that was just cast and landed a hit.</param>
        /// <param name="level">The cast skill's current level.</param>
        /// <param name="targetStatus">The hit target's status effects, or null if it has none wired.</param>
        private void TryProcInflictedStatus(SkillDefinition skill, int level, StatusEffectController targetStatus)
        {
            if (targetStatus == null || skill.InflictedStatus == StatusEffectType.None)
            {
                return;
            }

            if (Random.value < skill.GetSuccessChance(level))
            {
                targetStatus.Apply(skill.InflictedStatus, skill.GetInflictedStatusDuration(level), skill.GetPoisonDamagePerTick(level));
            }
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Toggle"/> skill: spends mana
        /// and flips the caster's own Hidden status via
        /// <see cref="statusEffects"/> — recasting the same skill turns it
        /// back off, matching how Ragnarok Online's Hiding works.
        /// ponytail: hardcoded to Hiding since it's the only Toggle skill
        /// today; generalize (e.g. a per-skill "which flag" field) only if
        /// a second toggle status shows up.
        /// </summary>
        /// <param name="skill">The toggle skill being cast.</param>
        /// <returns>True if the toggle was applied.</returns>
        private bool TryCastToggle(SkillDefinition skill)
        {
            if (statusEffects == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            statusEffects.SetHidden(!statusEffects.IsHidden);
            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Reveal"/> skill (e.g. Sight,
        /// Ruwach): spends mana, then calls <see cref="StatusEffectController.Reveal"/>
        /// on every hidden target within <see cref="SkillDefinition.AreaRadius"/>
        /// of the caster's own position — no target selection needed,
        /// same as <see cref="TryCastAreaDamage"/>. If the skill also has a
        /// non-zero damage roll (Ruwach; Sight's is zero), each revealed
        /// target additionally takes damage via the same hit-roll/size-scaling
        /// path as any other Damage skill, but only if it was actually hidden —
        /// this never hits an already-visible enemy.
        /// </summary>
        /// <param name="skill">The reveal skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True once the cast is committed (mana spent), regardless of how many hidden targets were found.</returns>
        private bool TryCastReveal(SkillDefinition skill, int level)
        {
            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var subStats = statsController.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            var hitColliders = Physics.OverlapSphere(transform.position, skill.AreaRadius, enemyLayer);
            var alreadyRevealed = new HashSet<StatusEffectController>();

            foreach (var hitCollider in hitColliders)
            {
                var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();

                if (targetStatus == null || !targetStatus.IsHidden || !alreadyRevealed.Add(targetStatus))
                {
                    continue;
                }

                targetStatus.Reveal();

                var target = hitCollider.GetComponentInParent<IDamageable>();
                var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);

                if (damage <= 0 || target == null || target.IsDead || !HitChanceCalculator.RollHit(accuracy, target.FleeRating))
                {
                    continue;
                }

                damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                target.TakeDamage(damage, skill.Element, category, attacker: transform);
            }

            return true;
        }

        private float GetCooldownEndTime(SkillDefinition skill)
        {
            return cooldownEndTimes.TryGetValue(skill, out var endTime) ? endTime : 0f;
        }

        /// <summary>
        /// Gets whether an active status effect (see <see cref="statusEffects"/>)
        /// is currently blocking the caster from casting anything — Stun,
        /// Freeze and Petrify block every action, Silence blocks casting alone.
        /// </summary>
        private bool IsCastBlocked()
        {
            return statusEffects != null && (statusEffects.IsImmobilized || statusEffects.IsSilenced);
        }

        /// <summary>
        /// Gets the equipped main-hand weapon's subtype, or
        /// <see cref="WeaponSubtype.Unarmed"/> if <see cref="equipment"/>
        /// isn't wired — <see cref="WeaponSizeModifiers"/> already treats
        /// Unarmed as no size penalty, so this degrades safely.
        /// </summary>
        private WeaponSubtype GetMainHandWeaponSubtype()
        {
            return equipment != null ? equipment.GetMainHandWeaponSubtype() : WeaponSubtype.Unarmed;
        }

        /// <summary>
        /// Gets the flat physical damage bonus from a learned "race bane"
        /// passive (e.g. Demon Bane) against the given target's race — zero
        /// for a Magical skill, matching real Ragnarok Online, where this
        /// kind of bonus only ever applies to physical damage.
        /// </summary>
        /// <param name="category">The skill's damage category.</param>
        /// <param name="targetRace">The race of the target being hit.</param>
        private int GetRaceDamageBonus(DamageCategory category, MonsterRace targetRace)
        {
            return category == DamageCategory.Physical ? statsController.GetRaceDamageBonus(targetRace) : 0;
        }
    }
}