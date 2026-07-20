# Future Improvements / Known Technical Debt

This document tracks considerations raised during development that were deliberately deferred — these aren't bugs, they're conscious scope decisions worth revisiting later.

---

## Performance

- **`WorldItemHoverDetector` raycasts every frame**, regardless of whether the mouse actually moved. Irrelevant with few items on screen; if an area ends up with many simultaneous drops, consider only re-raycasting when the pointer position actually changes.
- **`EnemyController.DetectPlayer()`** runs `Physics.OverlapSphere` every `Update()` while the mob is Idle. Fine with few mobs; with dozens on screen, an interval (e.g. every 0.2–0.5s) would be cheaper than every frame.
- **Domain Reload not optimized** — `Edit > Project Settings > Editor > Enter Play Mode Settings` allows disabling "Reload Domain" to drastically speed up entering Play Mode. Deferred because it requires extra care with static state/singletons (state can "leak" between test sessions).

## Architecture / Organization

- **`Project.DebugTools` is not restricted to the Editor platform** (unlike `Project.EditorTools`). Debug loggers (`HealthDebugLogger`, `EquipmentDebugLogger`) could end up in the final build if the components are forgotten on scene objects before publishing.
- **No automated tests** cover the pure logic classes (`Inventory.TryAddItem`, `EquipmentManager` multi-slot/eviction) — good candidates for the Unity Test Framework, since they've caused subtle bugs more than once during manual development.

## Balancing (placeholders assumed deliberately)

- **`FlatStatPointCostStrategy`**: always costs 1 point per stat increase, regardless of current value. Needs to become the real RO curve before launch.
- **`LinearExperienceCurve`**: required XP = 100 × current level. Test formula, not balanced.

## Incomplete systems / hooks for future features

- **`ItemDefinition.RequiredLevel` and `AllowedClasses`** exist as data, but **nothing validates them yet** — needs a character class system (`PlayerClass` or equivalent) before validation makes sense.
- **"Ammo" item type** (arrows, bullets) not implemented yet — needs special handling because it's stack-consumable while equipped, unlike the rest of the equipment system (which assumes "one item, worn once").
- **`ItemDefinition` has no `OnValidate()`** guarding against inconsistent Inspector configuration (e.g. an `Equipment` item marked `Is Stackable`, or with no `Required Slots` set).

## Player feedback (UX)

- **`PlayerLootController.TryCollect` can fail silently** if the inventory is at max weight — the player walks up to the item, nothing happens, with no message explaining why.
- **A pursued item can be destroyed mid-chase** (another player grabs it first, future time-based despawn) — today the player just stops walking with no explanation.

## Multiplayer / Persistence (out of scope for this phase, but relevant to the roadmap)

- **No persistence exists yet** — all progress (level, XP, points, inventory, equipment) is lost when Play Mode stops. Even a simple local save (JSON/PlayerPrefs) would meaningfully help the current testing phase.
- **Everything runs fully client-trusted**, with no server validation — expected at this stage (pre-Fish-Networking/authoritative server), but the more systems that depend on this structure, the more expensive migration becomes later. Worth keeping in mind, for each new system, how it would eventually become a server-validated action.
