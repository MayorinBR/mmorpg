using UnityEngine;
using Project.Skills;
using Project.Combat;
using Project.Items;

namespace Project.Character.Combat
{
    /// <summary>
    /// Resolves Steal: rolls the skill's success chance and, on success, moves one item from the target's drop
    /// table into the caster's inventory. A target can be stolen from once until it respawns, and the item can
    /// still drop from it normally. Mana is spent whether or not the attempt succeeds.
    /// ponytail: an item is lost if the inventory fills between the weight check and the add; add a rollback if it ever happens.
    /// </summary>
    public sealed class StealEffectHandler : ISkillEffectHandler
    {
        /// <inheritdoc />
        public bool TryCast(SkillCastContext context, SkillDefinition skill, int level)
        {
            var target = context.TargetSelector.CurrentTarget;
            var stealTarget = target != null ? target.GetComponentInParent<IStealTarget>() : null;
            var inventory = context.Inventory;

            if (stealTarget == null || inventory == null)
            {
                CombatLog.Write($"{skill.SkillName}: not cast, the target has nothing to steal.");
                return false;
            }

            var distance = CombatRangeMath.HorizontalDistance(context.Transform.position, target.position);
            var range = context.GetEffectiveRange(skill);

            if (distance > range)
            {
                CombatLog.Write($"{skill.SkillName}: not cast, the target is out of range ({distance:0.#} m, {range:0.#} max).");
                return false;
            }

            if (inventory.Items.IsMinorOverweight || !context.Mana.TryConsumeMana(skill.GetManaCost(level)))
            {
                return false;
            }

            if (!stealTarget.CanBeStolenFrom)
            {
                PlayerFeedbackChannel.Publish("There is nothing left to steal.");
                return true;
            }

            if (Random.value >= skill.GetSuccessChance(level))
            {
                PlayerFeedbackChannel.Publish($"{skill.SkillName} failed.");
                return true;
            }

            var item = stealTarget.Steal();

            if (item == null || !inventory.Items.TryAddItem(item, 1))
            {
                PlayerFeedbackChannel.Publish("Nothing was stolen.");
                return true;
            }

            PlayerFeedbackChannel.Publish($"Stole {item.ItemName}.");
            context.Visuals.PlayCustomVfx(skill, target);
            return true;
        }
    }
}
