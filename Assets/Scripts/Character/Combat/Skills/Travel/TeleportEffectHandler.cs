using System.Collections.Generic;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Teleport. Level 1 moves the caster to a random point of the current map; from level 2 the
    /// player may instead choose to return to the save point. SP is spent only once the caster has moved.
    /// </summary>
    public sealed class TeleportEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            var service = SkillTravel.Service;

            if (service == null)
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} cannot be used here.");
                return false;
            }

            var cost = skill.GetManaCost(level);

            if (context.Mana.CurrentMana < cost)
            {
                PlayerFeedbackChannel.Publish($"Not enough SP to cast {skill.SkillName}.");
                return false;
            }

            var points = context.Transform.root.GetComponentInChildren<PlayerTravelPoints>(true);
            var prompt = SkillTravel.Prompt;

            if (level < 2 || points == null || prompt == null)
            {
                return TeleportRandomly(context, service, cost);
            }

            prompt.Show($"{skill.SkillName}: where to?", BuildOptions(context, service, points, cost));
            return true;
        }

        private static IReadOnlyList<ChoiceOption> BuildOptions(SkillCastContext context, ISkillTravelService service, PlayerTravelPoints points, int cost)
        {
            var options = new List<ChoiceOption>
            {
                new ChoiceOption("Random spot on this map.", () => TeleportRandomly(context, service, cost))
            };

            var savePoint = points.GetSavePoint();

            if (savePoint.HasValue)
            {
                options.Add(new ChoiceOption(savePoint.Value.Label, () => TravelToSavePoint(context, service, savePoint.Value, cost)));
            }

            options.Add(new ChoiceOption("Cancel.", () => { }));
            return options;
        }

        private static bool TeleportRandomly(SkillCastContext context, ISkillTravelService service, int cost)
        {
            if (!service.TryTeleportRandomly())
            {
                PlayerFeedbackChannel.Publish("There is nowhere to teleport to.");
                return false;
            }

            context.Mana.TryConsumeMana(cost);
            return true;
        }

        private static void TravelToSavePoint(SkillCastContext context, ISkillTravelService service, WarpDestination destination, int cost)
        {
            if (!context.Mana.TryConsumeMana(cost))
            {
                PlayerFeedbackChannel.Publish("Not enough SP to teleport.");
                return;
            }

            service.TravelTo(destination);
        }
    }
}
