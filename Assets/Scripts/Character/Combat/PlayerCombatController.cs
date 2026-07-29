using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using System.Linq;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives player auto-attack: while a target is selected, walks into
    /// attack range if needed, then attacks on a fixed cooldown using the
    /// player's calculated sub-stats. Attack range comes from the equipped
    /// main-hand weapon's own <see cref="ItemDefinition.AttackRange"/>,
    /// for every class — different weapons of the same
    /// <see cref="WeaponType"/> can have different ranges (a dagger isn't
    /// a spear, a short bow isn't a crossbow). An unarmed player falls
    /// back to <see cref="unarmedRange"/>. Skill range is unaffected by
    /// this and continues to come from each
    /// <see cref="Skills.SkillDefinition"/>'s own Range value (see
    /// <see cref="PlayerSkillCaster"/>). Class still affects other
    /// behavior: Archer, while wielding a Ranged weapon, consumes
    /// equipped ammo per shot (falling back to a weaker but infinite base
    /// shot when out of ammo); Mage spends mana per basic attack; Thief
    /// hits twice when dual-wielding two one-handed weapons. Damage is
    /// applied through <see cref="Project.Combat.IDamageable"/>, the same
    /// contract enemies use.
    /// </summary>
    public class PlayerCombatController : MonoBehaviour
    {
        private const float CriticalDamageMultiplier = 1.4f;
        private const float OffHandDamageMultiplier = 0.5f;

        [SerializeField] private PlayerStatsController playerStats;
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private CharacterMovementController movementController;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private EquipmentManager equipment;
        [SerializeField] private ManaComponent mana;

        [SerializeField] private float unarmedRange = 1.5f;

        [Header("Mage Basic Attack")]
        [SerializeField] private int mageManaCostPerAttack = 2;

        [Header("Archer Basic Attack")]
        [SerializeField, Range(0f, 1f)] private float archerBaseAmmoDamageMultiplier = 0.5f;

        [SerializeField] private float attackCooldownSeconds = 1f;

        private float cooldownRemaining;

        private void Update()
        {
            if (targetSelector.CurrentTarget == null || targetSelector.CurrentDamageable == null)
            {
                return;
            }

            if (targetSelector.CurrentDamageable.IsDead)
            {
                targetSelector.ClearTarget();
                return;
            }

            var attackRange = GetAttackRange();
            var distanceToTarget = Vector3.Distance(transform.position, targetSelector.CurrentTarget.position);

            if (distanceToTarget > attackRange)
            {
                movementController.SetClickDestination(targetSelector.CurrentTarget.position);
                return;
            }

            movementController.StopMovement();

            var directionToTarget = targetSelector.CurrentTarget.position - transform.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude > 0.0001f)
            {
                transform.forward = directionToTarget.normalized;
            }

            cooldownRemaining -= Time.deltaTime;

            if (cooldownRemaining <= 0f && CanAttack())
            {
                PerformAttack();
                cooldownRemaining = attackCooldownSeconds;
            }
        }

        private float GetAttackRange()
        {
            var mainHandWeapon = GetMainHandWeapon();
            return mainHandWeapon != null ? mainHandWeapon.AttackRange : unarmedRange;
        }

        private bool CanAttack()
        {
            if (classController.CurrentClass == CharacterClass.Mage)
            {
                return mana.CurrentMana >= mageManaCostPerAttack;
            }

            // Archer always attacks: real ammo while wielding a Ranged
            // weapon, an infinite weaker base shot otherwise. Every other
            // class is unrestricted.
            return true;
        }

        private void PerformAttack()
        {
            if (classController.CurrentClass == CharacterClass.Mage)
            {
                mana.TryConsumeMana(mageManaCostPerAttack);
            }

            var baseDamage = playerStats.CurrentSubStats.StatusAtk;

            if (classController.CurrentClass == CharacterClass.Archer && equipment.IsMainHandWeaponRanged())
            {
                if (!equipment.TryConsumeAmmo())
                {
                    baseDamage = Mathf.Max(1, Mathf.RoundToInt(baseDamage * archerBaseAmmoDamageMultiplier));
                }
            }

            DealHit(baseDamage);

            if (classController.CurrentClass == CharacterClass.Thief && IsDualWielding())
            {
                DealHit(Mathf.RoundToInt(playerStats.CurrentSubStats.StatusAtk * OffHandDamageMultiplier));
            }
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

        private void DealHit(int baseDamage)
        {
            var isCriticalHit = Random.value * 100f < playerStats.CurrentSubStats.CriticalRate;
            var damage = isCriticalHit
                ? Mathf.RoundToInt(baseDamage * CriticalDamageMultiplier)
                : baseDamage;

            targetSelector.CurrentDamageable.TakeDamage(damage);
        }
    }
}