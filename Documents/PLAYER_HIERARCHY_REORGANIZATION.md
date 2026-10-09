# Player Hierarchy Reorganization

**Status: executed (Bootstrap scene, 2026-10-05).** The Player GameObject's components are split across child GameObjects. The manual copy/paste procedure originally planned in this document is obsolete and was not needed.

## Result

Root `Player` keeps what must live on the object that is the player: `NavMeshAgent`, `CharacterMovementController`, `PlayerCombatController`, `PlayerInput` and its routers (`PlayerInputRouter`, `SkillInputRouter`, `UIInputRouter`), `PlayerTargetSelector`, `Animator`, `PlayerAnimatorController`, `PlayerNpcInteractionController` (reads `transform.position`), `CameraInputForwarder`, and one component whose script was not identified (guid `df49f262...`).

Children:

- `Vitals`: `CharacterStatsHolder`, `HealthComponent`, `ManaComponent`, `PlayerDeathHandler`, `DamageNumberSpawner`, `PlayerHealthRegenController`
- `SkillTargeting`: `PlayerSkillCaster`, `SkillTargetingController`
- `Meta`: job progress, class, currency, name, stats, experience, gender, appearance, buff, status effect and status tint controllers
- `Items`: inventory, equipment manager, loot controller, hover detector, consumable user
- `SkillsData`: skill book, hotbar, block, element, passive skill and energy coat controllers
- `Persistence`: `PlayerSaveController`, `CharacterSessionBootstrap`, `PlayerLocationSaveParticipant`
- `Quests`: `QuestManager`
- `CombatHooks`: combat target indicators UI
- `EquipmentVisuals`: `PlayerWeaponAnimationBinder`, `EquipmentVisualController` (created by `Tools/Equipment/Wire Player In Open Scene`)
- `PlayerGeometry`, `AttackOrigin`, `PlayerStatsCanvas`, `PlayerNameTagCanvas`: visual and anchor objects

## Hard constraints

Keep these together, they rely on `RequireComponent` or `GetComponent` on the same object:

1. `CharacterStatsHolder` + `HealthComponent` + `ManaComponent` + `PlayerDeathHandler`
2. `PlayerSkillCaster` + `SkillTargetingController`
3. `NavMeshAgent` + `CharacterMovementController` + `PersistentPlayerAnchor` + `PlayerCombatController` (the camera and the facing logic use the root transform)

## Moving a component without rewiring

Copy/paste creates a new component instance, so every reference to it breaks. Editing the scene YAML does not: a reference points at the component's `fileID`, which does not change when the component's `m_GameObject` is changed and its entry moves to the other GameObject's `m_Component` list. The last move (health regen, quest manager, session bootstrap, location save participant) was done this way, so no field needed re-dragging. Do this only with the scene closed or reloaded in the Editor, and check that the component has no `GetComponent` or `transform` dependency first.

## Verification

Reload the Bootstrap scene without saving the old in-memory copy, enter Play Mode and check the Console for `MissingReferenceException`. Smoke test: move, attack, cast a skill, take damage and respawn, pick up and equip an item, open every UI window, save and reload. `CharacterSessionBootstrap` now sits on the `Persistence` object: confirm that character loading still runs in the right order relative to `PlayerSaveController`.
