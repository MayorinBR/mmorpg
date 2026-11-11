using System.Collections.Generic;
using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Warp Portal at a ground position the player picked. The player first chooses a destination
    /// among the save point and the memorized locations the skill level allows, and can memorize the current
    /// location from the same menu. SP and the catalyst are spent only once a destination is chosen.
    /// Not an <see cref="ISkillEffectHandler"/>, because it needs the picked position.
    /// </summary>
    public sealed class WarpPortalEffectHandler
    {
        /// <summary>
        /// Opens the destination menu for a portal at the picked position.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The Warp Portal skill being cast.</param>
        /// <param name="level">The skill's current level.</param>
        /// <param name="position">The world position the portal opens at.</param>
        /// <returns>True if the menu was shown.</returns>
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level, Vector3 position)
        {
            var points = context.Transform.root.GetComponentInChildren<PlayerTravelPoints>(true);

            if (SkillTravel.Service == null || SkillTravel.Prompt == null || points == null)
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} cannot be used here.");
                return false;
            }

            ShowMenu(context, skill, level, position, points);
            return true;
        }

        private static void ShowMenu(SkillCastContext context, SkillDefinition skill, int level, Vector3 position, PlayerTravelPoints points)
        {
            var options = new List<ChoiceOption>();

            foreach (var destination in points.GetDestinations(level))
            {
                var chosen = destination;
                options.Add(new ChoiceOption(chosen.Label, () => OpenPortal(context, skill, level, position, chosen)));
            }

            if (points.MemoCount < PlayerTravelPoints.MaxMemos)
            {
                options.Add(new ChoiceOption($"Memorize this location ({points.MemoCount}/{PlayerTravelPoints.MaxMemos}).", () => MemorizeAndReopen(context, skill, level, position, points)));
            }

            options.Add(new ChoiceOption("Cancel.", () => { }));
            var line = options.Count > 2 ? "Choose the portal's destination." : "No destination is available yet.";
            SkillTravel.Prompt.Show(line, options);
        }

        private static void MemorizeAndReopen(SkillCastContext context, SkillDefinition skill, int level, Vector3 position, PlayerTravelPoints points)
        {
            points.TryAddMemo(SkillTravel.Service.CurrentSceneName, context.Transform.position);
            ShowMenu(context, skill, level, position, points);
        }

        private static void OpenPortal(SkillCastContext context, SkillDefinition skill, int level, Vector3 position, WarpDestination destination)
        {
            if (!SkillRequirementChecker.MeetsResourceRequirement(context, skill) || !SkillRequirementChecker.MeetsCatalystRequirement(context, skill))
            {
                return;
            }

            context.Mana.TryConsumeMana(skill.GetManaCost(level));
            SkillRequirementChecker.SpendCatalyst(context, skill);

            var portal = SkillZoneController.Spawn(skill.ZonePrefab, position, skill.AreaRadius, skill.GetZoneDuration(level), skill.ZoneTickIntervalSeconds);
            SkillTravel.Service.ConfigurePortal(portal.gameObject, destination, skill.AreaRadius);
        }
    }
}
