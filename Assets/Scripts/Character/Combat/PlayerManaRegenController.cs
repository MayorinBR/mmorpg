using UnityEngine;
using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives natural SP regeneration using the iRO Wiki formula: every
    /// <see cref="regenIntervalSeconds"/> while alive and standing still,
    /// restores <c>1 + floor(MaxSP / 100) + floor(INT / 6)</c>, boosted by a
    /// learned Increase SP Recovery
    /// (<see cref="PlayerPassiveSkillController.GetSpRegenMultiplier"/>).
    /// SP does not regenerate while moving. Unlike <see cref="PlayerHealthRegenController"/>,
    /// taking damage does not reset the tick timer — this project has no
    /// "SP loss from combat" concept to mirror the HP controller's own
    /// grace-period approximation with, and real Ragnarok Online's SP
    /// regen isn't disrupted by being hit either.
    /// </summary>
    public class PlayerManaRegenController : MonoBehaviour
    {
        [Tooltip("The player's mana, restored on each regen tick.")]
        [SerializeField] private ManaComponent mana;

        [Tooltip("Source of Increase SP Recovery's regen multiplier.")]
        [SerializeField] private PlayerPassiveSkillController passiveSkills;

        [Tooltip("Optional. Source of whether the player is dead, used to pause SP regen the same way PlayerHealthRegenController pauses HP regen. Left empty, SP keeps regenerating even after death.")]
        [SerializeField] private HealthComponent health;

        [Tooltip("Optional. When assigned, natural regen stops while the player is minor overweight (70% carried weight or more).")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Source of the player's current movement state, used for the reduced-regen-while-moving penalty. Left empty, the penalty never applies.")]
        [SerializeField] private CharacterMovementController movement;

        [Tooltip("Optional. Source of the player's INT, which adds floor(INT / 6) to each regen tick. Left empty, INT contributes nothing.")]
        [SerializeField] private PlayerStatsController statsController;

        [Tooltip("Seconds between each natural regen tick while standing (iRO Wiki: 8).")]
        [SerializeField] private float regenIntervalSeconds = 8f;

        [Tooltip("Divisor of max mana in the natural regen formula (iRO Wiki: 100).")]
        [SerializeField] private int maxManaDivisor = 100;

        [Tooltip("Divisor of INT in the natural regen formula (iRO Wiki: 6).")]
        [SerializeField] private int intelligenceDivisor = 6;

        private float regenTimer;

        private void Update()
        {
            if ((health != null && health.IsDead) || (inventory != null && inventory.Items.IsMinorOverweight))
            {
                regenTimer = 0f;
                return;
            }

            if (movement != null && movement.IsMoving)
            {
                regenTimer = 0f;
                return;
            }

            regenTimer += Time.deltaTime;

            if (regenTimer < regenIntervalSeconds)
            {
                return;
            }

            regenTimer = 0f;
            ApplyRegenTick();
        }

        private void ApplyRegenTick()
        {
            var intelligence = statsController != null ? statsController.BaseStats.GetValue(StatType.Intelligence) : 0;
            var amount = 1 + mana.MaxMana / maxManaDivisor + intelligence / intelligenceDivisor;
            mana.RestoreMana(Mathf.Max(1, Mathf.RoundToInt(amount * passiveSkills.GetSpRegenMultiplier())));
        }
    }
}
