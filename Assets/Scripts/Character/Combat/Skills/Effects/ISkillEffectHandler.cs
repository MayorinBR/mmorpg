using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves the effect of one kind of skill (damage, heal, buff and so on) once its cast is committed.
    /// Adding a new kind of effect means adding a handler and registering it, without editing the caster.
    /// </summary>
    public interface ISkillEffectHandler
    {
        /// <summary>
        /// Spends the skill's cost and applies its effect.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <returns>True if the cast was committed and the skill should go on cooldown.</returns>
        bool TryCast(SkillCastContext context, SkillDefinition skill, int level);
    }
}
