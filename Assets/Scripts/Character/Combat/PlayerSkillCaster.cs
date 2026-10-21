using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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
    /// spawns a persistent damaging or attack-blocking zone (e.g. Fire
    /// Wall, Safety Wall — see <see cref="TryCastSkillAtPosition"/>) at a
    /// ground position the player picked, flips a status toggle (e.g.
    /// Hiding) on the caster, flips an ongoing SP-draining damage-reduction
    /// buff (e.g. Energy Coat, see <see cref="PlayerEnergyCoatController"/>),
    /// reveals (and optionally damages) hidden targets around the caster
    /// (e.g. Sight, Ruwach), clears every active debuff on the caster (e.g.
    /// Cure, Detoxify), instantly pushes the caster back (e.g. Back Slide),
    /// or crafts a recipe's result item from materials in the caster's own
    /// inventory (e.g. Arrow Crafting, Aqua Benedicta), depending on the
    /// skill's effect and target type. A Damage skill can also carry its
    /// own knockback, pushing the hit target back instead (e.g. Arrow Repel).
    /// </summary>
    public class PlayerSkillCaster : MonoBehaviour
    {
        private const float NearWaterRadius = 3f;
        private const float CastMoveTolerance = 0.5f;

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

        [Tooltip("Optional. The caster's own NavMeshAgent, used by a Displacement skill (e.g. Back Slide) to push the caster back via KnockbackUtility. Left empty, a Displacement skill can't be cast.")]
        [SerializeField] private NavMeshAgent agent;

        [Tooltip("Optional. Source of the player's Zeny, spent by a Damage skill with a nonzero SkillDefinition.ZenyCost (e.g. Mammonite), on top of its mana cost. Left empty, such a skill can't be cast; every skill with no Zeny cost is unaffected.")]
        [SerializeField] private PlayerCurrency currency;

        [Tooltip("Optional. Drives an ongoing SP-draining damage-reduction toggle (e.g. Energy Coat). Left empty, a ToggleDrain skill can't be cast.")]
        [SerializeField] private PlayerEnergyCoatController energyCoat;

        [Tooltip("Optional. The caster's own inventory, crafted into by a Craft skill (e.g. Arrow Crafting, Aqua Benedicta). Left empty, a Craft skill can't be cast.")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Source of the arrow/bolt visual a ranged skill fires. Left empty, it is looked up on this object's parents and children at first use.")]
        [SerializeField] private PlayerCombatController combat;

        [Tooltip("Layer containing Water colliders, checked by a Craft skill with SkillDefinition.RequiresNearWater (e.g. Aqua Benedicta). Left at its default (nothing), such a skill can never find water — ready for the moment a map actually tags one.")]
        [SerializeField] private LayerMask waterLayer;

        private readonly Dictionary<SkillDefinition, float> cooldownEndTimes = new Dictionary<SkillDefinition, float>();
        private Coroutine castRoutine;

        /// <summary>Gets whether a skill with a cast time is currently being cast; no other skill can start meanwhile.</summary>
        public bool IsCasting => castRoutine != null;

        /// <summary>Raised when a timed cast begins, with its total duration in seconds.</summary>
        public event System.Action<float> CastStarted;

        /// <summary>Raised when a timed cast ends, whether it completed or was interrupted.</summary>
        public event System.Action CastEnded;

        private void OnDisable()
        {
            if (castRoutine != null)
            {
                castRoutine = null;
                CastEnded?.Invoke();
            }
        }

        /// <summary>
        /// Gets the player's current level in the given skill.
        /// </summary>
        /// <param name="skill">The skill to look up.</param>
        /// <returns>The learned level, zero if not learned.</returns>
        public int GetSkillLevel(SkillDefinition skill)
        {
            return skillBook.GetLevel(skill);
        }

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
            if (ownHealth.IsDead || IsCastBlocked() || IsCasting)
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

            if (!MeetsWeaponRequirement(skill) || !MeetsCatalystRequirement(skill))
            {
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

            var castTime = GetCastTime(skill, level);

            if (castTime > 0f)
            {
                if (mana.CurrentMana < skill.ManaCost)
                {
                    return false;
                }

                animatorController?.TriggerCast();
                BeginCast(castTime, () => MeetsWeaponRequirement(skill) && MeetsCatalystRequirement(skill) && ExecuteCast(skill, level, false));
                return true;
            }

            return ExecuteCast(skill, level, true);
        }

        /// <summary>
        /// Resolves a skill's effect, then spends its ammo and catalyst and starts its cooldown when it took
        /// effect.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="triggerAnimation">Whether to play the cast animation now; false when it already played at the start of a timed cast.</param>
        /// <returns>True if the skill took effect.</returns>
        private bool ExecuteCast(SkillDefinition skill, int level, bool triggerAnimation)
        {
            var cast = skill.EffectType switch
            {
                SkillEffectType.Heal => TryCastHeal(skill),
                SkillEffectType.Buff => TryCastBuff(skill, level),
                SkillEffectType.Toggle => TryCastToggle(skill),
                SkillEffectType.ToggleDrain => TryCastToggleDrain(skill, level),
                SkillEffectType.Reveal => TryCastReveal(skill, level),
                SkillEffectType.Cleanse => TryCastCleanse(skill),
                SkillEffectType.Displacement => TryCastDisplacement(skill),
                SkillEffectType.Craft => TryCastCraft(skill),
                _ => TryCastDamage(skill, level)
            };

            if (cast)
            {
                SpendAmmo(skill);
                SpendCatalyst(skill);

                if (triggerAnimation)
                {
                    animatorController?.TriggerCast();
                }

                cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
            }

            return cast;
        }

        /// <summary>
        /// Gets how long the skill takes to cast: its fixed part plus its variable part scaled by the caster's
        /// DEX and INT.
        /// </summary>
        private float GetCastTime(SkillDefinition skill, int level)
        {
            return skill.GetFixedCastTime(level) + skill.GetVariableCastTime(level) * statsController.GetCastTimeMultiplier();
        }

        private void BeginCast(float castTime, System.Func<bool> onComplete)
        {
            castRoutine = StartCoroutine(CastRoutine(castTime, onComplete));
            CastStarted?.Invoke(castTime);
        }

        /// <summary>
        /// Waits out a cast, abandoning it if the caster dies, gets blocked (stun, flinch, silence) or walks
        /// away, then runs <paramref name="onComplete"/>.
        /// </summary>
        private IEnumerator CastRoutine(float castTime, System.Func<bool> onComplete)
        {
            var origin = transform.position;
            var endTime = Time.time + castTime;

            while (Time.time < endTime)
            {
                if (ownHealth.IsDead || IsCastBlocked() || (transform.position - origin).sqrMagnitude > CastMoveTolerance * CastMoveTolerance)
                {
                    castRoutine = null;
                    CastEnded?.Invoke();
                    PlayerFeedbackChannel.Publish("Cast interrupted.");
                    yield break;
                }

                yield return null;
            }

            castRoutine = null;
            CastEnded?.Invoke();
            onComplete();
        }

        /// <summary>
        /// Checks the weapon and ammo a skill needs before any resource is spent, telling the player why a cast
        /// was refused (e.g. Double Strafe without a bow or without arrows).
        /// </summary>
        /// <param name="skill">The skill about to be cast.</param>
        /// <returns>True when the skill needs nothing the caster lacks.</returns>
        private bool MeetsWeaponRequirement(SkillDefinition skill)
        {
            if (!skill.IsWeaponRequirementMet(GetMainHandWeaponSubtype()))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} requires a {string.Join(" or ", skill.RequiredWeaponSubtypes)}.");
                return false;
            }

            if (skill.AmmoCost > 0 && (equipment == null || equipment.EquippedAmmoCount < skill.AmmoCost))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} needs ammo equipped.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that the inventory holds the item the skill consumes (e.g. a gemstone), telling the player
        /// what is missing when it does not.
        /// </summary>
        /// <param name="skill">The skill about to be cast.</param>
        /// <returns>True when the skill needs no catalyst or the inventory holds enough of it.</returns>
        private bool MeetsCatalystRequirement(SkillDefinition skill)
        {
            if (skill.RequiredItem == null)
            {
                return true;
            }

            if (inventory == null || inventory.Items.GetQuantity(skill.RequiredItem) < skill.RequiredItemCount)
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} requires {skill.RequiredItemCount} {skill.RequiredItem.ItemName}.");
                return false;
            }

            return true;
        }

        private void SpendCatalyst(SkillDefinition skill)
        {
            if (skill.RequiredItem != null)
            {
                inventory.Items.TryRemoveItem(skill.RequiredItem, skill.RequiredItemCount);
            }
        }

        private void SpendAmmo(SkillDefinition skill)
        {
            for (var i = 0; i < skill.AmmoCost; i++)
            {
                equipment.TryConsumeAmmo();
            }
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
            if (ownHealth.IsDead || IsCastBlocked() || IsCasting)
            {
                return false;
            }

            var level = skillBook.GetLevel(skill);

            if (level <= 0 || Time.time < GetCooldownEndTime(skill) || !MeetsCatalystRequirement(skill))
            {
                return false;
            }

            var castTime = GetCastTime(skill, level);

            if (castTime > 0f)
            {
                if (mana.CurrentMana < skill.ManaCost)
                {
                    return false;
                }

                animatorController?.TriggerCast();
                BeginCast(castTime, () => ExecuteZoneCast(skill, level, position, false));
                return true;
            }

            return ExecuteZoneCast(skill, level, position, true);
        }

        private bool ExecuteZoneCast(SkillDefinition skill, int level, Vector3 position, bool triggerAnimation)
        {
            if (!MeetsCatalystRequirement(skill) || !TryCastZone(skill, level, position))
            {
                return false;
            }

            SpendCatalyst(skill);

            if (triggerAnimation)
            {
                animatorController?.TriggerCast();
            }

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

            return CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position) <= GetEffectiveRange(skill);
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
                var damage = Mathf.Max(1, skill.CalculateHeal() / 2);
                undeadTarget.TakeDamage(damage, Element.Holy, DamageCategory.Magical, attacker: transform);
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
                if (skill.ImmuneToUndeadAndBoss && IsUndeadOrBoss(targetSelector.CurrentTarget))
                {
                    return true;
                }

                var successChance = skill.GetSuccessChance(level);

                if (successChance > 0f && Random.value >= successChance)
                {
                    return true;
                }
            }

            buffs.ApplyBuff(BuildBuffPayload(skill, level), skill.GetBuffDuration(level));

            if (isSelfOrAlly && skill.BuffRevealsHidden)
            {
                RevealHiddenAround(skill.AreaRadius);
            }

            return true;
        }

        /// <summary>
        /// Reveals every hidden enemy within the given radius of the caster, without damaging it.
        /// </summary>
        /// <param name="radius">Radius, in meters, around the caster.</param>
        private void RevealHiddenAround(float radius)
        {
            foreach (var hitCollider in Physics.OverlapSphere(transform.position, radius, enemyLayer))
            {
                var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();

                if (targetStatus != null && targetStatus.IsHidden)
                {
                    targetStatus.Reveal();
                }
            }
        }

        /// <summary>
        /// Gets how far a target-picked skill reaches: its authored range plus the Archer's learned range
        /// passive (Vulture's Eye) for skills that fire ammo.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <returns>The effective range, in meters.</returns>
        private float GetEffectiveRange(SkillDefinition skill)
        {
            return skill.Range + (skill.AmmoCost > 0 ? statsController.GetPassiveRangeBonus() : 0f);
        }

        /// <summary>
        /// Fires the arrow (ammo skills) or bolt (magical skills) visual at the target through the basic
        /// attack's projectile setup. The skill's outcome is already decided, so this is purely cosmetic.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="target">The target the projectile flies to.</param>
        private void PlayProjectileVisual(SkillDefinition skill, Transform target)
        {
            var isMagical = skill.DamageType == SkillDamageType.Magical;

            if (skill.AmmoCost <= 0 && !isMagical)
            {
                return;
            }

            if (combat == null)
            {
                combat = GetComponentInParent<PlayerCombatController>() ?? GetComponentInChildren<PlayerCombatController>();
            }

            combat?.SpawnSkillProjectile(isMagical, target);
        }

        /// <summary>
        /// Gets whether the given target is an Undead-race monster or a Boss.
        /// </summary>
        /// <param name="target">The target transform to inspect.</param>
        /// <returns>True if the target is Undead or a Boss.</returns>
        private static bool IsUndeadOrBoss(Transform target)
        {
            var stats = target != null ? target.GetComponentInParent<CharacterStatsHolder>()?.Stats : null;
            return stats != null && (stats.IsBoss || stats.Race == MonsterRace.Undead);
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
                statBonus.Luck,
                preventsFlinch: skill.BuffPreventsFlinch);
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

            if (skill.ZenyCost > 0 && (currency == null || currency.CurrentZeny < skill.ZenyCost))
            {
                // Checked (not spent) before ManaCost below, so a skill
                // that can't be afforded never partially spends mana.
                return false;
            }

            var distance = CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position);

            if (distance > GetEffectiveRange(skill) || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            if (skill.ZenyCost > 0)
            {
                // Guaranteed to succeed: affordability was already checked
                // above and nothing else can spend Zeny mid-cast.
                currency.TrySpend(skill.ZenyCost);
            }

            PlayProjectileVisual(skill, targetSelector.CurrentTarget);

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
                damage = Mathf.RoundToInt(damage * skill.GetRaceDamageMultiplier(target.Race));
                var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
                damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                damage += GetRaceDamageBonus(category, target.Race);
                target.TakeDamage(damage, skill.Element, category, attacker: transform);
                ElementalBonusDamage.Apply(target, damage, statsController.FireDamagePercent, Element.Fire, transform);
                var targetStatus = targetSelector.CurrentTarget.GetComponentInParent<StatusEffectController>();
                TryProcStunAugment(skill, level, targetStatus);
                TryProcInflictedStatus(skill, level, targetStatus);

                if (skill.KnockbackDistance > 0f)
                {
                    var targetAgent = targetSelector.CurrentTarget.GetComponentInParent<NavMeshAgent>();
                    KnockbackUtility.Apply(targetAgent, transform.position, skill.KnockbackDistance);
                }
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
            var healthCost = skill.GetHealthCost(level);

            if (healthCost > 0 && ownHealth != null && ownHealth.CurrentHealth <= healthCost)
            {
                return false;
            }

            if (!mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            if (healthCost > 0 && ownHealth != null)
            {
                ownHealth.TrySpendHealth(healthCost);
            }

            ApplyAreaDamage(skill, level, transform.position);
            ApplyFireDamageBuff(skill, level);
            return true;
        }

        /// <summary>
        /// Gives the caster the skill's after-effect of extra Fire damage on every attack (e.g. Magnum Break),
        /// lasting <see cref="SkillDefinition.GetBuffDuration"/> seconds. No-ops for a skill without one.
        /// </summary>
        private void ApplyFireDamageBuff(SkillDefinition skill, int level)
        {
            if (skill.SelfFireDamageBonusPercent <= 0f || ownBuffs == null)
            {
                return;
            }

            ownBuffs.ApplyBuff(new BuffPayload(fireDamagePercent: skill.SelfFireDamageBonusPercent), skill.GetBuffDuration(level));
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
                    damage = Mathf.RoundToInt(damage * skill.GetRaceDamageMultiplier(target.Race));
                    damage = WeaponSizeModifiers.Apply(damage, category, GetMainHandWeaponSubtype(), target.Size);
                    damage += GetRaceDamageBonus(category, target.Race);
                    target.TakeDamage(damage, skill.Element, category, attacker: transform);
                    ElementalBonusDamage.Apply(target, damage, statsController.FireDamagePercent, Element.Fire, transform);
                    var targetStatus = hitCollider.GetComponentInParent<StatusEffectController>();
                    TryProcStunAugment(skill, level, targetStatus);
                    TryProcInflictedStatus(skill, level, targetStatus);

                    if (skill.KnockbackDistance > 0f)
                    {
                        KnockbackUtility.Apply(hitCollider.GetComponentInParent<NavMeshAgent>(), transform.position, skill.KnockbackDistance);
                    }
                }
                else
                {
                    target.NotifyDodged();
                }
            }
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Zone"/> skill: spends mana as
        /// soon as the cast is committed, the same as a Damage skill, then
        /// spawns a <see cref="SkillZoneController"/> at
        /// <paramref name="position"/>. If <see cref="SkillDefinition.BlocksAttacks"/>
        /// is true (e.g. Safety Wall, Pneuma), the zone blocks attacks of
        /// its <see cref="SkillDefinition.BlockedWeaponType"/> within
        /// range instead of damaging (see
        /// <see cref="SkillZoneController.InitializeBlock"/>); otherwise it
        /// deals this skill's damage, on its own tick interval, to every
        /// living enemy standing inside it until its duration runs out
        /// (e.g. Fire Wall). Damage and accuracy are fixed at cast time
        /// from the caster's current stats, the same as every other skill
        /// here — the zone itself never re-reads them.
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

            var zone = SkillZoneController.Spawn(skill.ZonePrefab, position, skill.AreaRadius, skill.GetZoneDuration(level), skill.ZoneTickIntervalSeconds);

            if (skill.BlocksAttacks)
            {
                zone.InitializeBlock(skill.BlockedWeaponType == WeaponType.Melee ? AttackRangeKind.Melee : AttackRangeKind.Ranged);
                return true;
            }

            var subStats = statsController.CurrentSubStats;
            var accuracy = subStats.Hit + skill.GetAccuracyBonus(level);
            var damage = skill.CalculateDamage(subStats.StatusAtk, subStats.StatusMatk, level);
            var category = skill.DamageType == SkillDamageType.Physical ? DamageCategory.Physical : DamageCategory.Magical;
            zone.Initialize(enemyLayer, accuracy, damage, skill.Element, category, transform, skill.GetZoneMaxHits(level));
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
        /// Casts a <see cref="SkillEffectType.ToggleDrain"/> skill (e.g.
        /// Energy Coat): spends this skill's <see cref="SkillDefinition.ManaCost"/>
        /// (paid on/off, mirroring <see cref="TryCastToggle"/>'s own
        /// Hiding convention) and flips <see cref="energyCoat"/>'s active
        /// state. The ongoing SP drain while active is paid separately, on
        /// its own timer, by <see cref="PlayerEnergyCoatController"/> itself.
        /// </summary>
        /// <param name="skill">The toggle-drain skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True if the toggle was applied.</returns>
        private bool TryCastToggleDrain(SkillDefinition skill, int level)
        {
            if (energyCoat == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            energyCoat.Toggle(skill, level);
            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Craft"/> skill (e.g. Arrow
        /// Crafting, Aqua Benedicta): spends mana, then attempts
        /// <see cref="SkillDefinition.CraftingRecipe"/> via
        /// <see cref="inventory"/>. If <see cref="SkillDefinition.RequiresNearWater"/>
        /// is true, the caster must also be within <see cref="NearWaterRadius"/>
        /// of a <see cref="waterLayer"/> collider — ponytail: no map in
        /// this prototype tags anything Water yet, so this check is ready
        /// but unexercised, the same "ready the moment one exists" pattern
        /// already used for the Undead/Demon race bonuses before any such
        /// enemy existed. Mana is still spent even if the craft itself
        /// fails (not enough materials, no water nearby, or inventory too
        /// full for the result), matching every other skill here that
        /// spends its cost as soon as the cast is committed.
        /// </summary>
        /// <param name="skill">The craft skill being cast.</param>
        /// <returns>True if the cast was committed (mana spent), regardless of whether the craft itself succeeded.</returns>
        private bool TryCastCraft(SkillDefinition skill)
        {
            if (inventory == null || inventory.Items.IsMinorOverweight || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            if (skill.RequiresNearWater && !Physics.CheckSphere(transform.position, NearWaterRadius, waterLayer))
            {
                return true;
            }

            inventory.Items.TryCraft(skill.CraftingRecipe);
            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Cleanse"/> skill (e.g. Cure,
        /// Detoxify): spends mana and clears every active debuff on the
        /// caster via <see cref="StatusEffectController.ClearAllDebuffs"/>.
        /// Ally targeting resolves to the caster itself, the same stopgap
        /// <see cref="TryCastBuff"/> already uses, until real ally
        /// selection exists.
        /// </summary>
        /// <param name="skill">The cleanse skill being cast.</param>
        /// <returns>True if the cleanse was applied.</returns>
        private bool TryCastCleanse(SkillDefinition skill)
        {
            if (statusEffects == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            statusEffects.ClearAllDebuffs();
            return true;
        }

        /// <summary>
        /// Casts a <see cref="SkillEffectType.Displacement"/> skill (e.g.
        /// Back Slide): spends mana, then pushes the caster
        /// <see cref="SkillDefinition.KnockbackDistance"/> meters away from
        /// the current target via <see cref="KnockbackUtility"/> — or
        /// straight back along the caster's own facing if no target is
        /// selected.
        /// </summary>
        /// <param name="skill">The displacement skill being cast.</param>
        /// <returns>True if the cast was committed (mana spent).</returns>
        private bool TryCastDisplacement(SkillDefinition skill)
        {
            if (agent == null || !mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var source = targetSelector.CurrentTarget != null
                ? targetSelector.CurrentTarget.position
                : transform.position - transform.forward;

            KnockbackUtility.Apply(agent, source, skill.KnockbackDistance);
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
            return (statusEffects != null && (statusEffects.IsImmobilized || statusEffects.IsSilenced))
                || (inventory != null && inventory.Items.IsMajorOverweight);
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