using UnityEngine;
using Project.Character.Movement;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives natural HP regeneration: every <see cref="regenIntervalSeconds"/>
    /// while alive, heals a percentage of max health, boosted by a learned
    /// Increase HP Recovery (<see cref="PlayerPassiveSkillController.GetRegenMultiplier"/>)
    /// and halved while moving unless a learned HP Recovery While Moving
    /// removes that penalty (<see cref="PlayerPassiveSkillController.IgnoresMovementRegenPenalty"/>).
    /// Taking damage resets the tick timer, approximating Ragnarok
    /// Online's own "no regen right after being hit" behavior without a
    /// separate grace-period timer.
    /// </summary>
    public class PlayerHealthRegenController : MonoBehaviour
    {
        [Tooltip("The player's health, healed on each regen tick and whose DamageTaken event resets the tick timer.")]
        [SerializeField] private HealthComponent health;

        [Tooltip("Source of Increase HP Recovery's regen multiplier and HP Recovery While Moving's penalty removal.")]
        [SerializeField] private PlayerPassiveSkillController passiveSkills;

        [Tooltip("Optional. Source of the player's current movement state, used for the reduced-regen-while-moving penalty. Left empty, the penalty never applies.")]
        [SerializeField] private CharacterMovementController movement;

        [Tooltip("Seconds between each natural regen tick. Real Ragnarok Online's own formula depends on AGI; this project uses a flat interval instead.")]
        [SerializeField] private float regenIntervalSeconds = 10f;

        [Tooltip("Fraction of max health restored per regen tick, before any multiplier. An approximation of Ragnarok Online's own regen formula, not a sourced value.")]
        [SerializeField] private float regenPercentOfMaxHealth = 0.01f;

        [Tooltip("Multiplier applied to the regen tick while moving, unless HP Recovery While Moving is learned.")]
        [SerializeField] private float movementRegenPenaltyMultiplier = 0.5f;

        private float regenTimer;

        private void OnEnable()
        {
            health.DamageTaken += HandleDamageTaken;
        }

        private void OnDisable()
        {
            health.DamageTaken -= HandleDamageTaken;
        }

        private void Update()
        {
            if (health.IsDead)
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

        private void HandleDamageTaken(int amount, bool isCritical)
        {
            regenTimer = 0f;
        }

        private void ApplyRegenTick()
        {
            var multiplier = passiveSkills.GetRegenMultiplier();

            if (movement != null && movement.IsMoving && !passiveSkills.IgnoresMovementRegenPenalty())
            {
                multiplier *= movementRegenPenaltyMultiplier;
            }

            health.Heal(Mathf.Max(1, Mathf.RoundToInt(health.MaxHealth * regenPercentOfMaxHealth * multiplier)));
        }
    }
}
