using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Checks and spends what a skill needs besides its mana: weapon, ammo, catalyst item, Zeny and crafting
    /// materials. Tells the player what is missing when a check fails.
    /// </summary>
    public static class SkillRequirementChecker
    {
        /// <summary>
        /// Checks the weapon and ammo a skill needs before any resource is spent.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill about to be cast.</param>
        /// <returns>True when the skill needs nothing the caster lacks.</returns>
        public static bool MeetsWeaponRequirement(SkillCastContext context, SkillDefinition skill)
        {
            if (!skill.IsWeaponRequirementMet(context.GetMainHandWeaponSubtype()))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} requires a {string.Join(" or ", skill.RequiredWeaponSubtypes)}.");
                return false;
            }

            if (skill.AmmoCost > 0 && (context.Equipment == null || context.Equipment.EquippedAmmoCount < skill.AmmoCost))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} needs ammo equipped.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks that the inventory holds the item the skill consumes, such as a gemstone.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill about to be cast.</param>
        /// <returns>True when the skill needs no catalyst or the inventory holds enough of it.</returns>
        public static bool MeetsCatalystRequirement(SkillCastContext context, SkillDefinition skill)
        {
            if (skill.RequiredItem == null)
            {
                return true;
            }

            if (context.Inventory == null || context.Inventory.Items.GetQuantity(skill.RequiredItem) < skill.RequiredItemCount)
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} requires {skill.RequiredItemCount} {skill.RequiredItem.ItemName}.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks the mana, Zeny and crafting materials a skill needs.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill about to be cast.</param>
        /// <returns>True when every resource the skill consumes is available.</returns>
        public static bool MeetsResourceRequirement(SkillCastContext context, SkillDefinition skill)
        {
            if (context.Mana.CurrentMana < skill.GetManaCost(context.SkillBook.GetLevel(skill)))
            {
                PlayerFeedbackChannel.Publish($"Not enough SP to cast {skill.SkillName}.");
                return false;
            }

            if (skill.ZenyCost > 0 && (context.Currency == null || context.Currency.CurrentZeny < skill.ZenyCost))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} requires {skill.ZenyCost} Zeny.");
                return false;
            }

            return skill.EffectType != SkillEffectType.Craft || MeetsCraftMaterials(context, skill);
        }

        /// <summary>
        /// Removes the catalyst item the skill consumes.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill that was cast.</param>
        public static void SpendCatalyst(SkillCastContext context, SkillDefinition skill)
        {
            if (skill.RequiredItem != null)
            {
                context.Inventory.Items.TryRemoveItem(skill.RequiredItem, skill.RequiredItemCount);
            }
        }

        /// <summary>
        /// Consumes the ammo the skill fires.
        /// </summary>
        /// <param name="context">The caster's collaborators.</param>
        /// <param name="skill">The skill that was cast.</param>
        public static void SpendAmmo(SkillCastContext context, SkillDefinition skill)
        {
            for (var i = 0; i < skill.AmmoCost; i++)
            {
                context.Equipment.TryConsumeAmmo();
            }
        }

        private static bool MeetsCraftMaterials(SkillCastContext context, SkillDefinition skill)
        {
            var materials = skill.CraftingRecipe != null ? skill.CraftingRecipe.Materials : null;

            if (context.Inventory == null || materials == null)
            {
                return true;
            }

            foreach (var material in materials)
            {
                if (context.Inventory.Items.GetQuantity(material.Item) < material.Quantity)
                {
                    PlayerFeedbackChannel.Publish($"{skill.SkillName} requires {material.Quantity} {material.Item.ItemName}.");
                    return false;
                }
            }

            return true;
        }
    }
}
