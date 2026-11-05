using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Toggle skills by flipping the caster's Hidden status, so recasting Hiding turns it off.
    /// ponytail: hardcoded to Hiding, the only Toggle skill today; add a per-skill flag field when a second
    /// toggle status appears.
    /// </summary>
    public sealed class ToggleEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (context.StatusEffects == null || !context.Mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            context.StatusEffects.SetHidden(!context.StatusEffects.IsHidden);
            return true;
        }
    }
}
