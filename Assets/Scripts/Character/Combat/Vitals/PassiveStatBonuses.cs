using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Read-only view over the bonuses that learned passive skills grant, resolved against the currently equipped
    /// main-hand weapon. Every member returns zero when the passive source (or, where the bonus is weapon-gated, the
    /// equipment) is not wired, so callers never need their own null checks.
    /// </summary>
    public readonly struct PassiveStatBonuses
    {
        private readonly PlayerPassiveSkillController passiveSkills;
        private readonly EquipmentManager equipment;

        /// <summary>Initializes a new instance of the <see cref="PassiveStatBonuses"/> struct.</summary>
        /// <param name="passiveSkills">The learned passive skills, or null when none are wired.</param>
        /// <param name="equipment">The equipment manager used to resolve the main-hand weapon, or null.</param>
        public PassiveStatBonuses(PlayerPassiveSkillController passiveSkills, EquipmentManager equipment)
        {
            this.passiveSkills = passiveSkills;
            this.equipment = equipment;
        }

        /// <summary>Gets the flat Status ATK bonus of passives matching the equipped main-hand weapon.</summary>
        public int AttackBonus => IsWeaponGated ? passiveSkills.GetAttackBonus(equipment.GetMainHandWeaponSubtype()) : 0;

        /// <summary>Gets the flat DEX bonus of passives matching the equipped main-hand weapon.</summary>
        public int DexBonus => IsWeaponGated ? passiveSkills.GetDexBonus(equipment.GetMainHandWeaponSubtype()) : 0;

        /// <summary>Gets the flat Hit bonus of passives matching the equipped main-hand weapon.</summary>
        public int HitBonus => IsWeaponGated ? passiveSkills.GetHitBonus(equipment.GetMainHandWeaponSubtype()) : 0;

        /// <summary>Gets the flat attack range bonus, in meters, of passives matching the equipped main-hand weapon.</summary>
        public float RangeBonus => IsWeaponGated ? passiveSkills.GetRangeBonus(equipment.GetMainHandWeaponSubtype()) : 0f;

        /// <summary>Gets the chance, from 0 to 1, that a basic attack with the equipped main-hand weapon deals double damage.</summary>
        public float DoubleAttackChance => IsWeaponGated ? passiveSkills.GetDoubleAttackChance(equipment.GetMainHandWeaponSubtype()) : 0f;

        /// <summary>Gets the flat Flee bonus of learned passives; unlike the others it works without equipment.</summary>
        public int FleeBonus
        {
            get
            {
                if (passiveSkills == null)
                {
                    return 0;
                }

                return passiveSkills.GetFleeBonus(equipment != null ? equipment.GetMainHandWeaponSubtype() : WeaponSubtype.Unarmed);
            }
        }

        /// <summary>Gets the extra carry weight, before scaling, granted by passives such as Enlarge Weight Limit.</summary>
        public float WeightLimitBonus => passiveSkills != null ? passiveSkills.GetWeightLimitBonus() : 0f;

        private bool IsWeaponGated => passiveSkills != null && equipment != null;

        /// <summary>Gets the flat physical damage bonus against the given race, from "bane" passives.</summary>
        /// <param name="targetRace">The race of the target being hit.</param>
        /// <returns>The bonus, zero if no passive applies.</returns>
        public int GetRaceDamageBonus(MonsterRace targetRace)
        {
            return passiveSkills != null ? passiveSkills.GetRaceDamageBonus(targetRace) : 0;
        }

        /// <summary>Gets the flat defense bonus against attackers of the given race.</summary>
        /// <param name="attackerRace">The race of the attacker.</param>
        /// <returns>The bonus, zero if no passive applies.</returns>
        public int GetRaceDefenseBonus(MonsterRace attackerRace)
        {
            return passiveSkills != null ? passiveSkills.GetRaceDefenseBonus(attackerRace) : 0;
        }
    }
}
