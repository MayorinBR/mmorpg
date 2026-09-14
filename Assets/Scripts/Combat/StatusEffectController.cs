using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Holds a character's active status effects. Currently limited to a
    /// single stun timer — the only status effect any skill in this
    /// project produces today (see Fatal Blow) — rather than a general
    /// per-status collection, since nothing else needs one yet. Works the
    /// same way for the player and for an enemy, mirroring how
    /// <see cref="BuffController"/> is wired as an optional hook on both.
    /// </summary>
    public class StatusEffectController : MonoBehaviour
    {
        private float stunExpireTime;

        /// <summary>Gets whether this character is currently stunned.</summary>
        public bool IsStunned => Time.time < stunExpireTime;

        /// <summary>
        /// Applies a stun lasting the given duration. Extends the current
        /// stun rather than shortening it if one is already active and
        /// would outlast this one.
        /// </summary>
        /// <param name="durationSeconds">How long the stun should last, in seconds.</param>
        public void ApplyStun(float durationSeconds)
        {
            stunExpireTime = Mathf.Max(stunExpireTime, Time.time + durationSeconds);
        }
    }
}
