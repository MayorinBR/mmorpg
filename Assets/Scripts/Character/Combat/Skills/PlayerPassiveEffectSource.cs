using System.Collections.Generic;
using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Reports every learned passive skill as a permanent effect, so the active
    /// effects bar can show them when the player chooses to see passives.
    /// </summary>
    public class PlayerPassiveEffectSource : MonoBehaviour, IActiveEffectSource
    {
        [Tooltip("Source of the learned skills. Resolved from the player hierarchy when left empty.")]
        [SerializeField] private PlayerSkillBook skillBook;

        private void Awake()
        {
            if (skillBook == null)
            {
                skillBook = transform.root.GetComponentInChildren<PlayerSkillBook>(true);
            }
        }

        /// <inheritdoc />
        public void CollectActiveEffects(List<ActiveEffect> into)
        {
            if (skillBook == null)
            {
                return;
            }

            foreach (var entry in skillBook.LearnedSkills)
            {
                var skill = entry.Key;

                if (skill.EffectType != SkillEffectType.Passive || entry.Value <= 0)
                {
                    continue;
                }

                var descriptor = new EffectDescriptor("passive." + skill.name, skill.SkillName, skill.Description, skill.Icon, EffectCategory.Passive);
                into.Add(new ActiveEffect(descriptor, 0f, 0f, $"Level {entry.Value}"));
            }
        }
    }
}
