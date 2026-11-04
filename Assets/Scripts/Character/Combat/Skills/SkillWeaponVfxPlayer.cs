using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Plays the visuals of a melee skill that charges the equipped weapon: a glow that follows the weapon
    /// and, for a skill that bursts around the caster, an explosion once the glow has built up.
    /// </summary>
    public sealed class SkillWeaponVfxPlayer : MonoBehaviour
    {
        private const float MinimumGlowScale = 0.5f;
        private const float MaximumGlowScale = 2.5f;

        [Tooltip("Optional. Source of the equipped weapon model. Left empty, it is looked up on this object's parents and children at first use.")]
        [SerializeField] private EquipmentVisualController visuals;

        [Tooltip("Seconds between the glow starting and the burst of an area-around-caster skill.")]
        [SerializeField, Min(0f)] private float burstDelaySeconds = 0.35f;

        [Tooltip("Length in metres of the weapon the glow prefabs are authored for. The glow is scaled by the real weapon length over this value.")]
        [SerializeField, Min(0.1f)] private float referenceWeaponLength = 1f;

        [Tooltip("Height above the character's feet where the glow appears while no weapon is equipped.")]
        [SerializeField] private float unarmedGlowHeight = 1f;

        private readonly List<FollowingGlow> glows = new List<FollowingGlow>();

        /// <summary>
        /// Plays the weapon glow of a skill and, when the skill bursts around the caster and has an impact effect,
        /// schedules that burst. Does nothing for a skill without a weapon glow.
        /// </summary>
        /// <param name="skill">The skill that was just cast.</param>
        public void Play(SkillDefinition skill)
        {
            if (skill == null || skill.WeaponGlowVfxPrefab == null)
            {
                return;
            }

            SpawnGlow(skill.WeaponGlowVfxPrefab);

            if (skill.TargetType == SkillTargetType.AreaAroundCaster && skill.ImpactVfxPrefab != null)
            {
                StartCoroutine(SpawnBurst(skill.ImpactVfxPrefab));
            }
        }

        private void LateUpdate()
        {
            for (var i = glows.Count - 1; i >= 0; i--)
            {
                var glow = glows[i];

                if (glow.Instance == null)
                {
                    glows.RemoveAt(i);
                    continue;
                }

                glow.Instance.position = GetAnchor(glow.WeaponRenderers, out _);
            }
        }

        private void SpawnGlow(GameObject prefab)
        {
            var weapon = ResolveVisuals()?.MainHandWeapon;
            var renderers = weapon != null ? weapon.GetComponentsInChildren<Renderer>() : System.Array.Empty<Renderer>();
            var instance = SkillVfx.Spawn(prefab, GetAnchor(renderers, out var length));

            if (instance == null)
            {
                return;
            }

            instance.transform.localScale *= Mathf.Clamp(length / referenceWeaponLength, MinimumGlowScale, MaximumGlowScale);
            glows.Add(new FollowingGlow(instance.transform, renderers));
        }

        private IEnumerator SpawnBurst(GameObject prefab)
        {
            yield return new WaitForSeconds(burstDelaySeconds);
            SkillVfx.Spawn(prefab, transform.position);
        }

        private EquipmentVisualController ResolveVisuals()
        {
            if (visuals == null)
            {
                visuals = GetComponentInParent<EquipmentVisualController>();
            }

            if (visuals == null)
            {
                visuals = GetComponentInChildren<EquipmentVisualController>();
            }

            return visuals;
        }

        private Vector3 GetAnchor(Renderer[] weaponRenderers, out float length)
        {
            var found = false;
            var bounds = new Bounds();

            foreach (var weaponRenderer in weaponRenderers)
            {
                if (weaponRenderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    bounds = weaponRenderer.bounds;
                    found = true;
                }
                else
                {
                    bounds.Encapsulate(weaponRenderer.bounds);
                }
            }

            if (!found)
            {
                length = referenceWeaponLength;
                return transform.position + Vector3.up * unarmedGlowHeight;
            }

            length = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            return bounds.center;
        }

        private sealed class FollowingGlow
        {
            public FollowingGlow(Transform instance, Renderer[] weaponRenderers)
            {
                Instance = instance;
                WeaponRenderers = weaponRenderers;
            }

            public Transform Instance { get; }

            public Renderer[] WeaponRenderers { get; }
        }
    }
}
