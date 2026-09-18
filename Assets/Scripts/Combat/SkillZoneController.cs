using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// A persistent, ground-placed skill effect (e.g. Fire Wall): damages
    /// every living <see cref="IDamageable"/> found on the given layer
    /// within its radius, on a fixed tick interval, until its duration runs
    /// out, then destroys itself. Damage, accuracy and the hit's
    /// element/category are fixed at <see cref="Initialize"/> time — the
    /// caster's stats when the skill was cast — the same as every other
    /// skill in this project resolving its effect once at cast time rather
    /// than re-reading live stats on every tick. A zone can instead be
    /// initialized via <see cref="InitializeBlock"/> (e.g. Safety Wall,
    /// Pneuma) to block attacks of a given <see cref="AttackRangeKind"/>
    /// within its radius instead of damaging — see
    /// <see cref="BlocksAttack"/>.
    /// </summary>
    public class SkillZoneController : MonoBehaviour
    {
        private static readonly List<SkillZoneController> blockingZones = new List<SkillZoneController>();

        private LayerMask targetLayer;
        private int accuracy;
        private int damage;
        private Element element;
        private DamageCategory category;
        private Transform caster;
        private float radius;
        private float tickIntervalSeconds;
        private float durationRemaining;
        private float tickRemaining;
        private AttackRangeKind? blockedRangeKind;

        /// <summary>
        /// Spawns a new zone at <paramref name="position"/>, using
        /// <paramref name="prefab"/> for its visuals if one is assigned
        /// (it must carry its own <see cref="SkillZoneController"/>, or one
        /// is added), or a bare invisible GameObject otherwise. Call
        /// <see cref="Initialize"/> right after to set what it hits.
        /// </summary>
        /// <param name="prefab">Optional visual prefab, or null for a logic-only zone.</param>
        /// <param name="position">World position to spawn the zone at.</param>
        /// <param name="radius">Radius, in meters, the zone affects.</param>
        /// <param name="durationSeconds">How long the zone lasts before it despawns.</param>
        /// <param name="tickIntervalSeconds">Seconds between damage ticks against anything inside.</param>
        /// <returns>The spawned zone's controller.</returns>
        public static SkillZoneController Spawn(GameObject prefab, Vector3 position, float radius, float durationSeconds, float tickIntervalSeconds)
        {
            var instance = prefab != null ? Instantiate(prefab, position, Quaternion.identity) : new GameObject("SkillZone");
            instance.transform.position = position;

            var controller = instance.GetComponent<SkillZoneController>();
            if (controller == null)
            {
                controller = instance.AddComponent<SkillZoneController>();
            }

            controller.radius = radius;
            controller.durationRemaining = durationSeconds;
            controller.tickIntervalSeconds = tickIntervalSeconds;
            return controller;
        }

        /// <summary>
        /// Sets the effect this zone deals to whatever it hits. Must be
        /// called right after <see cref="Spawn"/>, before the first tick.
        /// </summary>
        /// <param name="targetLayer">Layer the zone checks for living targets on.</param>
        /// <param name="accuracy">The caster's Hit rating, plus any skill accuracy bonus, at cast time.</param>
        /// <param name="damage">The fixed damage dealt on each tick a target is hit.</param>
        /// <param name="element">The element this zone's damage carries.</param>
        /// <param name="category">Whether this damage is mitigated by physical or magical defense.</param>
        /// <param name="caster">The transform that cast the skill, passed to <see cref="IDamageable.TakeDamage"/> so a hit enemy can aggro onto it, the same as any other skill hit.</param>
        public void Initialize(LayerMask targetLayer, int accuracy, int damage, Element element, DamageCategory category, Transform caster)
        {
            this.targetLayer = targetLayer;
            this.accuracy = accuracy;
            this.damage = damage;
            this.element = element;
            this.category = category;
            this.caster = caster;
        }

        /// <summary>
        /// Marks this zone as attack-blocking instead of damaging — e.g.
        /// Safety Wall blocking melee, Pneuma blocking ranged. Call instead
        /// of <see cref="Initialize"/> right after <see cref="Spawn"/>; a
        /// blocking zone never ticks damage. Registers this zone so
        /// <see cref="BlocksAttack"/> can find it until it despawns.
        /// </summary>
        /// <param name="rangeKind">Which attack range this zone blocks.</param>
        public void InitializeBlock(AttackRangeKind rangeKind)
        {
            blockedRangeKind = rangeKind;
            blockingZones.Add(this);
        }

        private void OnDestroy()
        {
            blockingZones.Remove(this);
        }

        /// <summary>
        /// Checks whether any active blocking zone (see
        /// <see cref="InitializeBlock"/>) of the given
        /// <paramref name="rangeKind"/> covers <paramref name="position"/>
        /// — e.g. an enemy's melee attack against a player standing inside
        /// Safety Wall. Used by <see cref="AI.EnemyAttackState"/> before it
        /// resolves an attack against the player.
        /// </summary>
        /// <param name="position">The position being attacked, e.g. the target's own position.</param>
        /// <param name="rangeKind">The attack's range kind.</param>
        /// <returns>True if a matching blocking zone covers the position.</returns>
        public static bool BlocksAttack(Vector3 position, AttackRangeKind rangeKind)
        {
            foreach (var zone in blockingZones)
            {
                if (zone.blockedRangeKind == rangeKind && Vector3.Distance(zone.transform.position, position) <= zone.radius)
                {
                    return true;
                }
            }

            return false;
        }

        private void Update()
        {
            durationRemaining -= Time.deltaTime;

            if (durationRemaining <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            if (blockedRangeKind.HasValue)
            {
                // A blocking zone (Safety Wall, Pneuma) never ticks damage
                // — BlocksAttack is a static query against blockingZones,
                // not something resolved here.
                return;
            }

            tickRemaining -= Time.deltaTime;

            if (tickRemaining > 0f)
            {
                return;
            }

            tickRemaining = tickIntervalSeconds;
            Tick();
        }

        // ponytail: no WeaponSizeModifiers scaling on zone damage — it's
        // spell-like (matches real RO's Fire Wall not being a weapon hit),
        // and Project.Combat can't reference Project.Items (WeaponSizeModifiers'
        // assembly) anyway without creating a cycle. Revisit if a physical
        // zone skill ever needs size scaling.
        private void Tick()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, radius, targetLayer);
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
                    target.TakeDamage(damage, element, category, attacker: caster);
                }
                else
                {
                    target.NotifyDodged();
                }
            }
        }
    }
}
