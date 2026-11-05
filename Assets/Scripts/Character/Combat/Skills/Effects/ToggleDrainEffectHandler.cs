using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves ToggleDrain skills, such as Energy Coat, by flipping the controller that drains SP while the
    /// skill is on. The mana cost is paid on and off; the ongoing drain is paid by the controller itself.
    /// </summary>
    public sealed class ToggleDrainEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            if (context.EnergyCoat == null || !context.Mana.TryConsumeMana(skill.ManaCost))
            {
                return false;
            }

            context.EnergyCoat.Toggle(skill, level);
            return true;
        }
    }
}
