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
    public class BuffController : MonoBehaviour
    {
        private readonly List<TimedBuff> activeBuffs = new List<TimedBuff>();
        private readonly Dictionary<object, BuffPayload> persistentModifiers = new Dictionary<object, BuffPayload>();

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

        /// <summary>
        /// Applies a new timed buff, stacking with any others already
        /// active — e.g. a Provoke debuff on an enemy and a self Endure
        /// buff on the player coexist fine, since they live on different
        /// characters. Re-casting the same skill just adds another entry
        /// rather than refreshing one in place: with a real per-skill
        /// cooldown already preventing rapid re-casts, this is simpler than
        /// tracking one slot per source and not worth the extra bookkeeping
        /// today.
        /// </summary>
        /// <param name="payload">The stat bonuses this buff grants.</param>
        /// <param name="durationSeconds">How long this buff lasts, in seconds.</param>
        public void ApplyBuff(BuffPayload payload, float durationSeconds)
        {
            Purge();
            activeBuffs.Add(new TimedBuff(payload, Time.time + durationSeconds));
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
            if (payload.IsEmpty)
            {
                persistentModifiers.Remove(source);
                return;
            }

            persistentModifiers[source] = payload;
        }

        private void Purge()
        {
            activeBuffs.RemoveAll(buff => buff.ExpireTime <= Time.time);
        }

        private readonly struct TimedBuff
        {
            public TimedBuff(BuffPayload payload, float expireTime)
            {
                Payload = payload;
                ExpireTime = expireTime;
            }

            public BuffPayload Payload { get; }
            public float ExpireTime { get; }
        }
    }
}
