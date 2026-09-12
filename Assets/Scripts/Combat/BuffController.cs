using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Holds temporary, timed stat modifiers applied by buff/debuff skills
    /// (<see cref="Skills.SkillEffectType.Buff"/> — e.g. Provoke, Endure),
    /// on either the caster (a self-buff) or whoever the skill targets (a
    /// debuff). Works the same way for the player and for an enemy:
    /// <see cref="HealthComponent"/> reads <see cref="DefenseMultiplier"/>
    /// and <see cref="MagicalDefenseBonus"/> through its own optional-hook
    /// field, mirroring <see cref="IDefensiveStatsProvider"/>, and
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
        private readonly List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
        private readonly Dictionary<object, PersistentModifier> persistentModifiers = new Dictionary<object, PersistentModifier>();

        /// <summary>Gets the combined multiplier to apply to outgoing physical attack power (1 = no change).</summary>
        public float AttackMultiplier => 1f + Sum(buff => buff.AtkPercent) + SumPersistent(m => m.AtkPercent);

        /// <summary>Gets the combined multiplier to apply to incoming physical defense (1 = no change).</summary>
        public float DefenseMultiplier => 1f + Sum(buff => buff.DefPercent) + SumPersistent(m => m.DefPercent);

        /// <summary>Gets the combined flat bonus to apply to magical defense.</summary>
        public int MagicalDefenseBonus => Mathf.RoundToInt(Sum(buff => buff.MdefFlat) + SumPersistent(m => m.MdefFlat));

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
        /// <param name="atkPercent">Attack power multiplier bonus, e.g. 0.32 for +32%. Zero for a buff without an ATK component.</param>
        /// <param name="defPercent">Physical defense multiplier bonus, e.g. -0.55 for -55%. Zero for a buff without a DEF component.</param>
        /// <param name="mdefFlat">Flat magical defense bonus. Zero for a buff without an MDEF component.</param>
        /// <param name="durationSeconds">How long this buff lasts, in seconds.</param>
        public void ApplyBuff(float atkPercent, float defPercent, int mdefFlat, float durationSeconds)
        {
            Purge();
            activeBuffs.Add(new ActiveBuff(atkPercent, defPercent, mdefFlat, Time.time + durationSeconds));
        }

        /// <summary>
        /// Sets (or clears, if every value is zero) a persistent modifier
        /// identified by <paramref name="source"/>, replacing any previous
        /// modifier from that same source instead of stacking. Intended
        /// for a condition-triggered effect (e.g. Berserk's HP-threshold
        /// controller) that re-evaluates on events like
        /// <see cref="HealthComponent.HealthChanged"/> and would otherwise
        /// add a new <see cref="ApplyBuff"/> entry every time it fires.
        /// Unlike a timed buff, a persistent modifier never expires on its
        /// own — the source is responsible for clearing it once its
        /// condition ends.
        /// </summary>
        /// <param name="source">Identifies which caller owns this modifier, so repeated calls from the same source replace rather than stack.</param>
        /// <param name="atkPercent">Attack power multiplier bonus. Zero contributes nothing.</param>
        /// <param name="defPercent">Physical defense multiplier bonus. Zero contributes nothing.</param>
        /// <param name="mdefFlat">Flat magical defense bonus. Zero contributes nothing.</param>
        public void SetPersistentModifier(object source, float atkPercent, float defPercent, int mdefFlat)
        {
            if (atkPercent == 0f && defPercent == 0f && mdefFlat == 0)
            {
                persistentModifiers.Remove(source);
                return;
            }

            persistentModifiers[source] = new PersistentModifier(atkPercent, defPercent, mdefFlat);
        }

        private float Sum(Func<ActiveBuff, float> selector)
        {
            Purge();
            var total = 0f;

            foreach (var buff in activeBuffs)
            {
                total += selector(buff);
            }

            return total;
        }

        private float SumPersistent(Func<PersistentModifier, float> selector)
        {
            var total = 0f;

            foreach (var modifier in persistentModifiers.Values)
            {
                total += selector(modifier);
            }

            return total;
        }

        private void Purge()
        {
            activeBuffs.RemoveAll(buff => buff.ExpireTime <= Time.time);
        }

        private readonly struct ActiveBuff
        {
            public ActiveBuff(float atkPercent, float defPercent, int mdefFlat, float expireTime)
            {
                AtkPercent = atkPercent;
                DefPercent = defPercent;
                MdefFlat = mdefFlat;
                ExpireTime = expireTime;
            }

            public float AtkPercent { get; }
            public float DefPercent { get; }
            public int MdefFlat { get; }
            public float ExpireTime { get; }
        }

        private readonly struct PersistentModifier
        {
            public PersistentModifier(float atkPercent, float defPercent, int mdefFlat)
            {
                AtkPercent = atkPercent;
                DefPercent = defPercent;
                MdefFlat = mdefFlat;
            }

            public float AtkPercent { get; }
            public float DefPercent { get; }
            public int MdefFlat { get; }
        }
    }
}
