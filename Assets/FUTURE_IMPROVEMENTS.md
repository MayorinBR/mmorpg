# Future Improvements / Known Technical Debt

This document tracks considerations raised during development that were deliberately deferred — these aren't bugs, they're conscious scope decisions worth revisiting later.

---

## Performance

- **`WorldItemHoverDetector` raycasts every frame**, regardless of whether the mouse actually moved. Irrelevant with few items on screen; if an area ends up with many simultaneous drops, consider only re-raycasting when the pointer position actually changes.
- **`EnemyController.DetectPlayer()`** runs `Physics.OverlapSphere` every `Update()` while the mob is Idle. Fine with few mobs; with dozens on screen, an interval (e.g. every 0.2–0.5s) would be cheaper than every frame.
- **Domain Reload not optimized** — `Edit > Project Settings > Editor > Enter Play Mode Settings` allows disabling "Reload Domain" to drastically speed up entering Play Mode. Deferred because it requires extra care with static state/singletons (state can "leak" between test sessions).

## Architecture / Organization

- **`Prefabs/Characters` is still empty** — the Player still exists only as a scene object, never turned into a real prefab, unlike `Prefabs/Enemies` (Poring/Poporing are now proper Prefab Variants). Worth doing before the Player gets iterated on further, since scene-only changes are harder to review in Git diffs than prefab changes.
- **`HealthBarUI` and `ManaBarUI` are near-identical duplicates** (same Slider-driving logic, different source component). A unification via a shared `IResourceProvider` interface was implemented and then deliberately reverted back to two separate scripts, by team preference (simplicity/independence over DRY in this case). Worth remembering before re-proposing the same unification later.
- **`Project.DebugTools` is not restricted to the Editor platform** (unlike `Project.EditorTools`). Debug loggers (`HealthDebugLogger`, `EquipmentDebugLogger`) could end up in the final build if the components are forgotten on scene objects before publishing.
- **No automated tests** cover the pure logic classes (`Inventory.TryAddItem`, `EquipmentManager` multi-slot/eviction) — good candidates for the Unity Test Framework, since they've caused subtle bugs more than once during manual development.

## Balancing (placeholders assumed deliberately)

- **`FlatStatPointCostStrategy`**: costs a flat 1 point per stat increase, regardless of current value. **Real target formula now documented** (source: iRO Wiki — Stats, consulted July 2026): `floor((X-1)/10) + 2` for stat value X = 1–99 (steeper tier for 100–129, third-class only). Full table in the companion Design Data Tracker's "Leveling & Stat Costs" sheet. Not yet implemented.
- **Stat points granted per Base Level-up**: currently a flat 5 points (`PlayerExperience.statPointsPerLevel`). **Real target formula now documented**: tiered by level range — roughly `floor(level/5) + 3` under level 100, coarser bands from 100–200. Same tracker sheet. Not yet implemented.
- **`LinearExperienceCurve`** (XP required per level): still a fully open placeholder (100 × current level). The iRO Wiki Stats reference documents stat-point *rewards* per level, not XP *thresholds* — that curve needs a different source (e.g. the wiki's Experience/Levels pages) when it's tackled.

## Incomplete systems / hooks for future features

- **Quasi-stats not yet modeled**: Attack Range (partially covered by existing attack/aggro ranges), Cast Time and Cast Delay (skills cast instantly today, no channeling — `Cooldown` itself is now implemented per-skill in `PlayerSkillCaster`), Critical Hit Shield, Perfect Dodge, Perfect Hit, and Status Effect Resistance (no status effect system exists). See the Design Data Tracker's "Quasi-Stats" sheet for the full reference.
- **Movement Speed is correctly NOT tied to AGI** — this matches Ragnarok Online's actual behavior (a common misconception is that it should be). Worth remembering before ever "fixing" this as if it were a bug.
- **Stat Points Reset** — no mechanism exists to respec invested stat points. Ragnarok Online offers this via NPCs/consumables/one-time resets; a simplified version (without the advanced-class trappings) is a plausible near-future QoL addition.
- **Talent Stats (POW/STA/WIS/SPL/CON/CRT) and Level 200+ substats (P.ATK/S.MATK/RES/MRES)** — explicitly out of scope for now. These assume fourth-job classes and very high character levels, far beyond the current roadmap; documented in the GDD (Section 3.7) purely for future reference, not as near-term work.
- **Grid-based (SQM) movement, considered and deferred.** Explored making click-to-move snap to a Ragnarok-style square grid (`GridSystem`/`GridVisualizer`, later reverted). Worth revisiting once the project needs it more concretely — likely alongside the authoritative-server migration, since grid coordinates are cheap to sync/validate compared to free-form `Vector3` positions. Key open decisions for whenever this comes back:
  - Whether WASD movement should also be grid-locked, or stay free while only click-to-move/pathing snaps to cells (asymmetry vs. consistency trade-off).
  - Cell size, chosen carefully since it will affect every range/distance value in the game (attack range, aggro radius, skill AoE) once adopted — expensive to change later.
  - Character footprint in cells (1x1 vs. larger, e.g. 3x3) — not yet decided.
  - Whether the visual grid indicator needs to be visible in-game (shader/mesh) or Scene-view-only (Gizmos) is enough.
- **"Ammo" item type** (arrows, bullets) not implemented yet — needs special handling because it's stack-consumable while equipped, unlike the rest of the equipment system (which assumes "one item, worn once").
- **`ItemDefinition` has no `OnValidate()`** guarding against inconsistent Inspector configuration (e.g. an `Equipment` item marked `Is Stackable`, or with no `Required Slots` set).

## Skill system UI (implemented)

The Skill Book and Skill Hotbar windows are implemented: drag-and-drop from a learned skill's icon in the Skill Book onto any of the 10 hotbar slots (`PlayerSkillHotbar`, keys 1-9 and 0) binds it to that slot, and each hotbar slot (`SkillHotbarSlotUI`) reflects real-time availability via `PlayerSkillCaster.GetAvailability` — normal icon when ready, grayscale with a countdown timer (`GetCooldownRemaining`, shown to 1 decimal place) while on cooldown, red tint when blocked for any other reason (not learned, insufficient mana, no valid target). The Skill Book (`SkillBookWindowUI`/`SkillBookEntryUI`) lists every skill available to the current class with icon, name, current/max level, mana cost, and cooldown, plus a Learn button spending a `PlayerJobProgress` skill point.

Known gaps left from this pass:

- **No tooltip on hotbar hover.** The Skill Book shows mana cost/cooldown/description, but hovering a hotbar slot shows nothing — has to reopen the Skill Book to check what a bound skill does. A natural next step, reusing the `ItemTooltipUI`-style hover pattern already used elsewhere.
- **No minimum-level requirement to learn a skill**, only class restriction. `SkillDefinition` doesn't have a `requiredLevel` field yet; the quasi-stat gap for this was already noted below.
- **Hotbar slot assignments aren't persisted** — like everything else, they reset when Play Mode stops. Rolls into the general persistence gap noted further down.
- **`PlayerSkillHotbar.SlotCount`** was bumped from 4 to 10 (keys 1-9 and 0) after the initial 4-slot version shipped. Because `slots` is a `[SerializeField]` array sized from that constant only at authoring time, existing scene/prefab instances kept their old serialized array length (4) even after the constant changed — `GetSkill`/`SetSkill` then threw `IndexOutOfRangeException` for the new slot indices until the array was manually resized to 10 in the Inspector. Worth remembering for any future constant-driven array resize: bumping the constant in code does not resize already-serialized data: check the Inspector every time.

## Planned skill system expansion (documented, not implemented)

The current skill system (`SkillDefinition`, `PlayerSkillCaster`) only supports instant single-target Damage or Heal effects. Three kinds of skills are planned but deliberately not built yet — noted here with enough shape to avoid re-deriving the architecture later:

- **Temporary buffs and debuffs** (e.g. +20% ATK for 30s, -10% DEF for 15s). Needs a new `StatusEffectDefinition` (which stat, magnitude, duration) and a per-character `StatusEffectController` tracking active effects and their remaining time, applying/removing modifiers. This becomes a third layer in the stat pipeline, alongside the existing Base Stats + Equipment layers (`IStatProvider`/`EquippedStatsView`) — likely another `IStatProvider` implementation that sums active effect modifiers.
- **Ground-targeted area skills with no fixed target** (e.g. a Fire Wall that damages whoever walks into it, an AoE heal zone). Needs a different cast flow from today's (which always resolves to a specific `IDamageable`): the player would click a world position instead of selecting a target, spawning a persistent "zone" object (trigger collider + duration timer) that applies its effect to anything that enters/stays inside it, checking layers to determine who it affects (enemies only, allies only, or both, depending on the skill).
- **Elemental damage and resistances** (Fire, Ice, Holy, Dark, Wind, Electric, Neutral, etc.). Needs an `Element` enum, an optional `Element` field on `SkillDefinition` (and eventually weapons), and a per-element resistance/weakness table added to `CharacterStatsDefinition` (or a new asset type for enemies specifically) — damage would multiply by the target's resistance value for the attack's element. Connects to the already-noted Status Effect Resistance quasi-stat gap.

- **Ally-target skills (`SkillTargetType.Ally`) currently behave identically to Self-target** — `StrongHeal` can only heal the caster today, since no "select another player as a target" mechanic exists yet (there's no multiplayer to test it against). The data field is in place; only the targeting input is missing.

None of this is scheduled — it's here so a future skill-system pass starts from an already-thought-through shape instead of a blank page.

## Player feedback (UX)

- **`PlayerLootController.TryCollect` can fail silently** if the inventory is at max weight — the player walks up to the item, nothing happens, with no message explaining why.
- **Equip requirement checks (`EquipmentManager.MeetsRequirements`) also fail silently** — clicking an item that doesn't meet level/class requirements just does nothing, with no message explaining why. Same category as the weight-capacity silent failure above; worth solving both together when a general "action feedback" UI element is built.
- **A pursued item can be destroyed mid-chase** (another player grabs it first, future time-based despawn) — today the player just stops walking with no explanation.

## UI Window System

- **Window position is remembered per session only, not saved to disk.** Dragging a window and closing/reopening it keeps the position while the game is running, but restarting the Editor/build resets it to the default cascade. Connects to the general persistence gap above.
- **`WindowLayoutManager` and every window assume a top-left anchor and pivot.** This is an implicit contract, not enforced by code — configuring a new window panel with a different anchor/pivot will silently place it in the wrong spot, with no error.
- **The cascade reads each window's width/height at layout time.** Fine for fixed-size panels; if a future window uses a `Content Size Fitter` or otherwise resizes based on content, its size at `Relayout()` time might not match its final size, causing subtle overlap.
- **Bring-to-front only fires on clicks that hit the window's background or its title bar (drag start).** Clicking directly on a child element (a button, a stat row) doesn't bring the window forward, since that element consumes the pointer event instead of bubbling it up.

## Lesson learned: destroyed Unity object references

- Comparing a cached `MonoBehaviour`/`GameObject` reference directly against `null` doesn't reliably detect "was destroyed while referenced" in the same way as a plain `bool` flag — Unity's overloaded `==` operator treats destroyed objects as equal to `null`, which caused a real bug in `WorldItemHoverDetector` (tooltip staying on screen after the hovered item was collected). Worth keeping in mind for any future system that caches a reference to something that can be destroyed at runtime (mobs, pickups, projectiles): prefer a separate `bool` flag alongside the reference rather than relying solely on the `== null` check.

## Lesson learned: Awake-ordering and GetComponent lookups

- **Caching a `GetComponent<T>()` result only in `Awake()` is fragile** if any *other* component might read that cached value before this component's own `Awake()` has run (Unity doesn't guarantee `Awake` order across different GameObjects). This caused a real `NullReferenceException` in `HealthBarUI` reading `HealthComponent.MaxHealth` before `HealthComponent`'s own `Awake()` had resolved its `CharacterStatsHolder`. Fixed by resolving lazily (on first access, cached from then on) instead of only in `Awake()` — safe regardless of who asks first. Worth using this pattern by default for any "look up a sibling component once" case, not just when a bug surfaces.
- **`GetComponent<T>()` only checks the exact GameObject hit, not its parents.** When a collider lives on a child object (e.g. an imported model's mesh, like Poring's `Geometry` child) but the component you actually need (`HealthComponent`, etc.) lives on the prefab root, `GetComponent` silently returns null instead of finding it. `PlayerTargetSelector` had this exact bug. `GetComponentInParent<T>()` is the safer default whenever a hit collider might not live on the same object as the data you need from it.

## Lesson learned: serialized scene references on dynamically-instantiated prefabs

- **A `[SerializeField]` reference to a scene object (e.g. the parent `Canvas`) cannot be pre-wired on a prefab asset that gets `Instantiate`d at runtime** — there's no scene instance to drag into the field at author time, so it silently stays `null` on every clone. This caused a `NullReferenceException` in `SkillBookEntryUI`'s drag-and-drop (needed its `Canvas` to parent a floating drag icon). Fixed by resolving it lazily via `GetComponentInParent<Canvas>()` in `Awake()` instead of a serialized field — the same pattern `ItemTooltipUI` already used for its own parent canvas. Worth defaulting to this pattern for any component that will be cloned from a prefab asset and needs a reference to whatever scene hierarchy it ends up parented under.

## Multiplayer / Persistence (out of scope for this phase, but relevant to the roadmap)

- **No persistence exists yet** — all progress (level, XP, points, inventory, equipment, window layout) is lost when Play Mode stops. Even a simple local save (JSON/PlayerPrefs) would meaningfully help the current testing phase.
- **Everything runs fully client-trusted**, with no server validation — expected at this stage (pre-Fish-Networking/authoritative server), but the more systems that depend on this structure, the more expensive migration becomes later. Worth keeping in mind, for each new system, how it would eventually become a server-validated action.