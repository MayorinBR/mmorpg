using System.Collections.Generic;

namespace Project.Combat
{
    /// <summary>
    /// Implemented by any component that can report effects currently
    /// affecting a character, so the active effects bar can show them without
    /// knowing where they come from.
    /// </summary>
    public interface IActiveEffectSource
    {
        /// <summary>
        /// Adds a snapshot of every effect this source currently applies.
        /// </summary>
        /// <param name="into">The list to append to. Implementations must not clear it.</param>
        void CollectActiveEffects(List<ActiveEffect> into);
    }
}
