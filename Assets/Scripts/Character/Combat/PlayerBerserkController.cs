using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Drives Berserk, a conditional passive buff that is never cast: it
    /// stays active for as long as the player's HP is at or below
    /// <see cref="HpThresholdRatio"/> (Ragnarok Online's own 25% trigger),
    /// and turns off the moment HP rises above it or the character dies.
    /// Re-evaluates on every <see cref="HealthComponent.HealthChanged"/>
    /// and <see cref="PlayerSkillBook.SkillLeveledUp"/>, applying the
    /// result through <see cref="BuffController.SetPersistentModifier"/>
    /// so repeated HP changes replace rather than stack the modifier. This
    /// project has no Job Level unlock gate anywhere yet, so Berserk's real
    /// Job Level 30 requirement isn't modeled — learning the skill is the
    /// only requirement.
    /// </summary>
    public class PlayerBerserkController : MonoBehaviour
    {
        private const float HpThresholdRatio = 0.25f;

        [Tooltip("The player's health, whose HealthChanged event drives re-evaluation.")]
        [SerializeField] private HealthComponent health;

        [Tooltip("Source of the player's learned skills and levels, to read Berserk's level and to re-evaluate when it's learned or upgraded.")]
        [SerializeField] private PlayerSkillBook skillBook;

        [Tooltip("Target of the ATK/DEF modifier while Berserk is active.")]
        [SerializeField] private BuffController buffs;

        [Tooltip("The Berserk skill asset, used to look up its learned level and its per-level ATK/DEF values.")]
        [SerializeField] private SkillDefinition berserkSkill;

        private void OnEnable()
        {
            health.HealthChanged += HandleHealthChanged;
            skillBook.SkillLeveledUp += HandleSkillLeveledUp;
            Evaluate();
        }

        private void OnDisable()
        {
            health.HealthChanged -= HandleHealthChanged;
            skillBook.SkillLeveledUp -= HandleSkillLeveledUp;
        }

        private void HandleHealthChanged(int currentHealth, int maxHealth)
        {
            Evaluate();
        }

        private void HandleSkillLeveledUp(SkillDefinition skill, int level)
        {
            if (skill == berserkSkill)
            {
                Evaluate();
            }
        }

        private void Evaluate()
        {
            var level = skillBook.GetLevel(berserkSkill);
            var active = level > 0 && !health.IsDead && health.CurrentHealth <= health.MaxHealth * HpThresholdRatio;

            buffs.SetPersistentModifier(
                this,
                active ? berserkSkill.GetBuffAtkPercent(level) : 0f,
                active ? berserkSkill.GetBuffDefPercent(level) : 0f,
                0);
        }
    }
}
