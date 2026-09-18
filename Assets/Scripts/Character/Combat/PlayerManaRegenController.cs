using UnityEngine;
using Project.Character.Movement;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives natural SP regeneration, mirroring <see cref="PlayerHealthRegenController"/>'s
    /// own HP regen tick exactly, just for mana instead: every
    /// <see cref="regenIntervalSeconds"/> while alive, restores a
    /// percentage of max mana, boosted by a learned Increase SP Recovery
    /// (<see cref="PlayerPassiveSkillController.GetSpRegenMultiplier"/>)
    /// and halved while moving. Unlike <see cref="PlayerHealthRegenController"/>,
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

        [Tooltip("Optional. Source of the player's current movement state, used for the reduced-regen-while-moving penalty. Left empty, the penalty never applies.")]
        [SerializeField] private CharacterMovementController movement;

        [Tooltip("Seconds between each natural regen tick. Real Ragnarok Online's own formula depends on INT; this project uses a flat interval instead, matching PlayerHealthRegenController's own approach.")]
        [SerializeField] private float regenIntervalSeconds = 10f;

        [Tooltip("Fraction of max mana restored per regen tick, before any multiplier. An approximation of Ragnarok Online's own regen formula, not a sourced value.")]
        [SerializeField] private float regenPercentOfMaxMana = 0.01f;

        [Tooltip("Multiplier applied to the regen tick while moving.")]
        [SerializeField] private float movementRegenPenaltyMultiplier = 0.5f;

        private float regenTimer;

        private void Update()
        {
            if (health != null && health.IsDead)
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
            var multiplier = passiveSkills.GetSpRegenMultiplier();

            if (movement != null && movement.IsMoving)
            {
                multiplier *= movementRegenPenaltyMultiplier;
            }

            mana.RestoreMana(Mathf.Max(1, Mathf.RoundToInt(mana.MaxMana * regenPercentOfMaxMana * multiplier)));
        }
    }
}
