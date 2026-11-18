using Project.Combat;
using Project.Items;
using Project.Skills;
using UnityEngine;

namespace Project.Character.Combat
{
    /// <summary>
    /// Spawns the arrow and bolt visuals that accompany a basic attack or a skill cast. Purely cosmetic: the outcome of
    /// every hit is decided by the caller before a projectile is launched.
    /// </summary>
    public sealed class SkillProjectileLauncher
    {
        private const float ParallelArrowSpacing = 0.12f;
        private const int RainArrowCount = 8;
        private const float RainRadius = 1.2f;
        private const float RainHeight = 5f;
        private const float RainStagger = 0.8f;
        private const float RainSpeedMultiplier = 1.5f;

        private readonly Transform owner;
        private readonly Transform projectileOrigin;
        private readonly float projectileSpeed;
        private readonly GameObject arrowPrefab;
        private readonly GameObject boltPrefab;
        private readonly EquipmentManager equipment;

        /// <summary>Initializes a new instance of the <see cref="SkillProjectileLauncher"/> class.</summary>
        /// <param name="owner">The shooter; its position is the fallback origin.</param>
        /// <param name="projectileOrigin">The socket projectiles leave from, or null to use the owner's position.</param>
        /// <param name="projectileSpeed">The travel speed in meters per second.</param>
        /// <param name="arrowPrefab">The shared arrow visual, used when the equipped ammo has no model of its own.</param>
        /// <param name="boltPrefab">The Mage bolt visual.</param>
        /// <param name="equipment">The equipment manager that holds the equipped ammo.</param>
        public SkillProjectileLauncher(Transform owner, Transform projectileOrigin, float projectileSpeed, GameObject arrowPrefab, GameObject boltPrefab, EquipmentManager equipment)
        {
            this.owner = owner;
            this.projectileOrigin = projectileOrigin;
            this.projectileSpeed = projectileSpeed;
            this.arrowPrefab = arrowPrefab;
            this.boltPrefab = boltPrefab;
            this.equipment = equipment;
        }

        private Vector3 Origin => projectileOrigin != null ? projectileOrigin.position : owner.position;

        /// <summary>
        /// Picks the arrow visual for the next shot: the model authored on the equipped ammo item when it has one,
        /// otherwise the shared arrow prefab.
        /// </summary>
        /// <returns>The projectile prefab to spawn, or null when neither is available.</returns>
        public GameObject ResolveArrowPrefab()
        {
            var ammo = equipment.EquippedAmmoItem;
            var ammoModel = ammo != null ? ammo.GetEquippedModel(default) : null;
            return ammoModel != null ? ammoModel : arrowPrefab;
        }

        /// <summary>Fires the arrow or bolt visual at a target for a skill cast.</summary>
        /// <param name="magical">True for the Mage's bolt, false for the Archer's arrow.</param>
        /// <param name="target">The transform the projectile flies to.</param>
        public void SpawnProjectile(bool magical, Transform target)
        {
            var prefab = magical ? boltPrefab : ResolveArrowPrefab();

            if (prefab == null || target == null)
            {
                return;
            }

            Projectile.Spawn(prefab, Origin, target, projectileSpeed, null);
        }

        /// <summary>Fires a skill's own projectile effect at a target and runs a callback when it arrives.</summary>
        /// <param name="prefab">The skill's projectile effect.</param>
        /// <param name="target">The transform the projectile flies to.</param>
        /// <param name="onArrival">Invoked when the projectile reaches the target.</param>
        public void SpawnProjectile(GameObject prefab, Transform target, System.Action onArrival)
        {
            Projectile.Spawn(prefab, Origin, target, projectileSpeed, onArrival);
        }

        /// <summary>
        /// Draws the arrows of an ammo skill in the style the skill asks for (see <see cref="ArrowVolleyStyle"/>),
        /// attaching the skill's aura to each one.
        /// </summary>
        /// <param name="skill">The ammo skill being cast.</param>
        /// <param name="target">The transform the arrows fly to.</param>
        public void SpawnArrowVolley(SkillDefinition skill, Transform target)
        {
            var prefab = ResolveArrowPrefab();

            if (prefab == null || target == null)
            {
                return;
            }

            var origin = Origin;

            switch (skill.ArrowVolleyStyle)
            {
                case ArrowVolleyStyle.Parallel:
                    var side = Vector3.Cross(Vector3.up, target.position - origin).normalized * ParallelArrowSpacing;
                    AttachAura(skill, Projectile.Spawn(prefab, origin + side, target, projectileSpeed, null, side));
                    AttachAura(skill, Projectile.Spawn(prefab, origin - side, target, projectileSpeed, null, -side));
                    break;

                case ArrowVolleyStyle.Rain:
                    for (var i = 0; i < RainArrowCount; i++)
                    {
                        var landing = target.position + Quaternion.Euler(0f, Random.value * 360f, 0f) * Vector3.forward * (Random.value * RainRadius);
                        var start = landing + Vector3.up * (RainHeight + i * RainStagger);
                        AttachAura(skill, Projectile.SpawnToPoint(prefab, start, landing, projectileSpeed * RainSpeedMultiplier));
                    }

                    break;

                default:
                    AttachAura(skill, Projectile.Spawn(prefab, origin, target, projectileSpeed, null));
                    break;
            }
        }

        private static void AttachAura(SkillDefinition skill, GameObject arrow)
        {
            if (skill.ArrowAuraPrefab != null)
            {
                Object.Instantiate(skill.ArrowAuraPrefab, arrow.transform);
            }
        }
    }
}
