using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Displacement skills, such as Back Slide, by pushing the caster away from the current target,
    /// or straight back along its facing when nothing is selected.
    /// </summary>
    public sealed class DisplacementEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (context.Agent == null || !context.Mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            var transform = context.Transform;
            var currentTarget = context.TargetSelector.CurrentTarget;
            var source = currentTarget != null ? currentTarget.position : transform.position - transform.forward;

            KnockbackUtility.Apply(context.Agent, source, skill.KnockbackDistance);
            return true;
        }
    }
}
