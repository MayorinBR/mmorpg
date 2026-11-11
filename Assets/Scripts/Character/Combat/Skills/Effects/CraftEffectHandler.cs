using UnityEngine;
using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Craft skills, such as Arrow Crafting and Aqua Benedicta, by crafting the skill's recipe from
    /// the caster's inventory. A skill that requires water needs a water collider near the caster. Mana is
    /// spent even when the craft itself fails, like every other skill's cost.
    /// ponytail: no map tags anything as Water yet, so the water check is ready but unexercised.
    /// </summary>
    public sealed class CraftEffectHandler : ISkillEffectHandler
    {
        private const float NearWaterRadius = 3f;

        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            var inventory = context.Inventory;

            if (inventory == null || inventory.Items.IsMinorOverweight || !context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                return false;
            }

            if (skill.RequiresNearWater && !Physics.CheckSphere(context.Transform.position, NearWaterRadius, context.WaterLayer))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} must be cast near water.");
                return true;
            }

            if (!inventory.Items.TryCraft(skill.CraftingRecipe))
            {
                PlayerFeedbackChannel.Publish("Not enough inventory space.");
            }

            return true;
        }
    }
}
