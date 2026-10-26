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
    /// Lets a skill be cast without already having what it needs selected.
    /// When <see cref="PlayerSkillCaster.TryCastSkill"/> has no valid enemy
    /// target to cast an Enemy-targeted skill on, or the skill is
    /// <see cref="SkillTargetType.Ground"/>-targeted (which never has one
    /// already selected), it hands off here instead of just failing: this
    /// enters "picking" mode, raycasting the mouse every frame — against
    /// the enemy layer for an enemy pick (so a UI layer can show a ring
    /// around whatever's hovered, see <see cref="HoveredEnemyChanged"/>),
    /// or against the ground layer for a Ground skill (see
    /// <see cref="HoveredGroundPointChanged"/>). Right-clicking a hovered
    /// enemy confirms it as both the pending skill's target and the
    /// player's new combat target, then re-attempts the cast; right-clicking
    /// a hovered ground point instead calls
    /// <see cref="PlayerSkillCaster.TryCastSkillAtPosition"/> directly, since
    /// a ground position isn't a combat target. Escape, or a different
    /// skill being requested, cancels (or replaces) the pending pick either way.
    /// </summary>
    [RequireComponent(typeof(PlayerSkillCaster))]
    public class SkillTargetingController : MonoBehaviour
    {
        [SerializeField] private PlayerTargetSelector targetSelector;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Layer raycast against while picking a ground position for a SkillTargetType.Ground skill (e.g. Fire Wall) — typically the terrain/ground layer.")]
        [SerializeField] private LayerMask groundLayer;

        private PlayerSkillCaster caster;
        private SkillDefinition pendingSkill;
        private Collider hoveredEnemyCollider;
        private Vector3? hoveredGroundPoint;
        private SkillDefinition queuedSkill;
        private float nextApproachTime;
        private PlayerInputRouter inputRouter;
        private CharacterMovementController movement;
        private PlayerCombatController combat;

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

        /// <summary>Raised when the hovered ground point changes while picking a Ground skill's position, including to null.</summary>
        public event Action<Vector3?> HoveredGroundPointChanged;

        /// <summary>Gets whether a skill is currently waiting for the player to pick a target.</summary>
        public bool IsPicking => pendingSkill != null;

        /// <summary>Gets the skill waiting for a target or ground position, or null when not picking.</summary>
        public SkillDefinition PendingSkill => pendingSkill;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // A short-lived duplicate from a map scene being reloaded
                // (e.g. warping back into a previously visited map) — this
                // GameObject is already being destroyed as a duplicate.
                // Don't let it steal Instance from the real, persisted player.
                return;
            }

            Instance = this;
            caster = GetComponent<PlayerSkillCaster>();
            inputRouter = transform.root.GetComponentInChildren<PlayerInputRouter>(true);
            movement = transform.root.GetComponentInChildren<CharacterMovementController>(true);
            combat = transform.root.GetComponentInChildren<PlayerCombatController>(true);
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
                UpdateQueuedCast();
                return;
            }

            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CancelPicking();
                return;
            }

            if (pendingSkill.TargetType == SkillTargetType.Ground)
            {
                UpdateGroundHover();

                if (hoveredGroundPoint.HasValue && IsConfirmPressed())
                {
                    ConfirmGroundPicking();
                }
                else if (IsConfirmPressed() && !IsPointerOverUI())
                {
                    CancelPicking();
                }

                return;
            }

            UpdateHover();

            if (hoveredEnemyCollider != null && IsConfirmPressed())
            {
                ConfirmPicking();
            }
            else if (IsConfirmPressed() && !IsPointerOverUI())
            {
                CancelPicking();
            }
        }

        /// <summary>
        /// Enters picking mode for the given skill, replacing whatever
        /// skill was already being picked, if any. Called by
        /// <see cref="PlayerSkillCaster.TryCastSkill"/> when it has no
        /// valid target to cast the skill on, or when the skill is
        /// <see cref="SkillTargetType.Ground"/>-targeted.
        /// </summary>
        /// <param name="skill">The skill waiting for a target.</param>
        public void BeginPicking(SkillDefinition skill)
        {
            pendingSkill = skill;
            SetQueuedSkill(null);
            inputRouter?.SetClicksSuppressed(true);
            SetHoveredEnemy(null);
            SetHoveredGroundPoint(null);
        }

        /// <summary>
        /// Queues a skill on the already selected enemy, so the player walks
        /// into the skill's range and casts it there, instead of asking for
        /// a new pick. Queuing again replaces the previous queued skill.
        /// </summary>
        /// <param name="skill">The skill to cast once in range.</param>
        /// <returns>True when an alive target was selected and the skill was queued.</returns>
        public bool TryQueueSkill(SkillDefinition skill)
        {
            var damageable = targetSelector.CurrentDamageable;

            if (targetSelector.CurrentTarget == null || damageable == null || damageable.IsDead)
            {
                return false;
            }

            CancelPicking();
            SetQueuedSkill(skill);
            return true;
        }

        /// <summary>
        /// Drops the queued skill, called when the player chooses another skill.
        /// </summary>
        public void ClearQueuedSkill()
        {
            SetQueuedSkill(null);
        }

        private void SetQueuedSkill(SkillDefinition skill)
        {
            queuedSkill = skill;

            if (combat != null)
            {
                combat.IsSkillApproachPending = skill != null;
            }
        }

        private static bool IsConfirmPressed()
        {
            return Mouse.current != null
                && (Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame);
        }

        /// <summary>
        /// Casts the skill that was queued because its target was out of
        /// range, walking toward the target until it is close enough. The
        /// queue is dropped when the target is lost or dies, for example
        /// when the player clicks the ground to walk elsewhere.
        /// </summary>
        private void UpdateQueuedCast()
        {
            if (queuedSkill == null)
            {
                return;
            }

            var target = targetSelector.CurrentTarget;
            var damageable = targetSelector.CurrentDamageable;

            if (target == null || damageable == null || damageable.IsDead)
            {
                SetQueuedSkill(null);
                return;
            }

            if (caster.HasValidDamageTarget(queuedSkill))
            {
                var skill = queuedSkill;
                SetQueuedSkill(null);
                movement?.StopMovement();
                caster.TryCastSkill(skill);
                return;
            }

            if (movement != null && Time.time >= nextApproachTime)
            {
                nextApproachTime = Time.time + 0.2f;
                movement.SetClickDestination(target.position);
            }
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

        /// <summary>
        /// Raises <see cref="HoveredEnemyChanged"/> with the GameObject that
        /// owns the collider's <see cref="IDamageable"/> rather than the
        /// collider's own transform — the same reasoning as
        /// <see cref="PlayerTargetSelector.SelectTarget"/>, so the skill
        /// picker's ring doesn't drift away from the enemy's actual
        /// position while a purely cosmetic geometry animation plays.
        /// </summary>
        /// <param name="collider">The currently hovered enemy's collider, or null.</param>
        private void SetHoveredEnemy(Collider collider)
        {
            if (hoveredEnemyCollider == collider)
            {
                return;
            }

            hoveredEnemyCollider = collider;

            var hoveredEnemy = collider != null
                ? (collider.GetComponentInParent<IDamageable>() as Component)?.transform ?? collider.transform
                : null;

            HoveredEnemyChanged?.Invoke(hoveredEnemy);
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
            else if (targetSelector.CurrentTarget != null)
            {
                SetQueuedSkill(skill);
            }
        }

        private void UpdateGroundHover()
        {
            if (worldCamera == null || Mouse.current == null || IsPointerOverUI())
            {
                SetHoveredGroundPoint(null);
                return;
            }

            var ray = worldCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out var hit, float.MaxValue, groundLayer))
            {
                SetHoveredGroundPoint(hit.point);
            }
            else
            {
                SetHoveredGroundPoint(null);
            }
        }

        private void SetHoveredGroundPoint(Vector3? point)
        {
            if (hoveredGroundPoint == point)
            {
                return;
            }

            hoveredGroundPoint = point;
            HoveredGroundPointChanged?.Invoke(point);
        }

        private void ConfirmGroundPicking()
        {
            var skill = pendingSkill;
            var position = hoveredGroundPoint.Value;

            CancelPicking();
            caster.TryCastSkillAtPosition(skill, position);
        }

        private void CancelPicking()
        {
            pendingSkill = null;
            inputRouter?.SetClicksSuppressed(false);
            SetHoveredEnemy(null);
            SetHoveredGroundPoint(null);
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
