# Target ring markers - 2026-10-21

**Status: working in play (confirmed by the user on 2026-10-21).**

Replaces the per-map `GroundRingFollower` discs. Each enemy prefab now owns its rings, so a map needs no ring wiring and each enemy can have its own ring size.

## Behaviour
- White ring: the cursor is over a living enemy. Covers auto-attacks and targeted skills such as Bash, because both use the same hover.
- Red ring: the enemy is the player's current combat target (right-click attack, or a skill pick that selects it).
- Red wins when an enemy is both hovered and targeted.
- Rings are children of the enemy root, not of the animated geometry, so they stay on the ground while the body bounces or animates. They hide when the enemy dies or is disabled.

## Components
- `TargetRingMarker` (`Project.UI`, on the enemy prefab root). Fields: `radius` (outer radius, world units), `groundOffset` (height above the pivot, world units), `hoverMaterial`, `targetMaterial`. Builds the two ring meshes in `Awake` from a shared annulus mesh; values stay in world units even if the root is scaled. A gizmo shows the radius when the prefab is selected. `Reset()` fills the default materials when the component is added in the Inspector.
- `PlayerInputRouter` raises `HoveredEnemyChanged(Transform)` from one enemy-layer raycast per frame (skipped over UI and for dead enemies).
- `CombatTargetIndicatorsUI` (Player) listens to `HoveredEnemyChanged` and `PlayerTargetSelector.TargetChanged` and toggles the marker of the old and new enemy. `SkillTargetingController` is no longer involved in ring display.
- Materials: `Assets/Materials/TargetRingHover.mat` (white) and `TargetRingTarget.mat` (red), URP Unlit. The Lit shader tinted the rings with the sky reflection (lilac), so keep them Unlit.

## Adding a new enemy
1. Copy an existing enemy prefab (the marker comes along), or select the new prefab and run `Tools/Enemies/Add Target Ring Marker To Selected Prefabs`, or add the component manually (materials fill in automatically).
2. Tune `radius` and `groundOffset` in the Inspector. The pivot is assumed to be at the feet.
3. `EnemyRosterBuilder` (local editor tool) scales `radius` together with the model when it enlarges a variant.

Starting radii: Poring, Poporing, Drops, Marin 0.7; Wolf 0.9; DireWolf 1.0; AlphaWolf 1.2; KingPoporing 1.6; Skeleton 0.8; SkeletonWarrior 1.0; DummyTarget 1.0. All are guesses to be tuned.

## Removed
- `MapBootstrap` ring fields and the `SetRings` call; `CombatTargetIndicatorsUI.SetRings`.

## Known leftovers (safe to delete in the Editor)
- `CurrentTargetRing` and `SkillTargetPickerRing` objects in `Prototype_Map01` (now unused).
- `Assets/Prefabs/UI/CurrentTargetRing.prefab` and `SkillTargetPickerRing.prefab` (created during an intermediate fix, unused).
- `GroundRingFollower` and `Materials/TargetPickerRing.mat` can go once the Map01 objects are deleted.

## Limits
- Hover also shows while picking a ground skill (Fire Wall) if the cursor crosses an enemy.
- One raycast per frame for hover; negligible at the current scale.
- Enemies placed without the marker simply show no ring (no error).
