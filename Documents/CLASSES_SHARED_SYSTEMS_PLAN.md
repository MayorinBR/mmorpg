# Other Five Classes — Shared Systems Plan

Source: [iRO Wiki — Mage](https://irowiki.org/wiki/Mage), [Merchant](https://irowiki.org/wiki/Merchant), [Acolyte](https://irowiki.org/wiki/Acolyte), [Thief](https://irowiki.org/wiki/Thief), [Archer](https://irowiki.org/wiki/Archer), consulted September 2026. Companion to `CLASSES_SWORDMAN.md` (same per-class-series intent, see its own header), but scoped differently: this is an architecture plan across all five remaining base classes, not a single class's implementation record. Purpose: find which skill mechanics repeat across classes so the shared system gets built once, not re-derived per class when each one's actual implementation pass happens. **Correction, 2026-09-14: this document originally claimed nothing in it was implemented yet, which was wrong — `CLASSES_SWORDMAN.md`'s own pass (dated two days earlier) had already shipped items 1 (status effects, stun-only), 2 (buff payload) and the HP regen-over-time system this document's item 1 also depends on, and this document's own text simply hadn't been reconciled against that when it was written.** See item 1 below for the corrected status, and the new "Mage damage kit" section for the first skills actually authored against this plan.

## Already covered by existing systems (data only, no new code)

Skills whose mechanic already has a home and just need authoring as an asset once that class is actually implemented:

- **Level-scaling single/multi-hit damage** (`SkillEffectType.Damage`, `CalculateDamage` = stat × multiplier × level): Fire Bolt, Cold Bolt, Lightning Bolt (Mage — "launch 1~level bolts, each MATK×1" is exactly this formula, multiplier 1; **now authored as assets, see "Mage damage kit" below**), Soul Strike, Holy Light, Double Strafe, Envenom's damage half, Stone Fling, Sand Attack. No new mechanic, just tuning + an `Element` value (see the open question below).
- **Mammonite** (Merchant) — a normal single-target Damage skill; only needs the new Zeny-cost field below, nothing else.
- **Weapon-gated flat ATK passive** (`PlayerPassiveSkillController`, `SkillDefinition.PassiveRequiredWeaponSubtypes`) — no new instance of this among these five classes' passives (Thief/Archer passives below need a *different* generalization, not this one as-is).
- **Mage's `PlayerElementController`** already exists (selectable attack element, currently "purely informational" per its own doc comment) — Fire Bolt/Cold Bolt/Lightning Bolt/Fire Ball/Fire Wall don't need it: each is its own skill asset with its own fixed `Element`, independent of the auto-attack element toggle. `PlayerElementController` stays relevant only for the Mage's basic (non-skill) attack.

## New shared systems needed (priority order — most skills unblocked per build first)

### 1. Status effects — IMPLEMENTED, stun only (correction: already shipped, not still pending)
`CLASSES_SWORDMAN.md` item 4 already shipped this: `StatusEffectController` (`Project.Combat`) holds a single timed stun (`IsStunned`/`ApplyStun`), wired as an optional hook on both the player and `EnemyController`, unblocking Fatal Blow. This document's original text below described the still-open, broader shape — kept as-is since none of it is built yet, only the stun case is: **Stone Curse** (petrify/immobilize + element conversion), **Frost Driver** (freeze + element conversion), **Cure**/**Detoxify** (cure status), **Decrease AGI**/**Signum Crucis** (chance-based debuff — see item 6), **Stone Fling**/**Sand Attack** (stun/blind chance), **Envenom** (poison DoT). Proposed shape for the rest: widen `StatusEffectController` from its current single stun timer to a small set of timed flags (Poisoned, Silenced, Blinded, Frozen, Petrified alongside the existing Stunned), each gating a specific existing action (movement, basic attack, skill cast) at the one choke point each already goes through — poison's HP-drain-over-time additionally needs the same "apply an amount on a timer" primitive `PlayerHealthRegenController` (also already implemented, see `CLASSES_SWORDMAN.md` item 3) already ticks HP regen with. Not built yet because no skill in this project needs any status beyond stun today — the Mage kit added below doesn't touch this system at all (Frost Driver/Stone Curse are deliberately not part of that pass).

### 2. Generalized buff/debuff payload — IMPLEMENTED
`BuffController` now carries a `BuffPayload` (`Assets/Scripts/Combat/BuffPayload.cs`): the original AtkPercent/DefPercent/MdefFlat trio, plus AspdPercent, MaxHealthFlat, and flat STR/AGI/VIT/INT/DEX/LUK. The old `ActiveBuff`/`PersistentModifier` structs and separate `Sum`/`SumPersistent` helpers collapsed into one `Total` property (`BuffPayload`'s own `+` operator sums the active list and the persistent dictionary); `ApplyBuff`/`SetPersistentModifier` now take a `BuffPayload` instead of a handful of loose floats/ints. `PlayerSkillCaster.TryCastBuff` builds the payload from three new `SkillDefinition` fields (`buffStatBonusPerLevel: StatModifiers`, `buffAspdPercentPerLevel`, `buffMaxHealthPerLevel`); `PlayerBerserkController` and `PlayerStatsController` were updated to the new signatures. `PlayerStatsController` folds a buff's STR/AGI/VIT/INT/DEX/LUK component into `effectiveStats` through a second, reused `JobBonusStatsView` layer (the same class already used for the Job Level bonus — no new decorator type needed), multiplies `CurrentSubStats`' Aspd by `BuffController.AspdMultiplier` the same way it already did for ATK, and adds `BuffController.MaxHealthBonus` in `GetMaxHealthBonus`.

One deviation from the original plan below: `BuffPayload` carries raw `int`/`float` fields rather than an actual `StatModifiers` value. `Project.Combat` cannot reference `Project.Items` (`StatModifiers`'s assembly) without creating a cycle — `Project.Items` already references `Project.Combat`. The conversion from `BuffPayload`'s raw fields to a real `StatModifiers` happens in `PlayerStatsController.ToStatModifiers`, which sits at the top of the assembly graph and can see both. `StatModifiers`'s constructor was made public and a `*(StatModifiers, int)` operator was added so `SkillDefinition.GetBuffStatBonus` can scale a per-level value the same way the other per-level getters do.

This is infrastructure only — no buff skill (Blessing, Increase AGI, Angelus, Improve Concentration) is authored as an asset yet; that happens when Acolyte/Archer get their own implementation pass, per item 3 below.

Original proposal (superseded by the above, kept for context): extend the buff payload to carry a flat `StatModifiers` delta (reusing the struct `ItemDefinition.StatBonuses`/`EquipmentManager.GetBonus` already use — same shape, no new type) alongside the existing Atk%/Def%/Mdef channels, plus add Aspd% and MaxHealth-flat channels since both recur here. Keep the existing timed-list + persistent-dictionary dual design (already proven by Provoke and Berserk) — nothing about *how* a buff expires or gets replaced needs to change, only *what* it can carry.

### 3. Party support (multi-target buff propagation)
No party/group system exists anywhere in this project today (confirmed — not mentioned in `FUTURE_IMPROVEMENTS.md` at all). Blocks **Blessing**, **Increase AGI**, **Angelus** (all "self + party") and Merchant's **Crazy Uproar**. This is the biggest open prerequisite in this whole plan — Acolyte's real-RO identity is almost entirely party support, so it's worth deciding early whether a lightweight party model (even just "every other nearby player," since this project has no confirmed multiplayer grouping UI yet either) is in scope before Acolyte gets its own implementation pass, or whether Acolyte's first pass ships self-only versions of these buffs and party propagation follows once a real party system exists for any class to use.

### 4. Stealth / detection (Hide–Reveal)
Blocks **Hiding** (Thief), **Sight** (Mage), **Ruwach** (Acolyte), and the reveal half of **Improve Concentration** (Archer). Proposed: an `IsHidden` flag on the player, checked wherever AI/target-selection currently enumerates valid targets; a reveal skill just clears the flag for anyone in its radius. Real RO's "doesn't work on Boss/Insect/Demon" exception needs the monster Race classification (item 7) — fine to ship without that exception first and add it once Race exists, since no enemy in this project has a Race value to check against yet anyway.

### 5. Ground-targeted persistent zone skills — IMPLEMENTED (damage-on-stay case), now also authored for Fire Wall
`SkillTargetType` gained a `Ground` value and `SkillEffectType` gained `Zone`. A Ground skill never has an "already selected" position the way an Enemy target can already be selected, so `PlayerSkillCaster.TryCastSkill` always routes it into `SkillTargetingController`'s picking flow (`BeginPicking`), same as a Damage skill with no valid target. `SkillTargetingController` was extended with a second picking mode alongside its existing enemy-picking one: when the pending skill is Ground-targeted, it raycasts the mouse against a new `groundLayer` every frame instead of `enemyLayer` (`HoveredGroundPointChanged` mirrors `HoveredEnemyChanged` for UI), and right-click calls the new `PlayerSkillCaster.TryCastSkillAtPosition(skill, position)` directly instead of going through `PlayerTargetSelector` — a ground point isn't a combat target.

`TryCastSkillAtPosition` mirrors `TryCastSkill`'s learned/cooldown/dead checks, then `TryCastZone` spends mana and spawns a new `SkillZoneController` (`Assets/Scripts/Combat/SkillZoneController.cs`) at the picked position via `SkillZoneController.Spawn(...)`/`Initialize(...)`. The zone ticks on its own interval (`SkillDefinition.ZoneTickIntervalSeconds`) for its duration (`GetZoneDuration(level)`), each tick rolling a fresh `Physics.OverlapSphere` against a target layer (reusing `PlayerSkillCaster`'s existing `enemyLayer` field — no new one needed) and applying a fixed damage/accuracy/element/category baked in at cast time, the same "resolve once at cast time" approach every other skill here already uses. Reused `SkillDefinition.AreaRadius` for the zone's radius and the existing Damage-header fields (`damageMultiplierPerLevel`, `damageType`, `element`, `accuracyBonusPerLevel`) for its per-tick damage, rather than adding parallel Zone-specific copies. New Zone-only fields: `zonePrefab` (optional visual, null = logic-only), `zoneDurationSeconds`/`zoneDurationPerLevel`, `zoneTickIntervalSeconds`.

Two deliberate scope decisions:
- `SkillZoneController` lives in `Project.Combat`, so it does **not** apply `WeaponSizeModifiers` (that's `Project.Items`, downstream of `Project.Combat` — same assembly-cycle constraint as item 2's `BuffPayload`) — marked with a `ponytail:` comment. This matches real RO anyway: Fire Wall is spell damage, not a weapon hit.
- This unblocks **Fire Wall** (Mage) — a pure "damage anything standing inside" zone — end to end, **now authored as `FireWall.asset`, see "Mage damage kit" below**. It does **not** yet unblock **Safety Wall** (blocks melee) or **Pneuma** (Acolyte, blocks ranged): "block an incoming attack" is a fundamentally different mechanic than "deal damage on a tick" — it needs an interception point in the attack-resolution path (`PlayerCombatController.DealHit`, `PlayerSkillCaster`'s damage methods, and the mirrored enemy-attacks-player path), and no such hook exists anywhere in this codebase yet. Building it now would be speculative — no Acolyte implementation pass has started, and it's cleaner to design once the actual melee/ranged distinction at attack time is needed for something. The zone spawn/lifetime/detection core built here (`SkillZoneController`, the ground-picking flow) is the reusable half; Pneuma will need a small additional "does a zone block this attack" query layered on top when Acolyte's class pass actually happens.

### 6. Instant AoE centered on a target/location (not the caster) — IMPLEMENTED, now also authored for Fire Ball and Thunderstorm
`SkillTargetType` gained an `AreaAroundTarget` value alongside the existing `AreaAroundCaster` (Magnum Break). `PlayerSkillCaster.TryCastDamage` still requires a valid target in range for `AreaAroundTarget` (same range/mana gate as a single-target skill, via `HasValidDamageTarget`), then bursts on it instead of hitting only it. The actual "hit everyone within `AreaRadius`" loop — previously `TryCastAreaDamage`'s own body — was extracted into a shared `ApplyAreaDamage(skill, level, center)`, called with `transform.position` for `AreaAroundCaster` and the resolved target's position for `AreaAroundTarget`; no duplicated hit-roll/damage code between the two. `SkillDefinition.IsAreaOfEffect` still gates whether a Damage skill is an AoE at all (kept as a separate bool rather than derived from `TargetType`, since nothing guarantees existing skill assets — e.g. Magnum Break's — already have `TargetType` set to match; safer to leave that bool as the single source of truth for now), while `TargetType` now decides *where* the AoE is centered and whether a target must be selected first.

This unblocks **Fire Ball**, **Thunderstorm** (Mage) and **Arrow Shower** (Archer) — **Fire Ball and Thunderstorm are now authored as assets, see "Mage damage kit" below**; Arrow Shower still isn't, since no Archer implementation pass has started.

### 7. Chance-based skill effects (success rate independent of Hit/Flee)
Blocks **Decrease AGI**/**Signum Crucis** (Acolyte, both "X% success chance" debuffs), **Double Attack** (Thief, proc chance on basic attack), **Steal** (Thief, RO's own steal-chance formula), and the freeze/poison-inflict chances on **Frost Driver**/**Envenom**. Proposed: a `successChancePerLevel` (or flat) field on `SkillDefinition`'s relevant block, rolled once via a plain `Random.value` check before applying the effect — same simplicity as the existing dodge roll, no new RNG abstraction needed.

### 8. Monster classification: Race (pairs with the already-tracked Size gap)
`CLASSES_SWORDMAN.md` item 5 already shipped monster **Size** (unblocking the weapon-vs-size damage table). This pass adds **Race** to the same gap: blocks **Heal**'s bonus damage to Undead, **Demon Bane**/**Divine Protection** (Acolyte passives vs. Demon/Undead), and the Boss/Insect/Demon exception on Hiding (item 4). Worth building both axes together when either is finally tackled, since they're the same kind of addition (a new enum field on `CharacterStatsDefinition`/enemy prefabs) landing at the same time.

### 9. Elements this project's `Element` enum doesn't cover — open question
Current `Element` enum: `Water, Fire, Grass, Ground, Electric, Neutral` — a deliberately simplified set, not a direct port of Ragnarok Online's own ten-element wheel (Neutral/Water/Earth/Fire/Wind/Poison/Holy/Shadow/Ghost/Undead). Fire and Water map cleanly; Ground stands in for Earth; Electric is the closest analog to Wind. **Resolved for the Mage damage kit below**: Lightning Bolt and Thunderstorm both use `Electric`, and their description text says "Electric-element" rather than "Wind-element" — the flavor name follows the actual system element a player can see resisted/exploited, not RO's own lore name for the skill. Three real skills still don't have any reasonable existing value to reuse: **Soul Strike**/**Napalm Beat** (Ghost), **Holy Light**/Heal's undead-damage/**Ruwach** (Holy), **Envenom** (Poison, as an element — distinct from the Poison *status effect* in item 1). This still needs a decision when Acolyte/Thief actually get built: add `Ghost`/`Holy`/`Poison` as new `Element` values (cheap — the enum's raw-int Unity serialization only requires appending, never inserting, exactly like every other append-only enum in this project) versus deliberately folding them into `Neutral` to keep the element wheel small on purpose. Flagging now so it isn't decided by accident mid-implementation.

### 10. Generalize flat passive stat bonuses beyond ATK
`PlayerPassiveSkillController` today only sums a flat Status ATK bonus. Blocks **Improve Dodge** (Thief — flat Flee, not weapon-gated at all), **Owl's Eye** (Archer — flat DEX), **Vulture's Eye** (Archer — Hit/range with bow). Proposed: widen the passive's payload from "flat ATK" to a small stat-type + amount pair (or reuse the same `StatModifiers` delta as item 2), read by whichever system already owns that stat (`PlayerStatsController.CurrentSubStats` for Flee/Hit, same as `GetPassiveAttackBonus()` today for ATK). The existing weapon-gating (`PassiveRequiredWeaponSubtypes`) stays optional and already defaults to "always applies" when empty, which is exactly what Improve Dodge needs (it isn't gated to a weapon at all).

### 11. Displacement / knockback
Blocks **Arrow Repel** (Archer — pushes target back) and **Back Slide** (Thief — pushes self back). Both are the same primitive: move a transform N cells along a direction, respecting whatever obstacle/NavMesh checks movement already goes through. Small standalone utility, not worth a "system" — a static helper is enough.

### 12. Skills with a Zeny cost
Blocks only **Mammonite** among these five classes' kits. Trivial: add `zenyCost` to `SkillDefinition`, checked via `PlayerCurrency.TrySpend` alongside the existing mana-cost check in `PlayerSkillCaster`. Not worth grouping as a "system" — just a field, mentioned here so it isn't missed when Mammonite gets built. **Still not built** — `Mammonite.asset` (see the description-sync note below) is a plain Damage skill today with no Zeny cost, matching real RO's damage side only.

## Mage damage kit — IMPLEMENTED, 2026-09-14

Fire Bolt, Cold Bolt, Lightning Bolt, Fire Ball, Thunderstorm and Fire Wall are now authored as real skill assets under `Assets/Data/Skills/Mage/`, registered in both `MageBase.asset` (the Mage's own `SkillDatabase`, read by the Skill Book window) and the master `SkillDatabase.asset` (used by the save system to resolve a learned/hotbarred skill by id). Chosen as the first real Mage skills specifically because every mechanic they need already existed before this pass — no new shared system, just tuning:

- **Fire Bolt / Cold Bolt / Lightning Bolt**: plain single-target `SkillEffectType.Damage`, `damageMultiplierPerLevel: 1` — real RO's "launch 1~10 bolts, each MATK×1" maps exactly onto this project's `CalculateDamage = MATK × multiplier × level` with no compression needed, unlike most other ported skills. Water / Fire / Electric respectively (Electric standing in for RO's own Wind, per item 9 above).
- **Fire Ball / Thunderstorm**: `TargetType.AreaAroundTarget` + `IsAreaOfEffect`, `AreaRadius: 4` (same "5×5 area" radius convention as Magnum Break). Fire Ball's `damageMultiplierPerLevel` (0.68) is linearly compressed to reach real RO's level-10 upper bound (MATK×3.4) at this project's level-5 cap — the same convention `MagnumBreak.asset` already established, so it undershoots real's level-1 minimum the same way Magnum Break does. Thunderstorm needed no compression: real's "1~10 hits, each MATK×1" collapses onto this project's single-hit-per-cast model the same way the bolts' formula does.
- **Fire Wall**: this project's first `SkillEffectType.Zone` skill asset (the mechanic itself, `SkillZoneController`, already existed per item 5). `TargetType.Ground`, `AreaRadius: 1.5` (a small zone standing in for RO's directional wall shape, which this project's sphere-overlap zone can't represent), `damageMultiplierPerLevel: 0.1` reaching real's flat 50% MATK per tick at level 5, `ZoneDurationSeconds`/`ZoneDurationPerLevel` reaching 15s at level 5 (matching `SkillZoneController`'s own default duration). No visual prefab yet (`zonePrefab` left empty — logic-only, same as every other zone until a visual is authored).

**Not done by this pass, deliberately**: Soul Strike, Napalm Beat, Frost Driver, Stone Curse, Sight, Safety Wall, Increase SP Recovery and Energy Coat — each still blocked on an unbuilt system (Ghost element, status effects beyond stun, stealth, the melee/ranged-block query, or is class-exclusive), per the table below. No icons yet for any of the six new skills — same shared-sprite-sheet gap tracked since the Swordman pass (still only 4 slices, already used by BasicHeal/StrongHeal/Bash/Snipe). Scene wiring (`SkillBookWindowUI.allSkills` in `Prototype_Map01.unity`) is a manual Editor step, not attempted here — same reasoning as every other scene-YAML caution already documented in this project: too much serialized surface for a blind hand-edit.

### Skill description text sync, 2026-09-14
Several skill assets across every class still had placeholder description text (e.g. `"Fire Bolt Skill"`, `"Bash Skill"`) left over from when they were first stubbed out. Fixed to real, sourced text (iRO Wiki, consulted September 2026) for **FireBolt, Bash, StrongHeal, BasicHeal, Mammonite and Envenom**, each trimmed to describe only what's actually implemented today (no Zeny cost on Mammonite's text, no poison chance on Envenom's, no Undead bonus on StrongHeal's — matching the same "describe the shipped mechanic, not the full real-RO one" convention `CLASSES_SWORDMAN.md`'s own Endure/Provoke/Increase HP Recovery descriptions already established) rather than a verbatim wiki copy-paste.

**Found in passing, not fixed**: `Snipe.asset` (Archer)'s name doesn't correspond to any real base-Archer skill — the iRO Wiki lists no skill called "Snipe" for base Archer; the closest real analog is Double Strafe. Its description was still updated to accurately describe what it actually does (a single-target physical Damage skill) rather than force-fitting Double Strafe's real flavor text onto a differently-named skill. Renaming `Snipe` itself (to `Double Strafe`, or keeping the invented name deliberately) is a naming decision for whoever does Archer's own implementation pass, not something to change unilaterally here.

## Class-exclusive features (touch one class only, still worth naming so nothing's forgotten)

- **Merchant**: Pushcart/Vending/Cart Revolution/Cart Decoration/Change Cart/Open Buying Store — a whole player-shop-and-cart economy layer, RO's own peer-to-peer trading system. Large, essentially Merchant-only, likely its own multi-pass feature whenever Merchant's turn comes rather than something to front-load. **Discount**/**Overcharge** (NPC shop price modifiers) depend on whatever NPC shop system exists (not investigated here — separate check needed when Merchant is actually built). **Item Appraisal** needs an "unidentified item" flag on items/inventory that doesn't exist yet. **Enlarge Weight Limit** is the one easy piece — `Inventory.MaxCarryWeight` is a plain constructor value today; giving it the exact same optional-bonus-provider hook `IMaxHealthBonusProvider` already established would cover this with almost no new code.
- **Acolyte**: **Teleport**/**Warp Portal** — a player-cast, memorized-location fast-travel skill. Distinct from the already-built `Project.World`/`WarpPortal` inter-map system (that's fixed, scene-authored portals; this is player-triggered, to a remembered point). **Aqua Benedicta** (crafting holy water near water) is minor and easy to defer.
- **Thief**: **Steal** — RO's own steal-chance-vs-monster formula and one-steal-per-monster tracking; ties into loot but is otherwise Thief-only.
- **Archer**: **Arrow Crafting** — converting materials into ammo; a small standalone recipe/conversion feature, low reuse elsewhere.
- **Mage**: **Energy Coat** — a toggled, SP-draining damage-reduction buff. Shape-wise this is close to Berserk's controller (a component watching a condition and driving a persistent `BuffController` entry) but drains SP on a timer instead of watching HP — can likely reuse most of that pattern once Berserk's code exists as a reference (it already does, per this session's earlier work).

## Suggested build order

1. ~~**Status effects, stun only**~~ (item 1) — **IMPLEMENTED.** Broadening it to Poisoned/Silenced/Blinded/Frozen/Petrified is still the highest-reuse system left across all six classes, including Swordman's own Endure flinch-resist half, but nothing currently blocks on it beyond the skills item 1 itself lists.
2. **Generalized `BuffController` payload** (item 2) — IMPLEMENTED. Needed by nearly every Acolyte support skill and Archer's Improve Concentration.
3. **Party support** (item 3) — the real prerequisite question for whether Acolyte's first implementation pass can do anything beyond self-buffs.
4. **Stealth/detection** (item 4), then **instant AoE-at-target** (item 6, IMPLEMENTED, now used by Fire Ball/Thunderstorm) and **ground zone skills** (item 5, IMPLEMENTED for the damage-on-stay case, now used by Fire Wall — the melee/ranged-block mechanic for Safety Wall/Pneuma still needs item 1-adjacent design work) as Acolyte/Archer actually get built.
5. Everything else (items 7–12) are one-or-two-skill unlocks each — build them opportunistically per class rather than ahead of need, same YAGNI reasoning `CLASSES_SWORDMAN.md` already applied to its own gaps.

## Per-class skill → system mapping

### Mage
| Skill | Real effect | Depends on |
|---|---|---|
| Fire Bolt / Cold Bolt / Lightning Bolt | MATK×level, single target, elemental | **Implemented** — `FireBolt.asset`/`ColdBolt.asset`/`LightningBolt.asset` |
| Fire Ball | Fire AoE around target | **Implemented** — `FireBall.asset` (item 6) |
| Fire Wall | Persistent damaging wall | **Implemented** — `FireWall.asset` (item 5) |
| Thunderstorm | Wind AoE around target, multi-hit | **Implemented** — `Thunderstorm.asset` (item 6) |
| Soul Strike / Napalm Beat | Ghost-element, splash on Napalm Beat | Existing Damage system + item 9 (Ghost) + item 6 (done, Napalm Beat's splash) |
| Frost Driver | Water damage + freeze + element conversion | Item 1 (freeze), element-conversion is a further open question not covered above |
| Stone Curse | Petrify + element conversion | Item 1 |
| Sight | Reveal hidden in area | Item 4 |
| Safety Wall | Melee-blocking zone | Item 5 (zone core done, melee-block query still needed) |
| Increase SP Recovery | Passive SP regen boost | Same regen-over-time mechanism as `CLASSES_SWORDMAN.md` item 3 (implemented for HP), extended to SP |
| Energy Coat | Toggled SP-drain damage reduction | Class-exclusive, see above |

### Merchant
| Skill | Real effect | Depends on |
|---|---|---|
| Mammonite | Melee damage + Zeny cost | Damage half implemented (`Mammonite.asset`, description synced); Zeny cost still needs item 12 |
| Discount / Overcharge | NPC price modifiers | Class-exclusive (NPC shop system, unverified) |
| Enlarge Weight Limit | +carry weight | Class-exclusive, but easy (provider-hook pattern) |
| Item Appraisal | Identify items | Class-exclusive (needs "unidentified" item flag) |
| Pushcart / Vending / Cart Revolution / Cart Decoration / Change Cart / Open Buying Store | Cart + player-shop economy | Class-exclusive, large, own feature pass |
| Crazy Uproar | Party ATK/STR buff | Item 3 |

### Acolyte
| Skill | Real effect | Depends on |
|---|---|---|
| Heal | HP restore, bonus vs. Undead | Existing Heal system + item 8 (Race) for the Undead half |
| Blessing | Flat STR/DEX/INT/Hit, self + party | Item 2 (done) + item 3 |
| Increase AGI | Flat AGI + ASPD% + move speed, self + party | Item 2 (done) + item 3 |
| Angelus | DEF% + flat Max HP, self + party | Item 2 (done) + item 3 |
| Cure | Removes silence/blind/chaos | Item 1 |
| Decrease AGI / Signum Crucis | Chance-based debuff vs. Demon/Undead | Item 7 + item 8 |
| Demon Bane / Divine Protection | Flat bonus vs. Demon/Undead | Item 8 + item 10 (generalized passive payload) |
| Holy Light / Ruwach | Holy damage / reveal + damage | Item 9 (Holy) / item 4 |
| Pneuma | Ranged-blocking zone | Item 5 (zone core done, ranged-block query still needed) |
| Teleport / Warp Portal | Fast travel | Class-exclusive, see above |
| Aqua Benedicta | Craft holy water | Class-exclusive, minor |

### Thief
| Skill | Real effect | Depends on |
|---|---|---|
| Double Attack | Proc chance, double damage w/ dagger | Item 7 + weapon-gating already established |
| Improve Dodge | Flat Flee | Item 10 |
| Envenom | Poison damage + poison chance | Damage half implemented (`Envenom.asset`, description synced); poison element (item 9) + poison status (item 1) + chance (item 7) still needed |
| Steal | Steal item from monster | Class-exclusive |
| Hiding | Become hidden | Item 4 |
| Detoxify | Cure poison | Item 1 |
| Back Slide | Self-displacement | Item 11 |
| Stone Fling / Sand Attack | Damage + stun/blind chance | Item 1 + item 7 |

### Archer
| Skill | Real effect | Depends on |
|---|---|---|
| Arrow Shower | AoE around target | Item 6 (done) |
| Double Strafe | Single-target damage | Existing Damage system, no gap — see the Snipe naming note above; this project's `Snipe.asset` may actually be standing in for this skill |
| Arrow Repel | Damage + knockback | Item 11 |
| Arrow Crafting | Craft ammo | Class-exclusive |
| Improve Concentration | DEX/AGI% buff + reveal hidden | Item 2 (done) + item 4 |
| Owl's Eye | Flat DEX | Item 10 |
| Vulture's Eye | Hit/range with bow | Item 10 |
