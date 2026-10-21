using System;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Optional per-character table of resistances and weaknesses, covering
    /// both elemental damage and plain physical damage — the latter is
    /// modeled as <see cref="Element.Neutral"/> rather than being a special
    /// case, matching how Ragnarok Online itself treats Neutral as a real,
    /// resistable property rather than "no element". Plugs into
    /// <see cref="HealthComponent"/> through the same
    /// <see cref="IDamageModifier"/> hook <see cref="Project.Character.Combat.PlayerBlockController"/>
    /// already uses for Swordman's block, so it can be attached to any
    /// character — player or enemy — without <see cref="HealthComponent"/>
    /// needing to know resistances exist. An <see cref="Element"/> with no
    /// matching entry always passes through unchanged, Neutral included.
    /// </summary>
    /// <remarks>
    /// <see cref="HealthComponent"/> only supports a single wired
    /// <see cref="IDamageModifier"/> today (see its <c>damageModifierSource</c>
    /// field), so a character cannot yet combine this with another modifier
    /// such as block — tracked as a known follow-up in
    /// FUTURE_IMPROVEMENTS.md rather than solved here, since no character
    /// currently needs both at once.
    /// </remarks>
    public class ElementalResistanceComponent : MonoBehaviour, IDamageModifier
    {
        [Serializable]
        private struct ResistanceEntry
        {
            public Element element;

            [Tooltip("Multiplier applied to incoming damage of this element. 1 = no effect, 0.5 = resists half, 1.5 = 50% weaker against it (takes extra damage), 0 = fully immune.")]
            public float multiplier;
        }

        [SerializeField] private ResistanceEntry[] resistances = Array.Empty<ResistanceEntry>();

        private Element temporaryElement;
        private float temporaryElementExpireTime;

        /// <summary>
        /// Makes the character take damage as if it were of the given element until the duration runs out,
        /// replacing its own resistance table meanwhile — e.g. a frozen target becoming Water, a petrified
        /// one becoming Earth. Only Water and Ground have a chart; any other element leaves damage unchanged.
        /// </summary>
        /// <param name="element">The element the character temporarily becomes.</param>
        /// <param name="durationSeconds">How long the change lasts, in seconds.</param>
        public void ApplyTemporaryElement(Element element, float durationSeconds)
        {
            temporaryElement = element;
            temporaryElementExpireTime = Time.time + durationSeconds;
        }

        /// <inheritdoc />
        public int ModifyIncomingDamage(int amount, Element element)
        {
            if (Time.time < temporaryElementExpireTime)
            {
                return Mathf.RoundToInt(amount * GetLevelOneMultiplier(temporaryElement, element));
            }

            foreach (var entry in resistances)
            {
                if (entry.element == element)
                {
                    return Mathf.RoundToInt(amount * entry.multiplier);
                }
            }

            return amount;
        }

        private static float GetLevelOneMultiplier(Element defender, Element attacker)
        {
            switch (defender)
            {
                case Element.Water:
                    return attacker switch
                    {
                        Element.Water => 0.25f,
                        Element.Fire => 0.9f,
                        Element.Electric => 1.5f,
                        Element.Poison => 1.5f,
                        _ => 1f
                    };
                case Element.Ground:
                    return attacker switch
                    {
                        Element.Ground => 0.25f,
                        Element.Fire => 1.5f,
                        Element.Electric => 0.9f,
                        Element.Poison => 1.5f,
                        _ => 1f
                    };
                default:
                    return 1f;
            }
        }
    }
}
