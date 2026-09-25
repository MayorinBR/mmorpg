using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.AI
{
    /// <summary>
    /// Rolls a random level within this monster's CharacterStatsDefinition
    /// MinLevel-MaxLevel range on spawn and on every respawn (see
    /// RollNewLevel), then scales max health, attack power and experience
    /// reward proportionally to how far above the species' minimum level
    /// that roll landed. Implements IMaxHealthBonusProvider so
    /// HealthComponent picks up the health scaling through its existing
    /// optional hook, with no changes needed there.
    /// </summary>
    [RequireComponent(typeof(CharacterStatsHolder))]
    public class MonsterLevelController : MonoBehaviour, IMaxHealthBonusProvider
    {
        [Tooltip("Percentage bonus to health, attack power and experience reward per level above this species' MinLevel (e.g. 0.08 = +8% per level).")]
        [SerializeField, Range(0f, 1f)] private float statBonusPerLevel = 0.08f;

        [Tooltip("Optional. Updated with this instance's rolled level whenever it changes, so its name tag reads e.g. \"Lv.3 Poring\". Left empty, no tag is updated.")]
        [SerializeField] private EntityNameTagUI nameTag;

        private CharacterStatsHolder statsHolder;
        private HealthComponent health;

        private CharacterStatsHolder StatsHolder
        {
            get
            {
                if (statsHolder == null)
                {
                    statsHolder = GetComponent<CharacterStatsHolder>();
                }

                return statsHolder;
            }
        }

        private HealthComponent Health
        {
            get
            {
                if (health == null)
                {
                    health = GetComponent<HealthComponent>();
                }

                return health;
            }
        }

        private CharacterStatsDefinition Stats => StatsHolder.Stats;

        /// <summary>Gets the level rolled for this instance's current life.</summary>
        public int CurrentLevel { get; private set; }

        private float Multiplier => 1f + (CurrentLevel - Stats.MinLevel) * statBonusPerLevel;

        /// <summary>Gets this instance's attack power, scaled by <see cref="CurrentLevel"/>.</summary>
        public int ScaledAttackPower => Mathf.RoundToInt(Stats.AttackPower * Multiplier);

        /// <summary>Gets the experience reward for defeating this instance, scaled by <see cref="CurrentLevel"/>.</summary>
        public int ScaledExperienceReward => Mathf.RoundToInt(Stats.ExperienceReward * Multiplier);

        /// <summary>Gets the job experience reward for defeating this instance, scaled by <see cref="CurrentLevel"/>.</summary>
        public int ScaledJobExperienceReward => Mathf.RoundToInt(Stats.JobExperienceReward * Multiplier);

        private void Awake()
        {
            RollNewLevel();
        }

        /// <summary>
        /// Rolls a new level within the species' MinLevel-MaxLevel range
        /// (inclusive), pushes it to <see cref="nameTag"/> if wired, and
        /// refreshes <see cref="Health"/> so its max/current health reflect
        /// the freshly rolled level right away. The explicit refresh is what
        /// makes the very first roll (from <see cref="Awake"/>) correct
        /// regardless of whether this component's Awake runs before or
        /// after HealthComponent's own — sibling Awake order isn't
        /// guaranteed, so HealthComponent.Awake could otherwise compute its
        /// initial max health against this component's still-default,
        /// not-yet-rolled level.
        /// </summary>
        public void RollNewLevel()
        {
            CurrentLevel = Random.Range(Stats.MinLevel, Stats.MaxLevel + 1);
            nameTag?.SetLevel(CurrentLevel);
            Health?.ResetHealth();
        }

        /// <inheritdoc />
        public int GetMaxHealthBonus(int baseMaxHealth) => Mathf.RoundToInt(baseMaxHealth * (Multiplier - 1f));
    }
}
