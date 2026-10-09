using UnityEngine;
using Project.Character.Movement;
using Project.Character.Stats;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives natural HP regeneration using the iRO Wiki formula: every
    /// <see cref="regenIntervalSeconds"/> while alive, heals
    /// <c>max(1, floor(MaxHP / 200)) + floor(VIT / 5)</c>. While moving the
    /// regen is suspended entirely unless a learned HP Recovery While Moving
    /// is present (<see cref="PlayerPassiveSkillController.IgnoresMovementRegenPenalty"/>),
    /// in which case it runs at half rate. A learned Increase HP Recovery adds
    /// its own flat tick every <see cref="skillRegenIntervalSeconds"/> while
    /// standing still (<see cref="PlayerPassiveSkillController.GetSkillRegenAmount"/>).
    /// Taking damage resets the tick timers, approximating Ragnarok
    /// Online's own "no regen right after being hit" behavior without a
    /// separate grace-period timer.
    /// </summary>
    public class PlayerHealthRegenController : MonoBehaviour
    {
        [Tooltip("The player's health, healed on each regen tick and whose DamageTaken event resets the tick timer.")]
        [SerializeField] private HealthComponent health;

        [Tooltip("Source of Increase HP Recovery's skill regen tick and HP Recovery While Moving's penalty removal.")]
        [SerializeField] private PlayerPassiveSkillController passiveSkills;

        [Tooltip("Optional. When assigned, natural regen stops while the player is minor overweight (70% carried weight or more).")]
        [SerializeField] private PlayerInventory inventory;

        [Tooltip("Optional. Source of the player's current movement state, used for the reduced-regen-while-moving penalty. Left empty, the penalty never applies.")]
        [SerializeField] private CharacterMovementController movement;

        [Tooltip("Optional. Source of the player's VIT, which adds floor(VIT / 5) to each regen tick. Left empty, VIT contributes nothing.")]
        [SerializeField] private PlayerStatsController statsController;

        [Tooltip("Seconds between each natural regen tick while standing (iRO Wiki: 6).")]
        [SerializeField] private float regenIntervalSeconds = 6f;

        [Tooltip("Seconds between each Increase HP Recovery tick while standing (iRO Wiki: 10).")]
        [SerializeField] private float skillRegenIntervalSeconds = 10f;

        [Tooltip("Divisor of max health in the natural regen formula (iRO Wiki: 200).")]
        [SerializeField] private int maxHealthDivisor = 200;

        [Tooltip("Divisor of VIT in the natural regen formula (iRO Wiki: 5).")]
        [SerializeField] private int vitalityDivisor = 5;

        [Tooltip("Multiplier applied to the regen tick while moving with HP Recovery While Moving learned.")]
        [SerializeField] private float movementRegenPenaltyMultiplier = 0.5f;

        private float regenTimer;
        private float skillRegenTimer;

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
            if (health.IsDead || (inventory != null && inventory.Items.IsMinorOverweight))
            {
                ResetTimers();
                return;
            }

            var moving = movement != null && movement.IsMoving;
            var movingRegenAllowed = passiveSkills.IgnoresMovementRegenPenalty();

            regenTimer += Time.deltaTime;

            if (regenTimer >= regenIntervalSeconds)
            {
                regenTimer = 0f;

                if (!moving || movingRegenAllowed)
                {
                    ApplyRegenTick(moving ? movementRegenPenaltyMultiplier : 1f);
                }
            }

            if (moving)
            {
                skillRegenTimer = 0f;
                return;
            }

            skillRegenTimer += Time.deltaTime;

            if (skillRegenTimer >= skillRegenIntervalSeconds)
            {
                skillRegenTimer = 0f;
                ApplySkillRegenTick();
            }
        }

        private void HandleDamageTaken(int amount, bool isCritical, Element element)
        {
            ResetTimers();
        }

        private void ResetTimers()
        {
            regenTimer = 0f;
            skillRegenTimer = 0f;
        }

        private void ApplyRegenTick(float multiplier)
        {
            var vitality = statsController != null ? statsController.BaseStats.GetValue(StatType.Vitality) : 0;
            var amount = Mathf.Max(1, health.MaxHealth / maxHealthDivisor) + vitality / vitalityDivisor;
            health.Heal(Mathf.Max(1, Mathf.RoundToInt(amount * multiplier)));
        }

        private void ApplySkillRegenTick()
        {
            var amount = Mathf.RoundToInt(passiveSkills.GetSkillRegenAmount(health.MaxHealth));

            if (amount > 0)
            {
                health.Heal(amount);
            }
        }
    }
}
