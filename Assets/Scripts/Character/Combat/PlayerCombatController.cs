using Project.Character.Animation;
using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using Project.Skills;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives player auto-attack: while a target is selected, walks into
    /// attack range if needed, then attacks on a cooldown derived from the
    /// player's Aspd sub-stat (<see cref="AttackSpeedCalculator"/>) — higher
    /// Aspd attacks, and plays the swing animation, faster. Skill casts are
    /// unaffected: they run on their own per-skill cooldown
    /// (<see cref="PlayerSkillCaster"/>), never on this one. Attack range
    /// comes from the equipped
    /// main-hand weapon's own <see cref="ItemDefinition.AttackRange"/>,
    /// for every class — different weapons of the same
    /// <see cref="WeaponType"/> can have different ranges (a dagger isn't
    /// a spear, a short bow isn't a crossbow). An unarmed player falls
    /// back to <see cref="unarmedRange"/>. Skill range is unaffected by
    /// this and continues to come from each
    /// <see cref="Skills.SkillDefinition"/>'s own Range value (see
    /// <see cref="PlayerSkillCaster"/>). Class still affects other
    /// behavior: Archer, while wielding a Ranged weapon, consumes
    /// equipped ammo per shot and cannot attack while out of ammo; Mage spends mana per basic attack; Thief
    /// hits twice when dual-wielding two one-handed weapons. Damage is
    /// applied through <see cref="Project.Combat.IDamageable"/>, the same
    /// contract enemies use.
    /// </summary>
    /// <remarks>
    /// Whether a swing plays the melee or the ranged (bow) attack
    /// animation is decided by the equipped weapon's
    /// <see cref="WeaponType"/>, the same check already used for the
    /// Archer's ammo consumption. Mage is the one exception: its basic
    /// attack always plays the spell cast animation and always reaches out
    /// to <see cref="mageAttackRange"/>, regardless of the equipped
    /// weapon — a Mage attacks at range by class, not because of what's in
    /// its hand. Hit/miss and damage are always decided the instant the
    /// attack fires (see <see cref="DealHit"/>), the same immediate-resolve
    /// approach skill casts already use in <see cref="PlayerSkillCaster"/>.
    /// Applying that outcome to the target happens in step with the
    /// animation trigger too, UNLESS a ranged basic attack has a
    /// <see cref="Project.Combat.Projectile"/> prefab assigned
    /// (<see cref="arrowProjectilePrefab"/>/<see cref="boltProjectilePrefab"/>),
    /// in which case it's deferred until that projectile visually reaches
    /// the target (see <see cref="ResolveOutcome"/>) — purely a visual
    /// travel delay, the outcome itself doesn't change. The Mage's basic attack also
    /// carries its <see cref="PlayerElementController.CurrentElement"/>
    /// into the hit; every other class's basic attack deals
    /// <see cref="Project.Combat.Element.Neutral"/> damage — modeled as a
    /// real, resistable element rather than "no element", so a target's
    /// <see cref="Project.Combat.ElementalResistanceComponent"/> can scale
    /// ordinary physical damage too, not just elemental hits.
    /// </remarks>
    /// <remarks>
    /// Basic-attack damage is class-dependent the same way Ragnarok Online
    /// itself is: the Mage's spell scales off <see cref="Character.Stats.SubStats.StatusMatk"/>
    /// (INT-based), while every other class scales off
    /// <see cref="Character.Stats.SubStats.StatusAtk"/> — which
    /// <see cref="Character.Stats.SubStatsCalculator"/> derives from STR for
    /// a melee weapon or DEX for a ranged one (bow/gun/instrument/whip).
    /// That weapon check, not the player's job, is what makes an
    /// Archer's shots scale off DEX; a Swordman who somehow picked up a
    /// bow would scale off DEX too, exactly as in real Ragnarok Online.
    /// </remarks>
    /// <remarks>
    /// Every hit (see <see cref="DealHit"/>) is resolved against the
    /// target's Flee via <see cref="HitChanceCalculator"/> before it's
    /// applied — except a critical hit, which always lands, matching
    /// Ragnarok Online's own "crits bypass accuracy" rule. The Mage's basic
    /// attack is <see cref="DamageCategory.Magical"/> (mitigated by the
    /// target's magical defense); every other class's is
    /// <see cref="DamageCategory.Physical"/> — and, being physical, also
    /// scaled by <see cref="WeaponSizeModifiers"/> for the equipped
    /// weapon's subtype against the target's size (e.g. a dagger dealing
    /// half damage to a Large monster).
    /// </remarks>
    public class PlayerCombatController : MonoBehaviour
    {
        private const float CriticalDamageMultiplier = 1.4f;
        private const float OffHandDamageMultiplier = 0.5f;
        private const float AmmoWarningIntervalSeconds = 2f;
        private const float ParallelArrowSpacing = 0.12f;
        private const int RainArrowCount = 8;
        private const float RainRadius = 1.2f;
        private const float RainHeight = 5f;
        private const float RainStagger = 0.8f;
        private const float RainSpeedMultiplier = 1.5f;

        [SerializeField] private PlayerStatsController playerStats;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerElementController elementController;
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private ManaComponent mana;
        [SerializeField] private PlayerAnimatorController animatorController;

        [Tooltip("Optional. When assigned, basic attacks are blocked while the player is major overweight (90% carried weight or more).")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Source of active status effects (see StatusEffectController) affecting the player — e.g. a Stun/Freeze/Petrify landed on them. Left empty, the player is never immobilized by one.")]
        [SerializeField] private StatusEffectController statusEffects;

        [SerializeField] private float unarmedRange = 1.5f;

        [Header("Mage Basic Attack")]
        [SerializeField] private int mageManaCostPerAttack = 2;

        // Less than a bow's range so the Mage doesn't out-range the
        // Archer, but still enough to cast from a safe distance instead
        // of standing in melee like an unarmed weapon would put it.
        [SerializeField] private float mageAttackRange = 4f;

        [Header("Ranged Projectile (Archer arrow / Mage bolt)")]
        [Tooltip("Visual prefab spawned for the Archer's basic attack while wielding a Ranged weapon. Left empty, that attack hits instantly with no travel delay, same as every melee attack.")]
        [SerializeField] private GameObject arrowProjectilePrefab;

        [Tooltip("Visual prefab spawned for the Mage's basic attack. Left empty, it hits instantly with no travel delay.")]
        [SerializeField] private GameObject boltProjectilePrefab;

        [Tooltip("Speed, in meters/second, a basic-attack projectile travels at.")]
        [SerializeField] private float projectileSpeed = 15f;

        [Tooltip("World position a basic-attack projectile spawns from, e.g. a bow/staff hand socket. Left empty, falls back to this transform's position.")]
        [SerializeField] private Transform projectileOrigin;

        [Tooltip("If true, a ranged basic attack's projectile only spawns once ReleaseProjectile() is called — wire it as an Animation Event on the AttackRanged/Cast clip, at the frame the arrow/bolt should leave the hand. If false (default), the projectile spawns immediately when the attack triggers, same as before this option existed.")]
        [SerializeField] private bool releaseProjectileOnAnimationEvent;

        private float cooldownRemaining;
        private PlayerSkillCaster skillCaster;
        private float nextAmmoWarningTime;
        private System.Action pendingProjectileRelease;
        private float meleeImpactDelaySeconds;

        /// <summary>
        /// Gets or sets whether chasing and auto attacks are paused because a
        /// skill is walking the player into its own range. Set by
        /// <see cref="SkillTargetingController"/> while it holds a queued skill.
        /// </summary>
        public bool IsSkillApproachPending { get; set; }

        private bool IsSkillCasting()
        {
            if (skillCaster == null)
            {
                skillCaster = transform.root.GetComponentInChildren<PlayerSkillCaster>(true);
            }

            return skillCaster != null && skillCaster.IsCasting;
        }

        private void Update()
        {
            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                movementController.SetMovementLocked(false);
                return;
            }

            if (targetSelector.CurrentDamageable.IsDead)
            {
                movementController.SetMovementLocked(false);
                targetSelector.ClearTarget();
                return;
            }

            if (IsSkillApproachPending)
            {
                movementController.SetMovementLocked(false);
                return;
            }

            if (IsSkillCasting())
            {
                return;
            }

            var attackRange = GetAttackRange();
            var distanceToTarget = CombatRangeMath.HorizontalDistance(transform.position, targetSelector.CurrentTarget.position);

            if (distanceToTarget > attackRange)
            {
                // Still closing the distance — let movement keep facing
                // where it's walking until the player is actually in range.
                movementController.SetMovementLocked(false);
                movementController.SetClickDestination(targetSelector.CurrentTarget.position);
                return;
            }

            // In range: combat owns the character entirely from here on,
            // so movement's own rotation, its NavMeshAgent (residual
            // velocity, local avoidance) and a still-held directional key
            // never fight this frame's target-facing, or make the
            // Animator's Speed parameter flicker mid-swing — which was
            // what made the attack animation's feet drift.
            movementController.SetMovementLocked(true);

            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0f && CanAttack())
            {
                // Faced only once, right as this swing starts — not every
                // frame the player stays in range. Reassigning
                // transform.forward every frame kept nudging the whole
                // character (legs included, since nothing else was
                // rotating them) throughout the swing, which is what made
                // the legs look like they were still turning instead of
                // holding the animation's own stance; a single snap here
                // still guarantees every swing faces wherever the target
                // currently is.
                FaceTarget(targetSelector.CurrentTarget.position);
                PerformAttack();
                cooldownRemaining = GetAttackIntervalSeconds();
            }
        }

        private void FaceTarget(Vector3 targetPosition)
        {
            var directionToTarget = targetPosition - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude > 0.0001f)
            {
                transform.forward = directionToTarget.normalized;
            }
        }

        private float GetAttackRange()
        {
            if (classController.CurrentClass == CharacterClass.Mage)
            {
                return mageAttackRange;
            }

            var mainHandWeapon = GetMainHandWeapon();
            var baseRange = mainHandWeapon != null ? mainHandWeapon.AttackRange : unarmedRange;
            return baseRange + (playerStats != null ? playerStats.GetPassiveRangeBonus() : 0f);
        }

        private bool CanAttack()
        {
            if (inventory != null && inventory.Items.IsMajorOverweight)
            {
                return false;
            }

            if (statusEffects != null && statusEffects.IsImmobilized)
            {
                return false;
            }

            if (classController.CurrentClass == CharacterClass.Mage)
            {
                return mana.CurrentMana >= mageManaCostPerAttack;
            }

            if (classController.CurrentClass == CharacterClass.Archer && equipment.IsMainHandWeaponRanged() && equipment.EquippedAmmoCount <= 0)
            {
                if (Time.time >= nextAmmoWarningTime)
                {
                    nextAmmoWarningTime = Time.time + AmmoWarningIntervalSeconds;
                    PlayerFeedbackChannel.Publish("No ammo equipped.");
                }

                return false;
            }

            return true;
        }

        private void PerformAttack()
        {
            // Safety net: if the previous attack queued a projectile release
            // (releaseProjectileOnAnimationEvent) and its Animation Event
            // never fired — clip not wired yet — fire it now instead of
            // dropping that hit's outcome silently. Worst case it lands one
            // attack late instead of never.
            ReleaseProjectile();

            var isRanged = equipment.IsMainHandWeaponRanged();

            // Only the Attack state is bound to the AttackSpeedMultiplier
            // parameter this sets, so it has no effect on the Mage's Cast
            // state — the Mage's basic attack always plays at its authored
            // speed, not scaled by Aspd, since Cast is shared with real
            // skill casts. The swing's actual playback duration is made to
            // equal this same interval — not just scaled proportionally to
            // it — so the animation and the cooldown it's tied to are never
            // out of sync. Which clip the Attack state plays comes from the
            // equipped weapon (see PlayerWeaponAnimationBinder).
            animatorController?.SetAttackDuration(GetAttackIntervalSeconds());
            meleeImpactDelaySeconds = animatorController != null
                ? animatorController.AttackImpactNormalizedTime * GetAttackIntervalSeconds()
                : 0f;

            if (classController.CurrentClass == CharacterClass.Mage)
            {
                animatorController?.TriggerCast();
            }
            else
            {
                animatorController?.TriggerAttack();
            }

            if (classController.CurrentClass == CharacterClass.Mage)
            {
                mana.TryConsumeMana(mageManaCostPerAttack);
            }

            var isMage = classController.CurrentClass == CharacterClass.Mage;

            // Every class deals StatusATK damage (STR-based for melee,
            // DEX-based for a ranged weapon — see SubStatsCalculator) except
            // the Mage, whose basic attack is a spell and so scales off
            // StatusMATK (INT-based) instead, the same stat its skills use.
            var baseDamage = isMage ? playerStats.CurrentSubStats.StatusMatk : playerStats.CurrentSubStats.StatusAtk;

            var arrowPrefab = ResolveArrowPrefab();

            if (classController.CurrentClass == CharacterClass.Archer && isRanged)
            {
                equipment.TryConsumeAmmo();
            }

            // Only the Mage's basic attack carries a "real" element today —
            // every other class deals Neutral damage, which still goes
            // through a target's ElementalResistanceComponent like any
            // other element (Neutral is plain physical damage, not "no
            // element").
            var attackElement = isMage
                ? elementController.CurrentElement
                : Element.Neutral;

            var attackCategory = isMage ? DamageCategory.Magical : DamageCategory.Physical;

            // Melee attacks (and a ranged one with no prefab assigned) get
            // null here, which DealHit treats as "resolve instantly" — see
            // its own doc.
            var projectilePrefab = isMage ? boltProjectilePrefab : (isRanged ? arrowPrefab : null);

            DealHit(targetSelector.CurrentDamageable, baseDamage, attackElement, attackCategory, projectilePrefab);

            if (classController.CurrentClass == CharacterClass.Thief && IsDualWielding())
            {
                DealHit(targetSelector.CurrentDamageable, Mathf.RoundToInt(playerStats.CurrentSubStats.StatusAtk * OffHandDamageMultiplier));
            }
        }

        private float GetAttackIntervalSeconds()
        {
            var weapon = GetMainHandWeapon();
            var weaponMultiplier = weapon != null ? weapon.AttackSpeedMultiplier : 1f;
            return AttackSpeedCalculator.GetAttackIntervalSeconds(playerStats.CurrentSubStats.Aspd, weaponMultiplier);
        }

        private ItemDefinition GetMainHandWeapon()
        {
            return equipment.GetEquippedItems(EquipmentSlot.LeftHand).FirstOrDefault();
        }

        private bool IsDualWielding()
        {
            var mainHandItems = equipment.GetEquippedItems(EquipmentSlot.LeftHand);
            var offHandItems = equipment.GetEquippedItems(EquipmentSlot.RightHand);

            if (mainHandItems.Count == 0 || offHandItems.Count == 0)
            {
                return false;
            }

            return mainHandItems[0] != offHandItems[0];
        }

        /// <summary>
        /// Resolves one hit against a target: a critical hit always lands
        /// (bypassing the accuracy check entirely, the same way Ragnarok
        /// Online itself works) and deals bonus damage; otherwise the hit
        /// is subject to <see cref="HitChanceCalculator"/> and can miss
        /// outright. Hit, crit and damage are always decided instantly,
        /// right here — <paramref name="projectilePrefab"/> only affects
        /// when the resolved outcome (damage or a dodge notification) is
        /// applied to <paramref name="target"/>, via <see cref="ResolveOutcome"/>.
        /// </summary>
        private void DealHit(IDamageable target, int baseDamage, Element element = Element.Neutral, DamageCategory category = DamageCategory.Physical, GameObject projectilePrefab = null)
        {
            var isCriticalHit = Random.value * 100f < playerStats.CurrentSubStats.CriticalRate;

            if (!isCriticalHit && !HitChanceCalculator.RollHit(playerStats.CurrentSubStats.Hit, target.FleeRating))
            {
                ResolveOutcome(target, projectilePrefab, target.NotifyDodged);
                return;
            }

            var damage = isCriticalHit
                ? Mathf.RoundToInt(baseDamage * CriticalDamageMultiplier)
                : baseDamage;

            damage = WeaponSizeModifiers.Apply(damage, category, equipment.GetMainHandWeaponSubtype(), target.Size);

            if (category == DamageCategory.Physical)
            {
                damage += playerStats.GetRaceDamageBonus(target.Race);
            }

            ResolveOutcome(target, projectilePrefab, () =>
            {
                target.TakeDamage(damage, element, category, isCriticalHit, transform);
                ElementalBonusDamage.Apply(target, damage, playerStats.FireDamagePercent, Element.Fire, transform);
            });
        }

        /// <summary>
        /// Applies an already-resolved hit outcome (damage or a dodge
        /// notification) immediately, or — when <paramref name="projectilePrefab"/>
        /// is assigned — after a <see cref="Project.Combat.Projectile"/>
        /// visually travels from <see cref="projectileOrigin"/> to the
        /// target first, purely for the arrow/bolt's travel time; the
        /// outcome itself was already decided in <see cref="DealHit"/> and
        /// doesn't change based on whether the projectile "connects". With
        /// <see cref="releaseProjectileOnAnimationEvent"/> on, the
        /// projectile doesn't spawn here at all — it's queued for
        /// <see cref="ReleaseProjectile"/> to spawn once the attack's
        /// animation actually reaches its release frame.
        /// </summary>
        /// <param name="target">The target the outcome applies to.</param>
        /// <param name="projectilePrefab">Visual prefab to travel first, or null to apply instantly.</param>
        /// <param name="applyOutcome">Applies the already-decided damage or dodge notification.</param>
        private void ResolveOutcome(IDamageable target, GameObject projectilePrefab, System.Action applyOutcome)
        {
            var targetTransform = (target as Component)?.transform;

            if (projectilePrefab == null || targetTransform == null)
            {
                if (projectilePrefab == null && targetTransform != null && meleeImpactDelaySeconds > 0f)
                {
                    StartCoroutine(ApplyAfterDelay(meleeImpactDelaySeconds, targetTransform, applyOutcome));
                    return;
                }

                applyOutcome();
                return;
            }

            if (releaseProjectileOnAnimationEvent)
            {
                pendingProjectileRelease = () => SpawnProjectile(projectilePrefab, targetTransform, applyOutcome);
                return;
            }

            SpawnProjectile(projectilePrefab, targetTransform, applyOutcome);
        }

        /// <summary>
        /// Applies an already-resolved melee outcome once the swing animation reaches its impact moment, dropping it
        /// when the target no longer exists by then.
        /// </summary>
        /// <param name="seconds">How long after the attack started the blow lands.</param>
        /// <param name="targetTransform">The transform of the target the outcome applies to.</param>
        /// <param name="applyOutcome">Applies the already-decided damage or dodge notification.</param>
        private IEnumerator ApplyAfterDelay(float seconds, Transform targetTransform, System.Action applyOutcome)
        {
            yield return new WaitForSeconds(seconds);

            if (targetTransform != null)
            {
                applyOutcome();
            }
        }

        private void SpawnProjectile(GameObject projectilePrefab, Transform targetTransform, System.Action applyOutcome)
        {
            var origin = projectileOrigin != null ? projectileOrigin.position : transform.position;

            Projectile.Spawn(projectilePrefab, origin, targetTransform, projectileSpeed, () =>
            {
                // The target may have been destroyed while the projectile
                // was still travelling (e.g. despawned on death, or the
                // player warped away) — drop the outcome instead of
                // calling into a destroyed object.
                if (targetTransform != null)
                {
                    applyOutcome();
                }
            });
        }

        /// <summary>
        /// Picks the arrow visual for the next shot: the model authored on the equipped ammo item when it has one,
        /// otherwise the shared <see cref="arrowProjectilePrefab"/>.
        /// </summary>
        /// <returns>The projectile prefab to spawn, or null when neither is available.</returns>
        private GameObject ResolveArrowPrefab()
        {
            var ammo = equipment.EquippedAmmoItem;
            var ammoModel = ammo != null ? ammo.GetEquippedModel(default) : null;
            return ammoModel != null ? ammoModel : arrowProjectilePrefab;
        }

        /// <summary>
        /// Fires the arrow or bolt visual at a target for a skill cast, using the same prefabs, speed and
        /// origin as the basic attack. Purely cosmetic: the skill's outcome is decided by the caster.
        /// </summary>
        /// <param name="magical">True for the Mage's bolt, false for the Archer's arrow.</param>
        /// <param name="target">The transform the projectile flies to.</param>
        public void SpawnSkillProjectile(bool magical, Transform target)
        {
            var prefab = magical ? boltProjectilePrefab : ResolveArrowPrefab();

            if (prefab == null || target == null)
            {
                return;
            }

            var origin = projectileOrigin != null ? projectileOrigin.position : transform.position;
            Projectile.Spawn(prefab, origin, target, projectileSpeed, null);
        }

        /// <summary>
        /// Draws the arrows of an ammo skill in the style the skill asks for (see <see cref="ArrowVolleyStyle"/>),
        /// attaching the skill's aura to each one. Purely cosmetic: the skill's outcome is decided by the caster.
        /// </summary>
        /// <param name="skill">The ammo skill being cast.</param>
        /// <param name="target">The transform the arrows fly to.</param>
        public void SpawnSkillArrows(SkillDefinition skill, Transform target)
        {
            var prefab = ResolveArrowPrefab();

            if (prefab == null || target == null)
            {
                return;
            }

            var origin = projectileOrigin != null ? projectileOrigin.position : transform.position;

            switch (skill.ArrowVolleyStyle)
            {
                case ArrowVolleyStyle.Parallel:
                    var side = Vector3.Cross(Vector3.up, target.position - origin).normalized * ParallelArrowSpacing;
                    AttachAura(skill, Projectile.Spawn(prefab, origin + side, target, projectileSpeed, null, side));
                    AttachAura(skill, Projectile.Spawn(prefab, origin - side, target, projectileSpeed, null, -side));
                    break;

                case ArrowVolleyStyle.Rain:
                    for (var i = 0; i < RainArrowCount; i++)
                    {
                        var landing = target.position + Quaternion.Euler(0f, Random.value * 360f, 0f) * Vector3.forward * (Random.value * RainRadius);
                        var start = landing + Vector3.up * (RainHeight + i * RainStagger);
                        AttachAura(skill, Projectile.SpawnToPoint(prefab, start, landing, projectileSpeed * RainSpeedMultiplier));
                    }

                    break;

                default:
                    AttachAura(skill, Projectile.Spawn(prefab, origin, target, projectileSpeed, null));
                    break;
            }
        }

        private static void AttachAura(SkillDefinition skill, GameObject arrow)
        {
            if (skill.ArrowAuraPrefab != null)
            {
                Instantiate(skill.ArrowAuraPrefab, arrow.transform);
            }
        }

        /// <summary>
        /// Fires a skill's own projectile effect at a target and runs a callback when it arrives.
        /// </summary>
        /// <param name="prefab">The skill's projectile effect.</param>
        /// <param name="target">The transform the projectile flies to.</param>
        /// <param name="onArrival">Invoked when the projectile reaches the target.</param>
        public void SpawnSkillProjectile(GameObject prefab, Transform target, System.Action onArrival)
        {
            var origin = projectileOrigin != null ? projectileOrigin.position : transform.position;
            Projectile.Spawn(prefab, origin, target, projectileSpeed, onArrival);
        }

        /// <summary>
        /// Spawns the projectile queued by the most recent ranged basic
        /// attack, if <see cref="releaseProjectileOnAnimationEvent"/> is on
        /// and one is still pending — call this from an Animation Event on
        /// the Archer's AttackRanged clip or the Mage's Cast clip, at the
        /// exact frame the arrow leaves the bow or the bolt leaves the
        /// hand. Hit/miss and damage were already decided the instant the
        /// attack triggered (see <see cref="DealHit"/>); this only decides
        /// when the visual leaves and, with it, when that already-decided
        /// outcome lands. Also called defensively at the start of every
        /// <see cref="PerformAttack"/>, so a clip that never fires this
        /// event doesn't silently drop the previous attack's outcome — it
        /// lands at worst one attack late instead of never.
        /// </summary>
        public void ReleaseProjectile()
        {
            var release = pendingProjectileRelease;
            pendingProjectileRelease = null;
            release?.Invoke();
        }
    }
}