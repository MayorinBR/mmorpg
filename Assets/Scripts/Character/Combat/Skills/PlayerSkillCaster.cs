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
    /// Runs the common flow of casting a skill: checks it is learned, off cooldown and affordable, handles
    /// target picking, the cast time and its interruption, the cooldown, and the cast animation. What the skill
    /// actually does is resolved by an <see cref="ISkillEffectHandler"/> chosen by the skill's effect type (see
    /// <see cref="SkillEffectHandlerRegistry"/>), so a new kind of effect never touches this class.
    /// </summary>
    public class PlayerSkillCaster : MonoBehaviour
    {
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
        private readonly SkillEffectHandlerRegistry handlers = SkillEffectHandlerRegistry.CreateDefault();
        private readonly ZoneEffectHandler zoneHandler = new ZoneEffectHandler();

        private Coroutine castRoutine;
        private CharacterMovementController movement;
        private SkillCastContext castContext;

        /// <summary>Gets whether a skill with a cast time is currently being cast; no other skill can start meanwhile.</summary>
        public bool IsCasting => castRoutine != null;

        /// <summary>Raised when a timed cast begins, with its total duration in seconds.</summary>
        public event System.Action<float> CastStarted;

        /// <summary>Raised when a timed cast ends, whether it completed or was interrupted.</summary>
        public event System.Action CastEnded;

        private SkillCastContext Context => castContext ??= BuildContext();

        private void OnDisable()
        {
            if (castRoutine != null)
            {
                castRoutine = null;
                CastEnded?.Invoke();
            }
        }

        /// <summary>
        /// Registers the handler that resolves a kind of skill effect, replacing the default one.
        /// </summary>
        /// <param name="type">The effect type the handler resolves.</param>
        /// <param name="handler">The handler.</param>
        public void RegisterEffectHandler(SkillEffectType type, ISkillEffectHandler handler)
        {
            handlers.Register(type, handler);
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
        /// Attempts to cast the given skill. For a skill that needs an enemy target and has none, this hands
        /// off to <see cref="SkillTargetingController"/> to let the player pick one instead of just failing, so
        /// this can return false either because the skill genuinely can't be cast (not learned, on cooldown) or
        /// because it just started waiting for a target pick.
        /// </summary>
        /// <param name="skill">The skill to cast.</param>
        /// <returns>True if the skill was successfully cast; false if any requirement wasn't met, or a target pick was started instead.</returns>
        public bool TryCastSkill(SkillDefinition skill)
        {
            if (ownHealth.IsDead || IsCasting)
            {
                return false;
            }

            if (IsCastBlocked())
            {
                PlayerFeedbackChannel.Publish("You cannot cast right now (silenced, immobilized or overweight).");
                return false;
            }

            var level = skillBook.GetLevel(skill);

            if (level <= 0)
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} has not been learned.");
                return false;
            }

            if (Time.time < GetCooldownEndTime(skill))
            {
                CombatLog.Write($"{skill.SkillName} on cooldown: {GetCooldownRemaining(skill):0.0}s left.");
                return false;
            }

            if (skill.EffectType == SkillEffectType.Passive)
            {
                // Passive skills apply their bonus automatically while learned and are never actually cast.
                return false;
            }

            var requirements = Context;

            if (!SkillRequirementChecker.MeetsWeaponRequirement(requirements, skill)
                || !SkillRequirementChecker.MeetsCatalystRequirement(requirements, skill)
                || !SkillRequirementChecker.MeetsResourceRequirement(requirements, skill))
            {
                return false;
            }

            SkillTargetingController.Instance?.ClearQueuedSkill();

            if (skill.TargetType == SkillTargetType.Ground)
            {
                // A Ground skill needs a fresh position pick on every cast; TryCastSkillAtPosition does the
                // actual casting once SkillTargetingController confirms one.
                SkillTargetingController.Instance?.BeginPicking(skill);
                return false;
            }

            if (NeedsEnemyTarget(skill) && !HasValidDamageTarget(skill))
            {
                CombatLog.Write($"{skill.SkillName}: no target in range ({Context.GetEffectiveRange(skill):0.#} max), asking for a target.");
                var targeting = SkillTargetingController.Instance;

                if (targeting != null && !targeting.TryQueueSkill(skill))
                {
                    targeting.BeginPicking(skill);
                }

                return false;
            }

            var castTime = GetCastTime(skill, level);

            if (castTime > 0f)
            {
                if (mana.CurrentMana < skill.ManaCost)
                {
                    return false;
                }

                BeginCast(castTime, () => SkillRequirementChecker.MeetsWeaponRequirement(Context, skill)
                    && SkillRequirementChecker.MeetsCatalystRequirement(Context, skill)
                    && ExecuteCast(skill, level));
                return true;
            }

            return ExecuteCast(skill, level);
        }

        /// <summary>
        /// Casts a <see cref="SkillTargetType.Ground"/> skill at a world position the player just picked via
        /// <see cref="SkillTargetingController"/>. Mirrors <see cref="TryCastSkill"/>'s learned, cooldown and
        /// dead checks, since a Ground skill's cost and cooldown are only spent once a position is confirmed.
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

            if (level <= 0 || Time.time < GetCooldownEndTime(skill)
                || !SkillRequirementChecker.MeetsCatalystRequirement(Context, skill)
                || !SkillRequirementChecker.MeetsResourceRequirement(Context, skill))
            {
                return false;
            }

            SkillTargetingController.Instance?.ClearQueuedSkill();

            var castTime = GetCastTime(skill, level);

            if (castTime > 0f)
            {
                if (mana.CurrentMana < skill.ManaCost)
                {
                    return false;
                }

                BeginCast(castTime, () => ExecuteZoneCast(skill, level, position));
                return true;
            }

            return ExecuteZoneCast(skill, level, position);
        }

        /// <summary>
        /// Checks whether the given skill could be cast right now, without spending any resources or triggering
        /// its cooldown. Intended for UI (hotbar icon state) rather than the actual cast flow.
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
        /// Gets the remaining cooldown time for a skill, in seconds.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>Seconds remaining before the skill is off cooldown, or 0 if it's already ready.</returns>
        public float GetCooldownRemaining(SkillDefinition skill)
        {
            return Mathf.Max(0f, GetCooldownEndTime(skill) - Time.time);
        }

        /// <summary>
        /// Checks whether the current combat target is a usable target for this skill (selected, alive and
        /// within range). Public so <see cref="SkillTargetingController"/> can re-check it right after the
        /// player confirms a picked target. An area skill centered on the caster needs no target at all, so
        /// this is always true for one.
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

            return CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position) <= Context.GetEffectiveRange(skill);
        }

        /// <summary>
        /// Checks whether a skill needs a valid current enemy target before it can be cast: every Damage skill,
        /// plus an Enemy-targeted Buff skill such as Provoke.
        /// </summary>
        /// <param name="skill">The skill to check.</param>
        /// <returns>True if the skill requires a targeted enemy in range.</returns>
        private static bool NeedsEnemyTarget(SkillDefinition skill)
        {
            return skill.EffectType == SkillEffectType.Damage
                || (skill.EffectType == SkillEffectType.Buff && skill.TargetType == SkillTargetType.Enemy);
        }

        /// <summary>
        /// Resolves a skill's effect through its handler, then spends its ammo and catalyst and starts its
        /// cooldown when it took effect, playing the release animation at that moment.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True if the skill took effect.</returns>
        private bool ExecuteCast(SkillDefinition skill, int level)
        {
            var cast = handlers.Get(skill.EffectType).TryCast(Context, skill, level);
            CombatLog.Write($"{skill.SkillName} lv{level}: {(cast ? "cast" : "not cast")}.");

            if (cast)
            {
                SkillRequirementChecker.SpendAmmo(Context, skill);
                SkillRequirementChecker.SpendCatalyst(Context, skill);
                SkillAnnouncementChannel.Announce(skill.SkillName, transform.position);
                Context.Visuals.PlayWeaponVfx(skill);

                if (skill.TargetType == SkillTargetType.Self)
                {
                    SkillVfx.Spawn(skill.ImpactVfxPrefab, transform.position);
                }

                PlayCastAnimation(skill);
                cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
                StopMovementForCast();
            }

            return cast;
        }

        /// <summary>
        /// Spawns a ground skill's zone, then spends its catalyst and starts its cooldown, playing the release
        /// animation at that moment.
        /// </summary>
        /// <param name="skill">The ground skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="position">The world position the zone spawns at.</param>
        /// <returns>True if the zone was spawned.</returns>
        private bool ExecuteZoneCast(SkillDefinition skill, int level, Vector3 position)
        {
            if (!SkillRequirementChecker.MeetsCatalystRequirement(Context, skill) || !zoneHandler.TryCast(Context, skill, level, position))
            {
                return false;
            }

            SkillRequirementChecker.SpendCatalyst(Context, skill);
            SkillAnnouncementChannel.Announce(skill.SkillName, transform.position);

            animatorController?.TriggerCast();
            StopMovementForCast();
            cooldownEndTimes[skill] = Time.time + skill.CooldownSeconds;
            return true;
        }

        /// <summary>
        /// Plays the animation of a cast skill: the equipped weapon's basic-attack swing for a skill that strikes
        /// with the weapon, the cast animation for every other skill.
        /// </summary>
        /// <param name="skill">The skill that was just cast.</param>
        private void PlayCastAnimation(SkillDefinition skill)
        {
            var combatController = GetCombat();

            if (skill.UsesAttackAnimation && combatController != null)
            {
                combatController.PlaySkillSwing();
                return;
            }

            animatorController?.TriggerCast();
        }

        /// <summary>
        /// Gets the combat controller that spawns skill projectiles, looking it up on this object's parents and
        /// children when it was not assigned.
        /// </summary>
        /// <returns>The combat controller, or null when none exists.</returns>
        private PlayerCombatController GetCombat()
        {
            if (combat == null)
            {
                combat = GetComponentInParent<PlayerCombatController>() ?? GetComponentInChildren<PlayerCombatController>();
            }

            return combat;
        }

        private SkillCastContext BuildContext()
        {
            return new SkillCastContext(
                this,
                skillBook,
                statsController,
                mana,
                ownHealth,
                targetSelector,
                statusEffects,
                equipment,
                ownBuffs,
                enemyLayer,
                waterLayer,
                agent,
                currency,
                energyCoat,
                inventory,
                new SkillCastVisuals(gameObject, GetCombat));
        }

        /// <summary>
        /// Gets how long the skill takes to cast: its fixed part plus its variable part scaled by the caster's
        /// DEX and INT.
        /// </summary>
        private float GetCastTime(SkillDefinition skill, int level)
        {
            return skill.GetFixedCastTime(level) + skill.GetVariableCastTime(level) * statsController.GetCastTimeMultiplier();
        }

        /// <summary>
        /// Halts the character where it stands, so casting never happens while walking and a timed
        /// cast is not interrupted by leftover movement.
        /// </summary>
        private void StopMovementForCast()
        {
            if (movement == null)
            {
                movement = transform.root.GetComponentInChildren<CharacterMovementController>(true);
            }

            if (movement != null)
            {
                movement.StopMovement();
            }
        }

        private void BeginCast(float castTime, System.Func<bool> onComplete)
        {
            StopMovementForCast();
            animatorController?.SetChanneling(true, castTime);
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
                var walkedAway = (transform.position - origin).sqrMagnitude > CastMoveTolerance * CastMoveTolerance;

                if (ownHealth.IsDead || IsCastBlocked() || walkedAway)
                {
                    castRoutine = null;
                    animatorController?.SetChanneling(false);

                    if (!ownHealth.IsDead && !walkedAway)
                    {
                        animatorController?.TriggerHit();
                    }

                    CastEnded?.Invoke();
                    PlayerFeedbackChannel.Publish("Cast interrupted.");
                    yield break;
                }

                yield return null;
            }

            castRoutine = null;
            animatorController?.SetChanneling(false);
            CastEnded?.Invoke();
            onComplete();
        }

        private float GetCooldownEndTime(SkillDefinition skill)
        {
            return cooldownEndTimes.TryGetValue(skill, out var endTime) ? endTime : 0f;
        }

        /// <summary>
        /// Gets whether an active status effect is currently blocking the caster from casting anything: Stun,
        /// Freeze and Petrify block every action, Silence blocks casting alone, and heavy overweight blocks it too.
        /// </summary>
        private bool IsCastBlocked()
        {
            return (statusEffects != null && (statusEffects.IsImmobilized || statusEffects.IsSilenced))
                || (inventory != null && inventory.Items.IsMajorOverweight);
        }
    }
}
