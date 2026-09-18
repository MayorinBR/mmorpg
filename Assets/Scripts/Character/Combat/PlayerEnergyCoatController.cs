using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives Energy Coat: an SP-draining toggle buff that reduces incoming
    /// damage while active. Mirrors <see cref="PlayerBerserkController"/>'s
    /// persistent-modifier pattern, but the trigger is a player toggle
    /// (<see cref="Toggle"/>, called from
    /// <see cref="PlayerSkillCaster"/>'s <see cref="SkillEffectType.ToggleDrain"/>
    /// dispatch) instead of an HP threshold, and it drains
    /// <see cref="mana"/> on its own timer instead of just watching an
    /// event — the same tick shape <see cref="PlayerManaRegenController"/>
    /// already established for SP, just draining instead of restoring.
    /// Auto-deactivates the moment a drain tick can't be paid.
    /// </summary>
    public class PlayerEnergyCoatController : MonoBehaviour
    {
        private const float DrainIntervalSeconds = 1f;

        [Tooltip("The player's mana, drained on a timer while Energy Coat is active.")]
        [SerializeField] private ManaComponent mana;

        [Tooltip("Target of the incoming-damage-reduction modifier while Energy Coat is active.")]
        [SerializeField] private BuffController buffs;

        private SkillDefinition energyCoatSkill;
        private float drainTimer;

        /// <summary>Gets whether Energy Coat is currently active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>
        /// Toggles Energy Coat on or off. Turning it on applies the
        /// skill's incoming-damage-reduction bonus at the given level and
        /// starts the SP drain timer; turning it off (recasting, or an
        /// automatic drain failure) clears both.
        /// </summary>
        /// <param name="skill">The Energy Coat skill being toggled.</param>
        /// <param name="skillLevel">The skill's current level.</param>
        public void Toggle(SkillDefinition skill, int skillLevel)
        {
            if (IsActive)
            {
                Deactivate();
                return;
            }

            energyCoatSkill = skill;
            drainTimer = DrainIntervalSeconds;
            IsActive = true;
            buffs.SetPersistentModifier(this, new BuffPayload(incomingDamageReductionPercent: skill.GetToggleDrainIncomingDamageReductionPercent(skillLevel)));
        }

        private void Update()
        {
            if (!IsActive)
            {
                return;
            }

            drainTimer -= Time.deltaTime;

            if (drainTimer > 0f)
            {
                return;
            }

            drainTimer += DrainIntervalSeconds;

            if (!mana.TryConsumeMana(Mathf.RoundToInt(energyCoatSkill.ToggleDrainManaPerSecond)))
            {
                Deactivate();
            }
        }

        private void Deactivate()
        {
            IsActive = false;
            energyCoatSkill = null;
            buffs.SetPersistentModifier(this, default);
        }
    }
}
