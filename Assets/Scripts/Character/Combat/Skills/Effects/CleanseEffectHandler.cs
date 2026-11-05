using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Cleanse skills, such as Cure and Detoxify, by clearing every active debuff on the caster.
    /// Ally targeting resolves to the caster until ally selection exists.
    /// </summary>
    public sealed class CleanseEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (context.StatusEffects == null || !context.Mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            context.StatusEffects.ClearAllDebuffs();
            return true;
        }
    }
}
