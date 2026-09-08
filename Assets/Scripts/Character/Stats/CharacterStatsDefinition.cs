using UnityEngine;

namespace Project.Character.Stats
{
    /// <summary>
    /// Defines the base combat statistics for a character or enemy type.
    /// Instances are authored as assets and referenced by runtime entities,
    /// allowing both client and server to read identical data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterStats", menuName = "Project/Character/Stats")]
    public class CharacterStatsDefinition : ScriptableObject
    {
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int maxMana = 50;
        [SerializeField] private int attackPower = 10;
        [SerializeField] private int defense = 5;
        [SerializeField] private int magicalDefense = 0;
        [SerializeField] private int hit = 100;
        [SerializeField] private int flee = 20;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private int experienceReward = 10;
        [SerializeField] private int jobExperienceReward = 5;

        /// <summary>Gets the maximum health points for this character type.</summary>
        public int MaxHealth => maxHealth;

        /// <summary>Gets the maximum mana (SP) points for this character type.</summary>
        public int MaxMana => maxMana;

        /// <summary>Gets the base attack power used in damage calculations.</summary>
        public int AttackPower => attackPower;

        /// <summary>Gets the base physical defense used to reduce incoming <see cref="Project.Combat.DamageCategory.Physical"/> damage.</summary>
        public int Defense => defense;

        /// <summary>Gets the base magical defense used to reduce incoming <see cref="Project.Combat.DamageCategory.Magical"/> damage.</summary>
        public int MagicalDefense => magicalDefense;

        /// <summary>
        /// Gets the base accuracy rating used to resolve this character's hit
        /// chance against a target. Defaults high enough (100) that an
        /// enemy with no dedicated tuning still reliably hits a player with
        /// an average Flee, consistent with early-game Ragnarok Online mobs.
        /// </summary>
        public int Hit => hit;

        /// <summary>Gets the base dodge rating used to resolve an attacker's hit chance against this character.</summary>
        public int Flee => flee;

        /// <summary>Gets the movement speed in units per second.</summary>
        public float MoveSpeed => moveSpeed;

        /// <summary>Gets the experience granted when this character (typically an enemy) is defeated.</summary>
        public int ExperienceReward => experienceReward;

        /// <summary>Gets the job experience granted when this character (typically an enemy) is defeated.</summary>
        public int JobExperienceReward => jobExperienceReward;
    }
}