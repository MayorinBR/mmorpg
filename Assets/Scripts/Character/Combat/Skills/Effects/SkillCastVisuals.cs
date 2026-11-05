using UnityEngine;
using Project.Skills;
using Project.Combat;

namespace Project.Character.Combat
{
    /// <summary>
    /// Plays the visuals of a cast skill: the weapon glow, the projectile or bolt, and the impact effect.
    /// Purely cosmetic; the outcome of a skill is decided before anything here runs.
    /// </summary>
    public sealed class SkillCastVisuals
    {
        private readonly GameObject host;
        private readonly System.Func<PlayerCombatController> combatProvider;
        private SkillWeaponVfxPlayer weaponVfx;

        /// <summary>Initializes the visual player.</summary>
        /// <param name="host">The caster object; the weapon visual player is added to it on first use.</param>
        /// <param name="combatProvider">Returns the combat controller that spawns projectiles, or null.</param>
        public SkillCastVisuals(GameObject host, System.Func<PlayerCombatController> combatProvider)
        {
            this.host = host;
            this.combatProvider = combatProvider;
        }

        /// <summary>
        /// Plays the skill's weapon glow and burst visuals, adding the weapon visual player on first use.
        /// </summary>
        /// <param name="skill">The skill that was just cast.</param>
        public void PlayWeaponVfx(SkillDefinition skill)
        {
            if (skill.WeaponGlowVfxPrefab == null)
            {
                return;
            }

            if (weaponVfx == null && !host.TryGetComponent(out weaponVfx))
            {
                weaponVfx = host.AddComponent<SkillWeaponVfxPlayer>();
            }

            weaponVfx.Play(skill);
        }

        /// <summary>
        /// Fires the arrow (ammo skills) or bolt (magical skills) visual at the target through the basic
        /// attack's projectile setup, or plays the skill's own effects when it has them.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="target">The target the projectile flies to.</param>
        public void PlayProjectileVisual(SkillDefinition skill, Transform target)
        {
            if (skill.HasCustomVfx)
            {
                PlayCustomVfx(skill, target);
                return;
            }

            var isMagical = skill.DamageType == SkillDamageType.Magical;

            if (skill.AmmoCost <= 0 && !isMagical)
            {
                return;
            }

            var combat = combatProvider();

            if (isMagical)
            {
                combat?.SpawnSkillProjectile(true, target);
            }
            else
            {
                combat?.SpawnSkillArrows(skill, target);
            }
        }

        /// <summary>
        /// Plays the skill's own effects: its projectile flies to the target and the impact effect spawns on
        /// arrival, or at once when the skill has no projectile.
        /// </summary>
        /// <param name="skill">The skill being cast.</param>
        /// <param name="target">The target the effects are played on.</param>
        public void PlayCustomVfx(SkillDefinition skill, Transform target)
        {
            var position = target.position;

            void SpawnImpact()
            {
                var instance = SkillVfx.Spawn(skill.ImpactVfxPrefab, position);
                AlignToTargetHead(skill, instance, target);
            }

            if (skill.ProjectileVfxPrefab == null)
            {
                SpawnImpact();
                return;
            }

            var combat = combatProvider();

            if (combat == null)
            {
                SpawnImpact();
                return;
            }

            combat.SpawnSkillProjectile(skill.ProjectileVfxPrefab, target, SpawnImpact);
        }

        /// <summary>
        /// Shifts a spawned impact effect so the point it was authored at sits just above the top of the
        /// target's model. Does nothing for a skill without an authored height or a target without a mesh.
        /// </summary>
        /// <param name="skill">The skill whose effect was spawned.</param>
        /// <param name="instance">The spawned effect, or null.</param>
        /// <param name="target">The target the effect belongs to.</param>
        private static void AlignToTargetHead(SkillDefinition skill, GameObject instance, Transform target)
        {
            if (instance == null || target == null || skill.ImpactVfxAuthoredHeight <= 0f)
            {
                return;
            }

            var hasMesh = false;
            var top = float.NegativeInfinity;

            foreach (var meshRenderer in target.GetComponentsInChildren<Renderer>())
            {
                if (meshRenderer is ParticleSystemRenderer || meshRenderer is TrailRenderer || meshRenderer is LineRenderer)
                {
                    continue;
                }

                hasMesh = true;
                top = Mathf.Max(top, meshRenderer.bounds.max.y);
            }

            if (!hasMesh)
            {
                return;
            }

            var shift = HeadMarkerMath.ComputeShift(top, instance.transform.position.y, skill.ImpactVfxAuthoredHeight);
            instance.transform.position += Vector3.up * shift;
        }
    }
}
