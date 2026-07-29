using UnityEngine;
using Project.Character.Stats;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Gives Swordman a chance to block incoming attacks, reducing or fully
    /// negating the damage. Plugs into <see cref="HealthComponent"/> via
    /// <see cref="IDamageModifier"/> rather than intercepting damage
    /// directly, so it stays optional and doesn't affect other classes or
    /// enemies that don't reference it.
    /// </summary>
    public class PlayerBlockController : MonoBehaviour, IDamageModifier
    {
        [SerializeField] private PlayerClassController classController;
        [SerializeField, Range(0f, 1f)] private float blockChance = 0.25f;
        [SerializeField, Range(0f, 1f)] private float blockDamageReduction = 1f;

        /// <inheritdoc />
        public int ModifyIncomingDamage(int amount)
        {
            if (classController.CurrentClass != CharacterClass.Swordman)
            {
                return amount;
            }

            if (Random.value >= blockChance)
            {
                return amount;
            }

            return Mathf.RoundToInt(amount * (1f - blockDamageReduction));
        }
    }
}