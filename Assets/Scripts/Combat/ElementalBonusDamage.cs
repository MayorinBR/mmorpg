using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Deals the extra elemental damage some effects add on top of a landed hit (e.g. the Fire bonus left
    /// by Magnum Break). The bonus is a fraction of the hit and ignores the target's defense.
    /// </summary>
    public static class ElementalBonusDamage
    {
        /// <summary>
        /// Calculates the bonus damage a hit adds: a fraction of the hit, at least 1 when any bonus applies.
        /// </summary>
        /// <param name="hitDamage">The damage of the hit the bonus rides on.</param>
        /// <param name="bonusPercent">The bonus as a fraction of the hit, e.g. 0.2 for +20%.</param>
        /// <returns>The bonus damage, zero when the hit or the percent is not positive.</returns>
        public static int CalculateBonus(int hitDamage, float bonusPercent)
        {
            if (bonusPercent <= 0f || hitDamage <= 0)
            {
                return 0;
            }

            return Mathf.Max(1, Mathf.RoundToInt(hitDamage * bonusPercent));
        }

        /// <summary>
        /// Applies the bonus to <paramref name="target"/>. Does nothing if the bonus is zero or the target
        /// cannot take defense-ignoring damage.
        /// </summary>
        /// <param name="target">The target that was just hit.</param>
        /// <param name="hitDamage">The damage of the hit the bonus rides on.</param>
        /// <param name="bonusPercent">The bonus as a fraction of the hit, e.g. 0.2 for +20%.</param>
        /// <param name="element">The element of the bonus damage.</param>
        /// <param name="attacker">Who dealt the hit.</param>
        public static void Apply(IDamageable target, int hitDamage, float bonusPercent, Element element, Transform attacker = null)
        {
            var bonus = CalculateBonus(hitDamage, bonusPercent);

            if (bonus <= 0 || !(target is HealthComponent health))
            {
                return;
            }

            health.TakePiercingDamage(bonus, element, attacker);
        }
    }
}
