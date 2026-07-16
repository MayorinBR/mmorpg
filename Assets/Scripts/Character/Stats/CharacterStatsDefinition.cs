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
        [SerializeField] private int attackPower = 10;
        [SerializeField] private int defense = 5;
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private int experienceReward = 10;

        /// <summary>Gets the maximum health points for this character type.</summary>
        public int MaxHealth => maxHealth;

        /// <summary>Gets the base attack power used in damage calculations.</summary>
        public int AttackPower => attackPower;

        /// <summary>Gets the base defense used to reduce incoming damage.</summary>
        public int Defense => defense;

        /// <summary>Gets the movement speed in units per second.</summary>
        public float MoveSpeed => moveSpeed;

        /// <summary>Gets the experience granted when this character (typically an enemy) is defeated.</summary>
        public int ExperienceReward => experienceReward;
    }
}