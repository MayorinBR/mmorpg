using UnityEngine;
using Project.Combat;
using Project.UI;

namespace Project.AI
{
    /// <summary>
    /// Rage: a timed self-buff to Attack and Attack Speed via
    /// <see cref="BuffController"/>, applied by <see cref="EnemyAttackState"/>'s
    /// own read of <see cref="BuffController.AttackMultiplier"/>/
    /// <see cref="BuffController.AspdMultiplier"/> — no extra plumbing
    /// needed here beyond calling <see cref="BuffController.ApplyBuff"/>.
    /// </summary>
    [System.Serializable]
    public class BossRageSkill
    {
        [SerializeField] private int manaCost = 30;
        [SerializeField] private float cooldownSeconds = 25f;
        [SerializeField] private float durationSeconds = 8f;
        [SerializeField, Range(0f, 2f)] private float atkPercent = 0.5f;
        [SerializeField, Range(0f, 2f)] private float aspdPercent = 0.3f;

        /// <summary>Gets the mana this skill costs to cast.</summary>
        public int ManaCost => manaCost;

        /// <summary>Gets the cooldown, in seconds, between casts.</summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>Gets how long the buff lasts, in seconds.</summary>
        public float DurationSeconds => durationSeconds;

        /// <summary>Gets the attack power bonus, e.g. 0.5 for +50%.</summary>
        public float AtkPercent => atkPercent;

        /// <summary>Gets the attack speed bonus, e.g. 0.3 for +30%.</summary>
        public float AspdPercent => aspdPercent;
    }

    /// <summary>
    /// Poison: damage-over-time inflicted on the boss's current target via
    /// <see cref="StatusEffectController.ApplyPoison"/> — the same
    /// mechanism the player's own Envenom skill uses. The tick interval
    /// (how often the poison actually deals damage) is not configured
    /// here: it's a property of the poisoned target's own
    /// <see cref="StatusEffectController"/>, shared by every source of
    /// Poison that lands on that character, not per-application — set it
    /// on the target's StatusEffectController instead.
    /// </summary>
    [System.Serializable]
    public class BossPoisonSkill
    {
        [SerializeField] private int manaCost = 20;
        [SerializeField] private float cooldownSeconds = 15f;
        [SerializeField] private float durationSeconds = 20f;
        [SerializeField] private int damagePerTick = 10;

        /// <summary>Gets the mana this skill costs to cast.</summary>
        public int ManaCost => manaCost;

        /// <summary>Gets the cooldown, in seconds, between casts.</summary>
        public float CooldownSeconds => cooldownSeconds;

        /// <summary>Gets how long the poison lasts on the target, in seconds.</summary>
        public float DurationSeconds => durationSeconds;

        /// <summary>Gets the damage dealt on each poison tick.</summary>
        public int DamagePerTick => damagePerTick;
    }

    /// <summary>
    /// Casts a boss's unique skills (Rage, a self ATK/ASPD buff, and
    /// Poison, a damage-over-time debuff on its current target) whenever
    /// each is off cooldown and affordable, but only while the boss is
    /// actually in combat (see <see cref="EnemyController.PlayerTarget"/>)
    /// — cooldowns still tick down while idle, so a skill isn't
    /// immediately ready the instant a fight starts, but nothing is ever
    /// cast while wandering the map. While Rage is active, an optional
    /// <see cref="StatusTintController"/> shows a red mask over the
    /// boss's model. Every successful cast also shows a floating
    /// <see cref="DamagePopup"/> with the skill's name above the boss,
    /// the same visual language as its damage numbers, so nearby players
    /// see what it just did.
    /// </summary>
    public class BossSkillController : MonoBehaviour
    {
        [SerializeField] private EnemyController enemy;
        [SerializeField] private ManaComponent mana;

        [Tooltip("Optional. Shows a red mask over the boss's model for as long as Rage is active.")]
        [SerializeField] private StatusTintController rageTint;

        [SerializeField] private BossRageSkill rage;
        [SerializeField] private BossPoisonSkill poison;

        [Tooltip("Where the skill-name popup spawns, relative to this GameObject's position.")]
        [SerializeField] private Vector3 skillNamePopupOffset = new Vector3(0f, 2.5f, 0f);

        private float rageCooldownRemaining;
        private float poisonCooldownRemaining;
        private float rageEndTime;

        /// <summary>Gets whether Rage is currently active.</summary>
        public bool IsRaging => Time.time < rageEndTime;

        private void Update()
        {
            rageCooldownRemaining -= Time.deltaTime;
            poisonCooldownRemaining -= Time.deltaTime;

            if (enemy.PlayerTarget != null)
            {
                TryCastRage();
                TryCastPoison();
            }

            rageTint?.SetActive(IsRaging);
        }

        private void TryCastRage()
        {
            if (rageCooldownRemaining > 0f || mana == null || !mana.TryConsumeMana(rage.ManaCost))
            {
                return;
            }

            rageCooldownRemaining = rage.CooldownSeconds;
            rageEndTime = Time.time + rage.DurationSeconds;
            enemy.Buffs?.ApplyBuff(new BuffPayload(atkPercent: rage.AtkPercent, aspdPercent: rage.AspdPercent), rage.DurationSeconds);
            DamagePopup.CreateSkillName("Rage", transform.position + skillNamePopupOffset);
        }

        private void TryCastPoison()
        {
            if (poisonCooldownRemaining > 0f || mana == null || !mana.TryConsumeMana(poison.ManaCost))
            {
                return;
            }

            // Searched from the hierarchy root, mirroring EnemyAttackState's
            // own target lookup — the target's StatusEffectController can
            // live on a different child than whatever collider PlayerTarget
            // itself points at.
            var targetStatus = enemy.PlayerTarget.root.GetComponentInChildren<StatusEffectController>();

            if (targetStatus == null)
            {
                // ponytail: mana is already spent at this point with no
                // effect landed. PlayerTarget is only ever a player, and
                // every player has a StatusEffectController wired, so this
                // is unreachable in practice today — add a refund here if a
                // future target type without one ever becomes attackable.
                return;
            }

            poisonCooldownRemaining = poison.CooldownSeconds;
            targetStatus.ApplyPoison(poison.DurationSeconds, poison.DamagePerTick);
            DamagePopup.CreateSkillName("Poison", transform.position + skillNamePopupOffset);
        }
    }
}
