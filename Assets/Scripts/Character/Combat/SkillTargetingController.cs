using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Project.Character.Movement;
using Project.Combat;
using Project.Skills;

namespace Project.Character.Combat
{
    /// <summary>
    /// Lets an Enemy-targeted skill be cast without already having a
    /// valid combat target selected. When
    /// <see cref="PlayerSkillCaster.TryCastSkill"/> has no valid target to
    /// cast on, it hands off here instead of just failing: this enters
    /// "picking" mode, raycasting the mouse against the enemy layer every
    /// frame so a UI layer can show a ring around whatever enemy is
    /// currently hovered (see <see cref="HoveredEnemyChanged"/>).
    /// Right-clicking a hovered enemy confirms it as both the pending
    /// skill's target and the player's new combat target, then
    /// re-attempts the cast; Escape, or a different skill being
    /// requested, cancels (or replaces) the pending pick instead.
    /// </summary>
    [RequireComponent(typeof(PlayerSkillCaster))]
    public class SkillTargetingController : MonoBehaviour
    {
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask enemyLayer;

        private PlayerSkillCaster caster;
        private SkillDefinition pendingSkill;
        private Collider hoveredEnemyCollider;

        /// <summary>
        /// The single active instance, set in <see cref="Awake"/>. Mirrors
        /// <see cref="Project.World.PersistentPlayerAnchor.Instance"/>'s
        /// justification: there's only ever one player, so a static
        /// reference is simpler than threading a serialized reference
        /// through every caller and UI element that needs to reach this.
        /// </summary>
        public static SkillTargetingController Instance { get; private set; }

        /// <summary>Raised when the hovered enemy changes while picking a target, including to null.</summary>
        public event Action<Transform> HoveredEnemyChanged;

        /// <summary>Gets whether a skill is currently waiting for the player to pick a target.</summary>
        public bool IsPicking => pendingSkill != null;

        private void Awake()
        {
            Instance = this;
            caster = GetComponent<PlayerSkillCaster>();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (pendingSkill == null)
            {
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPicking();
                return;
            }

            UpdateHover();

            if (hoveredEnemyCollider != null && Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                ConfirmPicking();
            }
        }

        /// <summary>
        /// Enters picking mode for the given skill, replacing whatever
        /// skill was already being picked, if any. Called by
        /// <see cref="PlayerSkillCaster.TryCastSkill"/> when it has no
        /// valid target to cast the skill on.
        /// </summary>
        /// <param name="skill">The skill waiting for a target.</param>
        public void BeginPicking(SkillDefinition skill)
        {
            pendingSkill = skill;
            SetHoveredEnemy(null);
        }

        private void UpdateHover()
        {
            if (worldCamera == null || Mouse.current == null || IsPointerOverUI())
            {
                SetHoveredEnemy(null);
                return;
            }

            var ray = worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out var hit, float.MaxValue, enemyLayer))
            {
                var damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null && !damageable.IsDead)
                {
                    SetHoveredEnemy(hit.collider);
                    return;
                }
            }

            SetHoveredEnemy(null);
        }

        private void SetHoveredEnemy(Collider collider)
        {
            if (hoveredEnemyCollider == collider)
            {
                return;
            }

            hoveredEnemyCollider = collider;
            HoveredEnemyChanged?.Invoke(collider != null ? collider.transform : null);
        }

        private void ConfirmPicking()
        {
            var skill = pendingSkill;
            var confirmedTarget = hoveredEnemyCollider;

            CancelPicking();
            targetSelector.SelectTarget(confirmedTarget);

            if (caster.HasValidDamageTarget(skill))
            {
                caster.TryCastSkill(skill);
            }
        }

        private void CancelPicking()
        {
            pendingSkill = null;
            SetHoveredEnemy(null);
        }

        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        /// <summary>
        /// Assigns the camera used to convert the pointer position into a
        /// world-space ray. Called by <see cref="Project.World.MapBootstrap"/>
        /// after a map loads, matching the same re-wiring every other
        /// persisted-player component needing a camera reference already
        /// requires (see the stale per-map camera reference fixes
        /// documented in FUTURE_IMPROVEMENTS.md).
        /// </summary>
        /// <param name="newWorldCamera">The active map's camera.</param>
        public void SetWorldCamera(Camera newWorldCamera)
        {
            worldCamera = newWorldCamera;
        }
    }
}
