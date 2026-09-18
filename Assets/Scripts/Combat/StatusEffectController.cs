using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Holds a character's active timed status effects: Stun, Poison,
    /// Silence, Blind, Freeze and Petrify. Works the same way for the
    /// player and for an enemy, wired as an optional hook on both (see
    /// <see cref="Project.AI.EnemyController"/>). Stun, Freeze and Petrify
    /// all fully immobilize a character in real Ragnarok Online, so they
    /// share <see cref="IsImmobilized"/> as a single check for the
    /// movement/basic-attack/skill-cast gates that don't care which of the
    /// three caused it.
    /// </summary>
    public class StatusEffectController : MonoBehaviour
    {
        [Tooltip("Optional. Health this character takes Poison's damage-over-time tick from. Left empty, Poison behaves as a flag only (no damage).")]
        [SerializeField] private HealthComponent health;

        [Tooltip("Seconds between each Poison damage tick.")]
        [SerializeField] private float poisonTickIntervalSeconds = 1f;

        private float stunExpireTime;
        private float poisonExpireTime;
        private float silenceExpireTime;
        private float blindExpireTime;
        private float freezeExpireTime;
        private float petrifyExpireTime;
        private int poisonDamagePerTick;
        private float poisonTickTimer;
        private bool isHidden;

        /// <summary>Gets whether this character is currently stunned.</summary>
        public bool IsStunned => Time.time < stunExpireTime;

        /// <summary>Gets whether this character is currently poisoned. See <see cref="ApplyPoison"/> for its damage-over-time tick.</summary>
        public bool IsPoisoned => Time.time < poisonExpireTime;

        /// <summary>Gets whether this character is currently silenced, blocking skill casts only (movement and basic attacks are unaffected).</summary>
        public bool IsSilenced => Time.time < silenceExpireTime;

        /// <summary>
        /// Gets whether this character is currently blinded.
        /// ponytail: not wired to any accuracy check yet — real Ragnarok
        /// Online's Blind sharply reduces Hit, but every
        /// <see cref="HitChanceCalculator.RollHit"/> caller would need
        /// updating to read it. Add that once a skill actually applies Blind.
        /// </summary>
        public bool IsBlinded => Time.time < blindExpireTime;

        /// <summary>Gets whether this character is currently frozen (immobilized, see <see cref="IsImmobilized"/>).</summary>
        public bool IsFrozen => Time.time < freezeExpireTime;

        /// <summary>Gets whether this character is currently petrified (immobilized, see <see cref="IsImmobilized"/>).</summary>
        public bool IsPetrified => Time.time < petrifyExpireTime;

        /// <summary>Gets whether Stun, Freeze or Petrify is currently blocking movement, basic attacks and skill casts.</summary>
        public bool IsImmobilized => IsStunned || IsFrozen || IsPetrified;

        /// <summary>
        /// Gets whether this character is currently hidden — e.g. Hiding
        /// (Thief). Unlike every other status here, this isn't timed: it
        /// stays true until explicitly toggled off (<see cref="SetHidden"/>)
        /// or a reveal skill clears it (<see cref="Reveal"/>), since real
        /// Ragnarok Online's Hiding is a toggle, not a duration.
        /// </summary>
        public bool IsHidden => isHidden;

        /// <summary>
        /// Applies a stun lasting the given duration. Extends the current
        /// stun rather than shortening it if one is already active and
        /// would outlast this one.
        /// </summary>
        /// <param name="durationSeconds">How long the stun should last, in seconds.</param>
        public void ApplyStun(float durationSeconds)
        {
            stunExpireTime = Extend(stunExpireTime, durationSeconds);
        }

        /// <summary>
        /// Applies Poison for the given duration, ticking the given flat
        /// damage every <see cref="poisonTickIntervalSeconds"/> through the
        /// wired <see cref="health"/>. Re-applying while already poisoned
        /// extends the duration and keeps the stronger of the two tick
        /// amounts; re-applying after it expired starts fresh at the new amount.
        /// </summary>
        /// <param name="durationSeconds">How long the poison should last, in seconds.</param>
        /// <param name="damagePerTick">Flat damage dealt on each tick, mitigated by the target's magical defense like any other Magical hit.</param>
        public void ApplyPoison(float durationSeconds, int damagePerTick)
        {
            poisonDamagePerTick = IsPoisoned ? Mathf.Max(poisonDamagePerTick, damagePerTick) : damagePerTick;
            poisonExpireTime = Extend(poisonExpireTime, durationSeconds);
        }

        /// <summary>Applies Silence for the given duration, blocking skill casts only.</summary>
        /// <param name="durationSeconds">How long the silence should last, in seconds.</param>
        public void ApplySilence(float durationSeconds)
        {
            silenceExpireTime = Extend(silenceExpireTime, durationSeconds);
        }

        /// <summary>Applies Blind for the given duration. See <see cref="IsBlinded"/> for why nothing reacts to it yet.</summary>
        /// <param name="durationSeconds">How long the blind should last, in seconds.</param>
        public void ApplyBlind(float durationSeconds)
        {
            blindExpireTime = Extend(blindExpireTime, durationSeconds);
        }

        /// <summary>Applies Freeze for the given duration, immobilizing the character (see <see cref="IsImmobilized"/>).</summary>
        /// <param name="durationSeconds">How long the freeze should last, in seconds.</param>
        public void ApplyFreeze(float durationSeconds)
        {
            freezeExpireTime = Extend(freezeExpireTime, durationSeconds);
        }

        /// <summary>Applies Petrify for the given duration, immobilizing the character (see <see cref="IsImmobilized"/>).</summary>
        /// <param name="durationSeconds">How long the petrify should last, in seconds.</param>
        public void ApplyPetrify(float durationSeconds)
        {
            petrifyExpireTime = Extend(petrifyExpireTime, durationSeconds);
        }

        /// <summary>Turns Hiding on or off directly — e.g. casting Hiding again to cancel it.</summary>
        /// <param name="hidden">True to become hidden, false to become visible.</param>
        public void SetHidden(bool hidden)
        {
            isHidden = hidden;
        }

        /// <summary>Clears Hiding immediately, e.g. a reveal skill (Sight/Ruwach) hitting this character. No-ops if not hidden.</summary>
        public void Reveal()
        {
            isHidden = false;
        }

        /// <summary>
        /// Clears every active timed debuff (Stun, Poison, Silence, Blind,
        /// Freeze, Petrify) immediately — e.g. Cure, Detoxify. Doesn't touch
        /// <see cref="IsHidden"/>, which isn't a debuff.
        /// ponytail: clears everything rather than the specific subset real
        /// Ragnarok Online's Cure (Stun/Silence/Blind) or Detoxify (Poison)
        /// remove individually — this project already treats "one status
        /// per skill" as an accepted simplification elsewhere (e.g. Stone
        /// Fling's single inflicted status), so a single clear-everything
        /// method covers both real skills without a new per-skill status list.
        /// </summary>
        public void ClearAllDebuffs()
        {
            stunExpireTime = 0f;
            poisonExpireTime = 0f;
            silenceExpireTime = 0f;
            blindExpireTime = 0f;
            freezeExpireTime = 0f;
            petrifyExpireTime = 0f;
        }

        /// <summary>
        /// Applies the named status effect for the given duration, dispatching
        /// to the matching Apply* method — e.g. lets a skill (see
        /// <see cref="Project.Skills.SkillDefinition.InflictedStatus"/>) name
        /// which status it inflicts without a caller-side switch. Does
        /// nothing for <see cref="StatusEffectType.None"/>.
        /// </summary>
        /// <param name="type">Which status to apply.</param>
        /// <param name="durationSeconds">How long it should last, in seconds.</param>
        /// <param name="poisonDamagePerTick">Damage per tick, only used when <paramref name="type"/> is <see cref="StatusEffectType.Poison"/>.</param>
        public void Apply(StatusEffectType type, float durationSeconds, int poisonDamagePerTick = 0)
        {
            switch (type)
            {
                case StatusEffectType.Stun:
                    ApplyStun(durationSeconds);
                    break;
                case StatusEffectType.Poison:
                    ApplyPoison(durationSeconds, poisonDamagePerTick);
                    break;
                case StatusEffectType.Silence:
                    ApplySilence(durationSeconds);
                    break;
                case StatusEffectType.Blind:
                    ApplyBlind(durationSeconds);
                    break;
                case StatusEffectType.Freeze:
                    ApplyFreeze(durationSeconds);
                    break;
                case StatusEffectType.Petrify:
                    ApplyPetrify(durationSeconds);
                    break;
            }
        }

        private void Update()
        {
            if (!IsPoisoned || health == null)
            {
                poisonTickTimer = 0f;
                return;
            }

            poisonTickTimer += Time.deltaTime;

            if (poisonTickTimer < poisonTickIntervalSeconds)
            {
                return;
            }

            poisonTickTimer = 0f;
            health.TakeDamage(poisonDamagePerTick, Element.Neutral, DamageCategory.Magical);
        }

        private static float Extend(float currentExpireTime, float durationSeconds)
        {
            return Mathf.Max(currentExpireTime, Time.time + durationSeconds);
        }
    }
}
