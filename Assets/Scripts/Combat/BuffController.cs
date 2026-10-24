using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Holds temporary, timed <see cref="BuffPayload"/> bonuses applied by
    /// buff/debuff skills (<see cref="Skills.SkillEffectType.Buff"/> — e.g.
    /// Provoke, Endure), on either the caster (a self-buff) or whoever the
    /// skill targets (a debuff). Works the same way for the player and for
    /// an enemy: <see cref="HealthComponent"/> reads
    /// <see cref="DefenseMultiplier"/> and <see cref="MagicalDefenseBonus"/>
    /// through its own optional-hook field, mirroring
    /// <see cref="IDefensiveStatsProvider"/>, and
    /// <see cref="Project.AI.EnemyController"/> exposes this the same way
    /// for <see cref="Project.AI.EnemyAttackState"/> to read
    /// <see cref="AttackMultiplier"/> from. Expired entries are purged
    /// lazily on the next read or <see cref="ApplyBuff"/> call, rather than
    /// through a per-frame Update() tick. Also holds persistent modifiers
    /// (see <see cref="SetPersistentModifier"/>) for a condition-triggered
    /// effect like Berserk, which stays active for as long as its trigger
    /// holds true rather than for a fixed duration.
    /// </summary>
    public class BuffController : MonoBehaviour, IActiveEffectSource
    {
        private readonly List<TimedBuff> activeBuffs = new List<TimedBuff>();
        private readonly Dictionary<object, BuffPayload> persistentModifiers = new Dictionary<object, BuffPayload>();
        private readonly Dictionary<object, EffectDescriptor> persistentDescriptors = new Dictionary<object, EffectDescriptor>();
        private System.Func<float> clock;

        private float Now => clock != null ? clock() : Time.time;

        /// <summary>
        /// Replaces the time source, so tests can advance time without waiting.
        /// Passing null restores <see cref="Time.time"/>.
        /// </summary>
        /// <param name="timeProvider">Returns the current time in seconds.</param>
        public void SetClock(System.Func<float> timeProvider)
        {
            clock = timeProvider;
        }

        /// <summary>Gets the combined payload from every active timed buff and persistent modifier.</summary>
        public BuffPayload Total
        {
            get
            {
                Purge();
                var total = new BuffPayload();

                foreach (var buff in activeBuffs)
                {
                    total += buff.Payload;
                }

                foreach (var modifier in persistentModifiers.Values)
                {
                    total += modifier;
                }

                return total;
            }
        }

        /// <summary>Gets the combined multiplier to apply to outgoing physical attack power (1 = no change).</summary>
        public float AttackMultiplier => 1f + Total.AtkPercent;

        /// <summary>Gets the combined multiplier to apply to incoming physical defense (1 = no change).</summary>
        public float DefenseMultiplier => 1f + Total.DefPercent;

        /// <summary>Gets the combined flat bonus to apply to magical defense.</summary>
        public int MagicalDefenseBonus => Total.MdefFlat;

        /// <summary>Gets the combined multiplier to apply to attack speed (1 = no change).</summary>
        public float AspdMultiplier => 1f + Total.AspdPercent;

        /// <summary>Gets the combined flat bonus to apply to Max HP.</summary>
        public int MaxHealthBonus => Total.MaxHealthFlat;

        /// <summary>Gets the combined multiplier to apply to incoming damage after defense mitigation, e.g. 0.7 for a 30% reduction (1 = no change) — e.g. Energy Coat. Clamped so a reduction over 100% never inverts into damage amplification.</summary>
        public float IncomingDamageMultiplier => Mathf.Clamp01(1f - Total.IncomingDamageReductionPercent);

        /// <summary>Gets whether any active buff stops the holder from flinching when hit (e.g. Endure).</summary>
        public bool PreventsFlinch => Total.PreventsFlinch;

        /// <summary>Gets the combined extra Fire damage fraction added to every attack (0 = none) — e.g. Magnum Break's after-effect.</summary>
        public float FireDamagePercent => Total.FireDamagePercent;

        /// <summary>
        /// Applies a new timed buff, stacking with any others already
        /// active — e.g. a Provoke debuff on an enemy and a self Endure
        /// buff on the player coexist fine, since they live on different
        /// characters. This overload is anonymous: it always adds a new
        /// entry and is not shown in the active effects bar. Use the overload
        /// with an <see cref="EffectDescriptor"/> for a buff that must refresh.
        /// </summary>
        /// <param name="payload">The stat bonuses this buff grants.</param>
        /// <param name="durationSeconds">How long this buff lasts, in seconds.</param>
        public void ApplyBuff(BuffPayload payload, float durationSeconds)
        {
            ApplyBuff(payload, durationSeconds, default);
        }

        /// <summary>
        /// Applies a timed buff that is shown in the active effects bar. When a
        /// buff with the same <see cref="EffectDescriptor.Id"/> is already active
        /// it is refreshed (new payload, full duration) instead of stacked, so
        /// re-casting a buff never doubles its bonus.
        /// </summary>
        /// <param name="payload">The stat bonuses this buff grants.</param>
        /// <param name="durationSeconds">How long this buff lasts, in seconds.</param>
        /// <param name="descriptor">How the buff is presented. An invalid descriptor makes it anonymous: it works but is not shown and never refreshes.</param>
        public void ApplyBuff(BuffPayload payload, float durationSeconds, EffectDescriptor descriptor)
        {
            Purge();
            var buff = new TimedBuff(payload, Now + durationSeconds, durationSeconds, descriptor);

            if (descriptor.IsValid)
            {
                var index = activeBuffs.FindIndex(existing => existing.Descriptor.Id == descriptor.Id);

                if (index >= 0)
                {
                    activeBuffs[index] = buff;
                    return;
                }
            }

            activeBuffs.Add(buff);
        }

        /// <summary>
        /// Sets (or clears, if <paramref name="payload"/> is empty) a
        /// persistent modifier identified by <paramref name="source"/>,
        /// replacing any previous modifier from that same source instead
        /// of stacking. Intended for a condition-triggered effect (e.g.
        /// Berserk's HP-threshold controller) that re-evaluates on events
        /// like <see cref="HealthComponent.HealthChanged"/> and would
        /// otherwise add a new <see cref="ApplyBuff"/> entry every time it
        /// fires. Unlike a timed buff, a persistent modifier never expires
        /// on its own — the source is responsible for clearing it once its
        /// condition ends.
        /// </summary>
        /// <param name="source">Identifies which caller owns this modifier, so repeated calls from the same source replace rather than stack.</param>
        /// <param name="payload">The stat bonuses this modifier grants. An empty payload clears the modifier.</param>
        public void SetPersistentModifier(object source, BuffPayload payload)
        {
            SetPersistentModifier(source, payload, default);
        }

        /// <summary>
        /// Sets (or clears) a persistent modifier that is shown in the active
        /// effects bar without a timer — e.g. Energy Coat or Berserk.
        /// </summary>
        /// <param name="source">Identifies which caller owns this modifier.</param>
        /// <param name="payload">The stat bonuses this modifier grants. An empty payload clears the modifier.</param>
        /// <param name="descriptor">How the modifier is presented. An invalid descriptor keeps it out of the bar.</param>
        public void SetPersistentModifier(object source, BuffPayload payload, EffectDescriptor descriptor)
        {
            if (payload.IsEmpty)
            {
                persistentModifiers.Remove(source);
                persistentDescriptors.Remove(source);
                return;
            }

            persistentModifiers[source] = payload;
            persistentDescriptors[source] = descriptor;
        }

        /// <inheritdoc />
        public void CollectActiveEffects(List<ActiveEffect> into)
        {
            Purge();

            foreach (var buff in activeBuffs)
            {
                if (buff.Descriptor.IsValid)
                {
                    into.Add(new ActiveEffect(buff.Descriptor, buff.ExpireTime - Now, buff.DurationSeconds, BuffPayloadDescription.Describe(buff.Payload)));
                }
            }

            foreach (var entry in persistentModifiers)
            {
                if (persistentDescriptors.TryGetValue(entry.Key, out var descriptor) && descriptor.IsValid)
                {
                    into.Add(new ActiveEffect(descriptor, 0f, 0f, BuffPayloadDescription.Describe(entry.Value)));
                }
            }
        }

        private void Purge()
        {
            activeBuffs.RemoveAll(buff => buff.ExpireTime <= Now);
        }

        private readonly struct TimedBuff
        {
            public TimedBuff(BuffPayload payload, float expireTime, float durationSeconds, EffectDescriptor descriptor)
            {
                Payload = payload;
                ExpireTime = expireTime;
                DurationSeconds = durationSeconds;
                Descriptor = descriptor;
            }

            public BuffPayload Payload { get; }
            public float ExpireTime { get; }
            public float DurationSeconds { get; }
            public EffectDescriptor Descriptor { get; }
        }
    }
}
