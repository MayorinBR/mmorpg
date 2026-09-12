# Other Five Classes — Shared Systems Plan

Source: [iRO Wiki — Mage](https://irowiki.org/wiki/Mage), [Merchant](https://irowiki.org/wiki/Merchant), [Acolyte](https://irowiki.org/wiki/Acolyte), [Thief](https://irowiki.org/wiki/Thief), [Archer](https://irowiki.org/wiki/Archer), consulted September 2026. Companion to `CLASSES_SWORDMAN.md` (same per-class-series intent, see its own header), but scoped differently: this is an architecture plan across all five remaining base classes, not a single class's implementation record. Purpose: find which skill mechanics repeat across classes so the shared system gets built once, not re-derived per class when each one's actual implementation pass happens. **Nothing in this document is implemented yet** — it's the design pass before any of it is built, cross-referencing what `FUTURE_IMPROVEMENTS.md` and `CLASSES_SWORDMAN.md` already flagged as missing (status effects, monster Size, ground-targeted skills) so this doesn't re-discover those, only extends them with what these five classes' full kits reveal.

## Already covered by existing systems (data only, no new code)

Skills whose mechanic already has a home and just need authoring as an asset once that class is actually implemented:

- **Level-scaling single/multi-hit damage** (`SkillEffectType.Damage`, `CalculateDamage` = stat × multiplier × level): Fire Bolt, Cold Bolt, Lightning Bolt (Mage — "launch 1~level bolts, each MATK×1" is exactly this formula, multiplier 1), Soul Strike, Holy Light, Double Strafe, Envenom's damage half, Stone Fling, Sand Attack. No new mechanic, just tuning + an `Element` value (see the open question below).
- **Mammonite** (Merchant) — a normal single-target Damage skill; only needs the new Zeny-cost field below, nothing else.
- **Weapon-gated flat ATK passive** (`PlayerPassiveSkillController`, `SkillDefinition.PassiveRequiredWeaponSubtypes`) — no new instance of this among these five classes' passives (Thief/Archer passives below need a *different* generalization, not this one as-is).
- **Mage's `PlayerElementController`** already exists (selectable attack element, currently "purely informational" per its own doc comment) — Fire Bolt/Cold Bolt/Lightning Bolt/Fire Ball/Fire Wall don't need it: each is its own skill asset with its own fixed `Element`, independent of the auto-attack element toggle. `PlayerElementController` stays relevant only for the Mage's basic (non-skill) attack.

## New shared systems needed (priority order — most skills unblocked per build first)

### 1. Status effects
Already tracked (`FUTURE_IMPROVEMENTS.md`'s Status Effect Resistance quasi-stat note; `CLASSES_SWORDMAN.md` item 4, blocking Fatal Blow). This pass across five more classes confirms it's the single highest-leverage system left: **Stone Curse** (petrify/immobilize + element conversion), **Frost Driver** (freeze + element conversion), **Cure**/**Detoxify** (cure status), **Decrease AGI**/**Signum Crucis** (chance-based debuff — see item 6), **Stone Fling**/**Sand Attack** (stun/blind chance), **Envenom** (poison DoT). Proposed shape: an optional hook component (`StatusEffectController`, same single-hook pattern as `BuffController`) holding a small set of timed flags (Stunned, Poisoned, Silenced, Blinded, Frozen, Petrified), each gating a specific existing action (movement, basic attack, skill cast) at the one choke point each already goes through — poison's HP-drain-over-time additionally needs whatever the HP regen-over-time item lands on (see item 3 in `CLASSES_SWORDMAN.md`), since a damage-over-time tick and a regen-over-time tick are the same underlying "apply an amount on a timer" primitive.

### 2. Generalized buff/debuff payload
`BuffController` currently only carries `AtkPercent`/`DefPercent`/`MdefFlat` (built for Provoke/Endure/Berserk). Acolyte's whole kit needs more: **Blessing** (flat STR/DEX/INT + flat Hit), **Increase AGI** (flat AGI + ASPD% + move speed), **Angelus** (DEF% + flat Max HP), **Improve Concentration** (Archer — DEX/AGI%). Proposed: extend the buff payload to carry a flat `StatModifiers` delta (reusing the struct `ItemDefinition.StatBonuses`/`EquipmentManager.GetBonus` already use — same shape, no new type) alongside the existing Atk%/Def%/Mdef channels, plus add Aspd% and MaxHealth-flat channels since both recur here. Keep the existing timed-list + persistent-dictionary dual design (already proven by Provoke and Berserk) — nothing about *how* a buff expires or gets replaced needs to change, only *what* it can carry.

### 3. Party support (multi-target buff propagation)
No party/group system exists anywhere in this project today (confirmed — not mentioned in `FUTURE_IMPROVEMENTS.md` at all). Blocks **Blessing**, **Increase AGI**, **Angelus** (all "self + party") and Merchant's **Crazy Uproar**. This is the biggest open prerequisite in this whole plan — Acolyte's real-RO identity is almost entirely party support, so it's worth deciding early whether a lightweight party model (even just "every other nearby player," since this project has no confirmed multiplayer grouping UI yet either) is in scope before Acolyte gets its own implementation pass, or whether Acolyte's first pass ships self-only versions of these buffs and party propagation follows once a real party system exists for any class to use.

### 4. Stealth / detection (Hide–Reveal)
Blocks **Hiding** (Thief), **Sight** (Mage), **Ruwach** (Acolyte), and the reveal half of **Improve Concentration** (Archer). Proposed: an `IsHidden` flag on the player, checked wherever AI/target-selection currently enumerates valid targets; a reveal skill just clears the flag for anyone in its radius. Real RO's "doesn't work on Boss/Insect/Demon" exception needs the monster Race classification (item 7) — fine to ship without that exception first and add it once Race exists, since no enemy in this project has a Race value to check against yet anyway.

### 5. Ground-targeted persistent zone skills
Already designed, not yet built — `FUTURE_IMPROVEMENTS.md` already has the shape written down: a world-position click instead of a target selection, spawning a persistent trigger-collider object with a duration timer, applying its effect (layer-filtered) to anything that enters/stays inside. Blocks **Safety Wall** (blocks melee), **Fire Wall** (damages on entry), **Pneuma** (Acolyte, blocks ranged). Distinct from item 6 below — these persist over time and affect an area at a placed location, not a one-shot burst.

### 6. Instant AoE centered on a target/location (not the caster)
A smaller, separate gap from item 5: today's only AoE is `SkillTargetType.AreaAroundCaster` (Magnum Break — centered on the caster's own position, one-shot). **Fire Ball**, **Thunderstorm**, **Napalm Beat**'s splash, and **Arrow Shower** all need the same one-shot AoE burst but centered on the selected target instead. Proposed: a new `SkillTargetType.AreaAroundTarget` value, reusing the exact same `AreaRadius`/damage-to-everything-within-radius code Magnum Break already has, just resolving the center point from `target.position` instead of `caster.position`. Much cheaper than item 5 — no persistent object, no duration, no trigger collider.

### 7. Chance-based skill effects (success rate independent of Hit/Flee)
Blocks **Decrease AGI**/**Signum Crucis** (Acolyte, both "X% success chance" debuffs), **Double Attack** (Thief, proc chance on basic attack), **Steal** (Thief, RO's own steal-chance formula), and the freeze/poison-inflict chances on **Frost Driver**/**Envenom**. Proposed: a `successChancePerLevel` (or flat) field on `SkillDefinition`'s relevant block, rolled once via a plain `Random.value` check before applying the effect — same simplicity as the existing dodge roll, no new RNG abstraction needed.

### 8. Monster classification: Race (pairs with the already-tracked Size gap)
`CLASSES_SWORDMAN.md` item 5 already flagged monster **Size** as missing (blocks the weapon-vs-size damage table). This pass adds **Race** to the same gap: blocks **Heal**'s bonus damage to Undead, **Demon Bane**/**Divine Protection** (Acolyte passives vs. Demon/Undead), and the Boss/Insect/Demon exception on Hiding (item 4). Worth building both axes together when either is finally tackled, since they're the same kind of addition (a new enum field on `CharacterStatsDefinition`/enemy prefabs) landing at the same time.

### 9. Elements this project's `Element` enum doesn't cover — open question
Current `Element` enum: `Water, Fire, Grass, Ground, Electric, Neutral` — a deliberately simplified set, not a direct port of Ragnarok Online's own ten-element wheel (Neutral/Water/Earth/Fire/Wind/Poison/Holy/Shadow/Ghost/Undead). Fire and Water map cleanly; Ground stands in for Earth; Electric is the closest analog to Wind (fits "Lightning Bolt" thematically even though RO itself calls that skill Wind-element). Three real skills here don't have any reasonable existing value to reuse: **Soul Strike**/**Napalm Beat** (Ghost), **Holy Light**/Heal's undead-damage/**Ruwach** (Holy), **Envenom** (Poison, as an element — distinct from the Poison *status effect* in item 1). This needs a decision when Mage/Acolyte/Thief actually get built: add `Ghost`/`Holy`/`Poison` as new `Element` values (cheap — the enum's raw-int Unity serialization only requires appending, never inserting, exactly like every other append-only enum in this project) versus deliberately folding them into `Neutral` to keep the element wheel small on purpose. Flagging now so it isn't decided by accident mid-implementation.

### 10. Generalize flat passive stat bonuses beyond ATK
`PlayerPassiveSkillController` today only sums a flat Status ATK bonus. Blocks **Improve Dodge** (Thief — flat Flee, not weapon-gated at all), **Owl's Eye** (Archer — flat DEX), **Vulture's Eye** (Archer — Hit/range with bow). Proposed: widen the passive's payload from "flat ATK" to a small stat-type + amount pair (or reuse the same `StatModifiers` delta as item 2), read by whichever system already owns that stat (`PlayerStatsController.CurrentSubStats` for Flee/Hit, same as `GetPassiveAttackBonus()` today for ATK). The existing weapon-gating (`PassiveRequiredWeaponSubtypes`) stays optional and already defaults to "always applies" when empty, which is exactly what Improve Dodge needs (it isn't gated to a weapon at all).

### 11. Displacement / knockback
Blocks **Arrow Repel** (Archer — pushes target back) and **Back Slide** (Thief — pushes self back). Both are the same primitive: move a transform N cells along a direction, respecting whatever obstacle/NavMesh checks movement already goes through. Small standalone utility, not worth a "system" — a static helper is enough.

### 12. Skills with a Zeny cost
Blocks only **Mammonite** among these five classes' kits. Trivial: add `zenyCost` to `SkillDefinition`, checked via `PlayerCurrency.TrySpend` alongside the existing mana-cost check in `PlayerSkillCaster`. Not worth grouping as a "system" — just a field, mentioned here so it isn't missed when Mammonite gets built.

## Class-exclusive features (touch one class only, still worth naming so nothing's forgotten)

- **Merchant**: Pushcart/Vending/Cart Revolution/Cart Decoration/Change Cart/Open Buying Store — a whole player-shop-and-cart economy layer, RO's own peer-to-peer trading system. Large, essentially Merchant-only, likely its own multi-pass feature whenever Merchant's turn comes rather than something to front-load. **Discount**/**Overcharge** (NPC shop price modifiers) depend on whatever NPC shop system exists (not investigated here — separate check needed when Merchant is actually built). **Item Appraisal** needs an "unidentified item" flag on items/inventory that doesn't exist yet. **Enlarge Weight Limit** is the one easy piece — `Inventory.MaxCarryWeight` is a plain constructor value today; giving it the exact same optional-bonus-provider hook `IMaxHealthBonusProvider` already established would cover this with almost no new code.
- **Acolyte**: **Teleport**/**Warp Portal** — a player-cast, memorized-location fast-travel skill. Distinct from the already-built `Project.World`/`WarpPortal` inter-map system (that's fixed, scene-authored portals; this is player-triggered, to a remembered point). **Aqua Benedicta** (crafting holy water near water) is minor and easy to defer.
- **Thief**: **Steal** — RO's own steal-chance-vs-monster formula and one-steal-per-monster tracking; ties into loot but is otherwise Thief-only.
- **Archer**: **Arrow Crafting** — converting materials into ammo; a small standalone recipe/conversion feature, low reuse elsewhere.
- **Mage**: **Energy Coat** — a toggled, SP-draining damage-reduction buff. Shape-wise this is close to Berserk's controller (a component watching a condition and driving a persistent `BuffController` entry) but drains SP on a timer instead of watching HP — can likely reuse most of that pattern once Berserk's code exists as a reference (it already does, per this session's earlier work).

## Suggested build order

1. **Status effects** (item 1) — highest reuse across all six classes, including Swordman's own still-unblocked Fatal Blow and Endure's flinch-resist half.
2. **Generalized `BuffController` payload** (item 2) — needed by nearly every Acolyte support skill and Archer's Improve Concentration.
3. **Party support** (item 3) — the real prerequisite question for whether Acolyte's first implementation pass can do anything beyond self-buffs.
4. **Stealth/detection** (item 4), then **instant AoE-at-target** (item 6, cheap) and **ground zone skills** (item 5, more involved) as Mage/Acolyte/Archer actually get built.
5. Everything else (items 7–12) are one-or-two-skill unlocks each — build them opportunistically per class rather than ahead of need, same YAGNI reasoning `CLASSES_SWORDMAN.md` already applied to its own gaps.

## Per-class skill → system mapping

### Mage
| Skill | Real effect | Depends on |
|---|---|---|
| Fire Bolt / Cold Bolt / Lightning Bolt | MATK×level, single target, elemental | Existing Damage system; element mapping (item 9: Electric for Lightning) |
| Fire Ball | Fire AoE around target | Item 6 |
| Fire Wall | Persistent damaging wall | Item 5 |
| Thunderstorm | Wind AoE around target, multi-hit | Item 6 |
| Soul Strike / Napalm Beat | Ghost-element, splash on Napalm Beat | Existing Damage system + item 9 (Ghost) + item 6 (Napalm Beat's splash) |
| Frost Driver | Water damage + freeze + element conversion | Item 1 (freeze), element-conversion is a further open question not covered above |
| Stone Curse | Petrify + element conversion | Item 1 |
| Sight | Reveal hidden in area | Item 4 |
| Safety Wall | Melee-blocking zone | Item 5 |
| Increase SP Recovery | Passive SP regen boost | Same regen-over-time gap as `CLASSES_SWORDMAN.md` item 3, extended to SP |
| Energy Coat | Toggled SP-drain damage reduction | Class-exclusive, see above |

### Merchant
| Skill | Real effect | Depends on |
|---|---|---|
| Mammonite | Melee damage + Zeny cost | Existing Damage system + item 12 |
| Discount / Overcharge | NPC price modifiers | Class-exclusive (NPC shop system, unverified) |
| Enlarge Weight Limit | +carry weight | Class-exclusive, but easy (provider-hook pattern) |
| Item Appraisal | Identify items | Class-exclusive (needs "unidentified" item flag) |
| Pushcart / Vending / Cart Revolution / Cart Decoration / Change Cart / Open Buying Store | Cart + player-shop economy | Class-exclusive, large, own feature pass |
| Crazy Uproar | Party ATK/STR buff | Item 3 |

### Acolyte
| Skill | Real effect | Depends on |
|---|---|---|
| Heal | HP restore, bonus vs. Undead | Existing Heal system + item 8 (Race) for the Undead half |
| Blessing | Flat STR/DEX/INT/Hit, self + party | Item 2 + item 3 |
| Increase AGI | Flat AGI + ASPD% + move speed, self + party | Item 2 + item 3 |
| Angelus | DEF% + flat Max HP, self + party | Item 2 + item 3 |
| Cure | Removes silence/blind/chaos | Item 1 |
| Decrease AGI / Signum Crucis | Chance-based debuff vs. Demon/Undead | Item 7 + item 8 |
| Demon Bane / Divine Protection | Flat bonus vs. Demon/Undead | Item 8 + item 10 (generalized passive payload) |
| Holy Light / Ruwach | Holy damage / reveal + damage | Item 9 (Holy) / item 4 |
| Pneuma | Ranged-blocking zone | Item 5 |
| Teleport / Warp Portal | Fast travel | Class-exclusive, see above |
| Aqua Benedicta | Craft holy water | Class-exclusive, minor |

### Thief
| Skill | Real effect | Depends on |
|---|---|---|
| Double Attack | Proc chance, double damage w/ dagger | Item 7 + weapon-gating already established |
| Improve Dodge | Flat Flee | Item 10 |
| Envenom | Poison damage + poison chance | Existing Damage system + item 9 (Poison element) + item 1 (poison status) + item 7 (chance) |
| Steal | Steal item from monster | Class-exclusive |
| Hiding | Become hidden | Item 4 |
| Detoxify | Cure poison | Item 1 |
| Back Slide | Self-displacement | Item 11 |
| Stone Fling / Sand Attack | Damage + stun/blind chance | Item 1 + item 7 |

### Archer
| Skill | Real effect | Depends on |
|---|---|---|
| Arrow Shower | AoE around target | Item 6 |
| Double Strafe | Single-target damage | Existing Damage system, no gap |
| Arrow Repel | Damage + knockback | Item 11 |
| Arrow Crafting | Craft ammo | Class-exclusive |
| Improve Concentration | DEX/AGI% buff + reveal hidden | Item 2 + item 4 |
| Owl's Eye | Flat DEX | Item 10 |
| Vulture's Eye | Hit/range with bow | Item 10 |
