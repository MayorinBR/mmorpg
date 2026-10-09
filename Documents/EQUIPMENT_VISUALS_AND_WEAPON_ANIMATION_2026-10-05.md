# Equipment visuals and weapon-driven attack animation (2026-10-05)

Two related changes, implemented together:

1. **Equipment visuals (phase A)**: every equipped item can show its own 3D model, attached to a bone of the character.
2. **Weapon-driven attack animation**: the Animator has a single Attack state, and the clip it plays depends on the equipped weapon subtype.

Status: implemented and committed, **not yet compiled or run in Unity**. Run the Editor menus (below) to finish the setup.

## 1. Equipment visuals

### Theory
The character is a Humanoid skeleton. A rigid object (sword, helmet) parented to a bone follows the animation at no cost. A garment that deforms with the body (boots, pants, body armor) needs a skinned mesh sharing the character's bones, which is phase B and is not built yet.

### Decisions
- **The item prefab is its own placement.** `ItemDefinition.equippedModel` is instantiated under the bone with its local transform preserved, so the prefab root transform is the offset. Author the pivot at the grip (weapons) or at the head origin (headgear) and no offset data is needed.
- **Bones come from the Humanoid avatar** (`Animator.GetBoneTransform`), not from bone names, so the `mixamorig:` prefix does not matter.
- **Full rebuild instead of diffing.** On every change the controller destroys all spawned models and re-instantiates them. There are at most five visible slots and equipment changes are rare.
- **Persistence needs nothing new.** `EquipmentManager.RestoreState` already raises `EquipmentChanged`, which rebuilds the visuals.

### Slot to bone mapping
Slot naming in this project: `LeftHand` is the main hand and `RightHand` is the off hand.

| Slot | Bone | Notes |
|---|---|---|
| `LeftHand` (main hand) | Right hand | A `Bow` goes to the left hand instead |
| `RightHand` (off hand) | Left hand | Shield, off-hand dagger |
| `HeadTop`, `HeadFace` | Head | |
| `Ammo` | Upper chest (falls back to Chest) | Quiver-style model, only if the item has one |
| `Body`, `Pants`, `Boots`, `Accessory` | none | Ignored until phase B |

A two-handed weapon occupies both hand slots but is instantiated once, on the main hand: the off-hand slot is skipped when the item's `RequiredSlots` contains `LeftHand`. Two copies of the same one-handed item in two hands are two separate records and both appear.

### Hair
`ItemDefinition.hidesHair` hides the hair model while the item is equipped. `HairCustomization` rebuilds its model on every change and destroys the old one only at the end of the frame, so right after a rebuild two models named `Hair` exist under the socket. The controller toggles every child with that name and re-applies the state on `HairCustomization.Changed`.

### Gender swap
`PlayerGenderController` now exposes `CurrentGeometry`. `EquipmentVisualController` listens to `GenderChanged` and rebuilds against the new model. `ItemDefinition.GetEquippedModel(gender)` returns `equippedModelFemale` for a female character when one is authored, otherwise the shared model.

## 2. Weapon-driven attack animation

### Theory
An `AnimatorOverrideController` wraps a base controller and replaces individual clips while keeping the same states, parameters and transitions. The Attack state keeps one placeholder clip; at runtime that clip is swapped for the one that belongs to the equipped weapon.

### How it works
1. `WeaponAnimationCatalog` maps `WeaponSubtype` to an attack clip, with a fallback clip for any subtype without an entry (or with an entry that has no clip).
2. `PlayerWeaponAnimationBinder` listens to `EquipmentManager.EquipmentChanged`, resolves the main-hand subtype and calls `PlayerAnimatorController.SetAttackClip`.
3. `PlayerAnimatorController` creates the override controller on the Animator and sets `overrideController[attackPlaceholderClip] = clip`. The chosen clip is remembered and re-applied in `SetAnimator`, so it survives a gender swap.
4. `PlayerCombatController.PerformAttack` always calls `SetAttackDuration(seconds)` and `TriggerAttack()`. `SetAttackDuration` reads the length of the active clip, so the swing still lasts exactly the Aspd-derived interval.

### API changes in `PlayerAnimatorController`
| Before | After |
|---|---|
| `TriggerAttack(bool isUnarmedMelee)` | `TriggerAttack()` |
| `TriggerRangedAttack()` | removed |
| `SetAttackDuration(seconds, isRanged, isUnarmedMelee)` | `SetAttackDuration(seconds)` |
| fields `meleeAttackClip`, `punchingClip`, `rangedAttackClip` | one field `attackPlaceholderClip` (`FormerlySerializedAs("meleeAttackClip")`) |
| none | `SetAttackClip(AnimationClip)` |

The `IsUnarmedMelee` and `AttackRanged` Animator parameters are no longer written. The `Attack` transition requires `IsUnarmedMelee == false`, which is its default, so the `Attack` state is always the one that plays.

### Kept on purpose
- **The Mage still plays `Cast` for its basic attack** (class exception in `PlayerCombatController`). There is no staff item yet, and an unarmed Mage would otherwise punch. Remove the exception once `Rod` or `Book` items exist with their own clips in the catalog.
- **Animation Events keep working.** `ReleaseProjectile` and any other event live on the clip, so the bow clip assigned to `Bow` must still contain its event.
- **Damage timing is unchanged**: still applied the moment the attack fires.

## Files

New, in `Assets/Scripts/Character/Combat`:
- `EquipmentVisualController.cs`: spawns and removes the item models.
- `EquipmentAnchorResolver.cs`: static slot-to-bone rule.
- `WeaponAnimationCatalog.cs` and `WeaponAnimationEntry.cs`: subtype to clip table.
- `PlayerWeaponAnimationBinder.cs`: applies the catalog on equipment changes.

Changed:
- `Items/ItemDefinition.cs`: `equippedModel`, `equippedModelFemale`, `hidesHair`, `HidesHair`, `GetEquippedModel(gender)`.
- `Character/Animation/PlayerAnimatorController.cs`: override controller, API above.
- `Character/Combat/PlayerCombatController.cs`: single attack trigger.
- `Character/Combat/PlayerGenderController.cs`: `CurrentGeometry`.
- `Character/Combat/Project.Character.Combat.asmdef`: added `Project.Character.Hair` (a leaf assembly with no references, so no cycle).

Tests, in `Assets/Tests/EditMode/CharacterCombat` (assembly `Project.Character.Combat.Tests`): `EquipmentAnchorResolverTests` (every slot, bow in the off hand) and `WeaponAnimationCatalogTests` (entry, missing entry, entry without clip).

## Editor automation
Three menus under `Tools/Equipment/` (`Assets/Scripts/Character/Combat/Editor/EquipmentSetupMenu.cs`, assembly `Project.Character.Combat.Editor`). All are idempotent. Run them in this order:

1. **Build Item Models**: for each item FBX under `Assets/Models/Items`, creates `<Name>_Toon.mat` (shader `ToonCharacter`, `_BaseColor.png` as base map; `_SkinMask` defaults to black, so no skin tint) and `Assets/Prefabs/Items/<Name>.prefab` (identity root with the FBX as child). Links `IronSword` to `TestWeapon` and `IronDagger` to `TestWeaponOff`. Creates placeholder items for `OakBow` (two-handed), `NordicAxe`, `AprenticeBook` and `ArcaneStaff` (Rod), and adds them to `ItemDatabase`. Placeholder items have no icon and zero prices.
2. **Build Weapon Animation Catalog**: creates `Assets/Data/WeaponAnimationCatalog.asset`. Fallback is Standing Melee Attack Downward; Unarmed and Dagger use Punching, Bow uses Shooting Arrow, Rod and Book use Spell Cast.
3. **Wire Player In Open Scene**: adds `PlayerWeaponAnimationBinder` and `EquipmentVisualController` on a new `EquipmentVisuals` child of the Player, assigns their references and fills `Attack Placeholder Clip` from the `Attack` state of `Player.controller` when empty. Save the scene afterwards.

Still manual: check each prefab pivot and rotation in the hand (see limits), and give the new items icons and prices.

## Known limits and follow-ups
- `AttackUnarmed` and `AttackRanged` states in `Player.controller` are now unused and can be deleted, together with the `IsUnarmedMelee` condition on the `Attack` transition.
- The hand bone orientation of a Mixamo rig varies, so a model may need a rotation on its prefab root.
- Replacing the override clip while an attack is playing may cause a one-frame pop. It is applied on equip, which normally happens out of combat.
- **Phase B (not built)**: skinned body armor, pants and boots, with the underlying body part hidden. The body models keep torso and pelvis as double shells (cloth and skin), so armor will need to hide the cloth shell. Needs a decision on whether the body `part_*` meshes are separated by region.
- The character creation preview does not show equipment.
- Sheathing weapons when out of combat, and idle/run posture per weapon, are not built. Both would extend the catalog entry.
- `FUTURE_IMPROVEMENTS.md`, section "Player animations", still describes the separate `AttackRanged` state and its trigger. This document supersedes it.
