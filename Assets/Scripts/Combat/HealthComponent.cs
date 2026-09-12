using System;
using UnityEngine;
using Project.Character.Stats;

namespace Project.Combat
{
    /// <summary>
    /// Tracks current health for a character, sourcing max health from a
    /// shared <see cref="CharacterStatsDefinition"/> so player and enemies
    /// use the same data-driven stat asset, optionally boosted by an
    /// <see cref="IMaxHealthBonusProvider"/> (see <see cref="maxHealthBonusSource"/>).
    /// Incoming damage is mitigated by physical or magical defense (see
    /// <see cref="IDefensiveStatsProvider"/> and <see cref="defensiveStatsSource"/>),
    /// sourced the same optional-hook way, falling back to
    /// <see cref="CharacterStatsDefinition"/>'s flat values for characters
    /// with no provider wired. A <see cref="BuffController"/> (see
    /// <see cref="buffs"/>), if wired, layers a further temporary
    /// multiplier/bonus on top of whichever physical/magical defense
    /// source above applies — e.g. Provoke's DEF debuff on an enemy, or
    /// Endure's MDEF buff on the player. Raises events on change and
    /// death so other systems (UI, AI, loot) can react without polling.
    /// </summary>
    [RequireComponent(typeof(CharacterStatsHolder))]
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private MonoBehaviour damageModifierSource;

        [Tooltip("Optional component adding a bonus on top of the base max health from Stats (e.g. the player's VIT). Left empty, enemies simply use their flat Stats value.")]
        [SerializeField] private MonoBehaviour maxHealthBonusSource;

        [Tooltip("Optional component supplying physical/magical defense and flee rating (e.g. the player's stats controller). Left empty, enemies fall back to their flat Stats values.")]
        [SerializeField] private MonoBehaviour defensiveStatsSource;

        [Tooltip("Optional. Source of temporary buff/debuff modifiers (see BuffController) affecting this character's defense — e.g. Provoke's DEF debuff on an enemy, Endure's MDEF buff on the player. Left empty, this character is never affected by one.")]
        [SerializeField] private BuffController buffs;

        private CharacterStatsHolder statsHolder;
        private int currentHealth;
        private IDamageModifier damageModifier;
        private IMaxHealthBonusProvider maxHealthBonusProvider;
        private IDefensiveStatsProvider defensiveStatsProvider;

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

        /// <summary>Raised whenever health changes, with (currentHealth, maxHealth).</summary>
        public event Action<int, int> HealthChanged;

        /// <summary>
        /// Raised whenever <see cref="TakeDamage"/> actually reduces health,
        /// with the mitigated amount that was applied and whether the hit
        /// was a critical hit. Purely a cosmetic notification (e.g.
        /// floating damage numbers) — nothing here depends on anyone
        /// listening to it.
        /// </summary>
        public event Action<int, bool> DamageTaken;

        /// <summary>
        /// Raised whenever <see cref="NotifyDodged"/> is called, i.e. an
        /// incoming attack missed. Purely a cosmetic notification (e.g. a
        /// floating "dodge" popup) — nothing here depends on anyone
        /// listening to it.
        /// </summary>
        public event Action Dodged;

        /// <summary>Raised once when health reaches zero.</summary>
        public event Action Died;

        /// <summary>
        /// Raised whenever <see cref="TakeDamage"/> actually reduces health
        /// and the caller identified itself via its <c>attacker</c>
        /// parameter. AI (see <see cref="Project.AI.EnemyController"/>)
        /// listens for this to target whoever just hit it immediately,
        /// without needing to detect them by proximity first.
        /// </summary>
        public event Action<Transform> AttackedBy;

        /// <summary>
        /// Gets the maximum health defined by the character's stats, plus
        /// any bonus from <see cref="maxHealthBonusSource"/> (e.g. the
        /// player's VIT-derived bonus).
        /// </summary>
        public int MaxHealth
        {
            get
            {
                var baseMaxHealth = StatsHolder.Stats.MaxHealth;
                return baseMaxHealth + (maxHealthBonusProvider?.GetMaxHealthBonus(baseMaxHealth) ?? 0);
            }
        }

        /// <summary>Gets the current health value.</summary>
        public int CurrentHealth => currentHealth;

        /// <inheritdoc />
        public bool IsDead => currentHealth <= 0;

        /// <inheritdoc />
        public int FleeRating => defensiveStatsProvider?.GetFleeRating() ?? StatsHolder.Stats.Flee;

        /// <inheritdoc />
        public MonsterSize Size => StatsHolder.Stats.Size;

        private int PhysicalDefense
        {
            get
            {
                var baseDefense = defensiveStatsProvider?.GetPhysicalDefense() ?? StatsHolder.Stats.Defense;
                return buffs != null ? Mathf.RoundToInt(baseDefense * buffs.DefenseMultiplier) : baseDefense;
            }
        }

        private int MagicalDefense
        {
            get
            {
                var baseDefense = defensiveStatsProvider?.GetMagicalDefense() ?? StatsHolder.Stats.MagicalDefense;
                return buffs != null ? baseDefense + buffs.MagicalDefenseBonus : baseDefense;
            }
        }

        private void Awake()
        {
            damageModifier = damageModifierSource as IDamageModifier;
            maxHealthBonusProvider = maxHealthBonusSource as IMaxHealthBonusProvider;
            defensiveStatsProvider = defensiveStatsSource as IDefensiveStatsProvider;
            currentHealth = MaxHealth;
        }

        /// <inheritdoc />
        public void TakeDamage(int amount, Element element = Element.Neutral, DamageCategory category = DamageCategory.Physical, bool isCritical = false, Transform attacker = null)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            if (damageModifier != null)
            {
                amount = damageModifier.ModifyIncomingDamage(amount, element);
            }

            if (amount <= 0)
            {
                return;
            }

            var defense = category == DamageCategory.Physical ? PhysicalDefense : MagicalDefense;
            amount = Mathf.Max(1, amount - defense);

            currentHealth = Mathf.Max(currentHealth - amount, 0);
            DamageTaken?.Invoke(amount, isCritical);
            HealthChanged?.Invoke(currentHealth, MaxHealth);

            if (attacker != null)
            {
                AttackedBy?.Invoke(attacker);
            }

            if (currentHealth == 0)
            {
                Died?.Invoke();
            }
        }

        /// <inheritdoc />
        public void NotifyDodged()
        {
            if (IsDead)
            {
                return;
            }

            Dodged?.Invoke();
        }

        /// <summary>
        /// Re-evaluates <see cref="MaxHealth"/> and raises
        /// <see cref="HealthChanged"/> with the current values, clamping
        /// <see cref="CurrentHealth"/> down if it now exceeds the new
        /// maximum. Call this whenever something feeding
        /// <see cref="maxHealthBonusSource"/> changes — a spent stat point,
        /// an equipment swap — since nothing here polls for that on its
        /// own: <see cref="MaxHealth"/> is a live computed value, but UI
        /// bound to <see cref="HealthChanged"/> only re-reads it when this
        /// event fires.
        /// </summary>
        public void RefreshMaxHealth()
        {
            currentHealth = Mathf.Min(currentHealth, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        /// <summary>
        /// Restores health to its maximum value, typically called when
        /// reviving an entity after death (e.g. enemy respawn).
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = MaxHealth;
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        /// <summary>
        /// Restores a partial amount of health, clamped to the maximum.
        /// Does nothing if already dead.
        /// </summary>
        /// <param name="amount">The amount of health to restore. Non-positive values are ignored.</param>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            currentHealth = Mathf.Min(currentHealth + amount, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }

        /// <summary>
        /// Directly sets the current health to a previously-known value,
        /// clamped to [0, MaxHealth], without raising <see cref="Died"/>.
        /// Intended for restoring captured state (e.g. enemy world-state
        /// restore across map switches) rather than as a combat action —
        /// normal gameplay should keep using <see cref="TakeDamage"/> and
        /// <see cref="Heal"/>.
        /// </summary>
        /// <param name="value">The health value to restore.</param>
        public void SetCurrentHealth(int value)
        {
            currentHealth = Mathf.Clamp(value, 0, MaxHealth);
            HealthChanged?.Invoke(currentHealth, MaxHealth);
        }
    }
}
