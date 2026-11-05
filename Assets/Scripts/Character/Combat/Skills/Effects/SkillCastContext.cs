using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Project.Skills;
using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// The collaborators of the player casting a skill, handed to every effect handler so handlers never
    /// reach into the caster component. Optional collaborators may be null; handlers check the ones they need.
    /// </summary>
    public sealed class SkillCastContext
    {
        private readonly MonoBehaviour host;

        /// <summary>Initializes the context.</summary>
        /// <param name="host">The component that owns the cast; also runs delayed effects.</param>
        /// <param name="skillBook">The caster's learned skills.</param>
        /// <param name="stats">The caster's stats.</param>
        /// <param name="mana">The caster's mana.</param>
        /// <param name="ownHealth">The caster's health.</param>
        /// <param name="targetSelector">The caster's current target.</param>
        /// <param name="statusEffects">The caster's status effects, or null.</param>
        /// <param name="equipment">The caster's equipment, or null.</param>
        /// <param name="ownBuffs">The caster's buffs, or null.</param>
        /// <param name="enemyLayer">The layer of enemy colliders.</param>
        /// <param name="waterLayer">The layer of water colliders.</param>
        /// <param name="agent">The caster's navigation agent, or null.</param>
        /// <param name="currency">The caster's Zeny, or null.</param>
        /// <param name="energyCoat">The Energy Coat controller, or null.</param>
        /// <param name="inventory">The caster's inventory, or null.</param>
        /// <param name="visuals">The skill visual player.</param>
        public SkillCastContext(
            MonoBehaviour host,
            PlayerSkillBook skillBook,
            PlayerStatsController stats,
            ManaComponent mana,
            HealthComponent ownHealth,
            PlayerTargetSelector targetSelector,
            StatusEffectController statusEffects,
            EquipmentManager equipment,
            BuffController ownBuffs,
            LayerMask enemyLayer,
            LayerMask waterLayer,
            NavMeshAgent agent,
            PlayerCurrency currency,
            PlayerEnergyCoatController energyCoat,
            PlayerInventory inventory,
            SkillCastVisuals visuals)
        {
            this.host = host;
            SkillBook = skillBook;
            Stats = stats;
            Mana = mana;
            OwnHealth = ownHealth;
            TargetSelector = targetSelector;
            StatusEffects = statusEffects;
            Equipment = equipment;
            OwnBuffs = ownBuffs;
            EnemyLayer = enemyLayer;
            WaterLayer = waterLayer;
            Agent = agent;
            Currency = currency;
            EnergyCoat = energyCoat;
            Inventory = inventory;
            Visuals = visuals;
        }

        /// <summary>Gets the caster's transform.</summary>
        public Transform Transform => host.transform;

        /// <summary>Gets the caster's learned skills.</summary>
        public PlayerSkillBook SkillBook { get; }

        /// <summary>Gets the caster's stats.</summary>
        public PlayerStatsController Stats { get; }

        /// <summary>Gets the caster's mana.</summary>
        public ManaComponent Mana { get; }

        /// <summary>Gets the caster's health.</summary>
        public HealthComponent OwnHealth { get; }

        /// <summary>Gets the caster's current target.</summary>
        public PlayerTargetSelector TargetSelector { get; }

        /// <summary>Gets the caster's status effects, or null.</summary>
        public StatusEffectController StatusEffects { get; }

        /// <summary>Gets the caster's equipment, or null.</summary>
        public EquipmentManager Equipment { get; }

        /// <summary>Gets the caster's own buffs, or null.</summary>
        public BuffController OwnBuffs { get; }

        /// <summary>Gets the layer of enemy colliders.</summary>
        public LayerMask EnemyLayer { get; }

        /// <summary>Gets the layer of water colliders.</summary>
        public LayerMask WaterLayer { get; }

        /// <summary>Gets the caster's navigation agent, or null.</summary>
        public NavMeshAgent Agent { get; }

        /// <summary>Gets the caster's Zeny, or null.</summary>
        public PlayerCurrency Currency { get; }

        /// <summary>Gets the Energy Coat controller, or null.</summary>
        public PlayerEnergyCoatController EnergyCoat { get; }

        /// <summary>Gets the caster's inventory, or null.</summary>
        public PlayerInventory Inventory { get; }

        /// <summary>Gets the skill visual player.</summary>
        public SkillCastVisuals Visuals { get; }

        /// <summary>
        /// Gets the equipped main-hand weapon's subtype, or Unarmed when no equipment is wired.
        /// </summary>
        /// <returns>The main-hand weapon subtype.</returns>
        public WeaponSubtype GetMainHandWeaponSubtype()
        {
            return Equipment != null ? Equipment.GetMainHandWeaponSubtype() : WeaponSubtype.Unarmed;
        }

        /// <summary>
        /// Gets how far a target-picked skill reaches: its authored range plus the learned range passive
        /// for skills that fire ammo.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <returns>The effective range, in meters.</returns>
        public float GetEffectiveRange(SkillDefinition skill)
        {
            return skill.Range + (skill.AmmoCost > 0 ? Stats.GetPassiveRangeBonus() : 0f);
        }

        /// <summary>
        /// Gets the flat physical damage bonus from a learned race bane passive; zero for magical damage.
        /// </summary>
        /// <param name="category">The skill's damage category.</param>
        /// <param name="targetRace">The race of the target being hit.</param>
        /// <returns>The flat bonus.</returns>
        public int GetRaceDamageBonus(DamageCategory category, MonsterRace targetRace)
        {
            return category == DamageCategory.Physical ? Stats.GetRaceDamageBonus(targetRace) : 0;
        }

        /// <summary>
        /// Runs the action right away, or after the skill's impact delay so a melee skill's damage lands when
        /// its weapon swing connects.
        /// </summary>
        /// <param name="skill">The skill being resolved.</param>
        /// <param name="resolve">The action to run.</param>
        public void RunAfterImpactDelay(SkillDefinition skill, System.Action resolve)
        {
            if (skill.ImpactDelaySeconds <= 0f)
            {
                resolve();
                return;
            }

            host.StartCoroutine(RunAfterDelay(skill.ImpactDelaySeconds, resolve));
        }

        private static IEnumerator RunAfterDelay(float seconds, System.Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }
    }
}
