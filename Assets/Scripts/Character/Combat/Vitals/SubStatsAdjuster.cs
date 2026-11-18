using Project.Character.Stats;
using Project.Combat;
using Project.Items;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Layers weapon, passive and buff contributions on top of the sub-stats the calculator derived from the base stats.
    /// </summary>
    public static class SubStatsAdjuster
    {
        /// <summary>Applies the equipped weapon's own values, passive skill bonuses and buff multipliers.</summary>
        /// <param name="raw">The sub-stats derived from the effective base stats.</param>
        /// <param name="equipment">The equipment manager, or null when none is wired.</param>
        /// <param name="passives">The passive skill bonuses.</param>
        /// <param name="buffs">The buff controller, or null when none is wired.</param>
        /// <returns>The adjusted sub-stats; the same instance when nothing changed.</returns>
        public static SubStats Apply(SubStats raw, EquipmentManager equipment, PassiveStatBonuses passives, BuffController buffs)
        {
            var weaponAttack = equipment != null ? equipment.GetMainHandWeaponAttack() : 0;
            var weaponMagicAttack = equipment != null ? equipment.GetMainHandWeaponMagicAttack() : 0;
            var weaponMagicDefense = equipment != null ? equipment.GetMainHandWeaponMagicDefense() : 0;

            var statusAtk = raw.StatusAtk + passives.AttackBonus + weaponAttack;
            var statusMatk = raw.StatusMatk + weaponMagicAttack;
            var statusMDef = raw.StatusMDef + weaponMagicDefense;
            var hit = raw.Hit + passives.HitBonus;
            var flee = raw.Flee + passives.FleeBonus;
            var aspd = raw.Aspd;

            if (buffs != null)
            {
                statusAtk = Mathf.RoundToInt(statusAtk * buffs.AttackMultiplier);
                aspd = Mathf.RoundToInt(aspd * buffs.AspdMultiplier);
            }

            var unchanged = statusAtk == raw.StatusAtk && statusMatk == raw.StatusMatk && statusMDef == raw.StatusMDef && hit == raw.Hit && flee == raw.Flee && aspd == raw.Aspd;
            return unchanged
                ? raw
                : new SubStats(statusAtk, statusMatk, raw.StatusDef, statusMDef, hit, flee, raw.CriticalRate, aspd);
        }
    }
}
