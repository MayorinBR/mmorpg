# Swordman — Class Reference

Source: [iRO Wiki Classic — Swordman](https://irowiki.org/wiki/Swordman), consulted September 2026. This document is the first of a planned per-class series (see `MMO_Project_GDD.docx`, Section 10.1) — it exists to keep the real Ragnarok Online reference and this project's own implementation status next to each other, so future class work doesn't need to re-derive it.

## Overview

Base class, no promotion yet available in this project (class evolution — e.g. Swordman to Knight/Crusader — is tracked as planned but undesigned; see `PlayerClassController.ChangeClass`). In classic Ragnarok Online, Swordman is a Human-only class that job-changes from Novice at the Izlude guild, with a Job Level cap of 50. Design identity: a durable melee fighter built around high ATK and HP rather than evasion — "offense as defense."

Currently implemented in this project: the basic-attack block chance (`PlayerBlockController`, an `IDamageModifier` hook — 25% chance, full negation by default, tunable per prefab), the Bash, Magnum Break, Sword Mastery and Two-Handed Sword Mastery skills below, and the Job Level 50 stat bonus (see below).

## Stat Bonuses at Job Level 50

| STR | AGI | VIT | INT | DEX | LUK |
|-----|-----|-----|-----|-----|-----|
| +7  | +2  | +4  | +0  | +3  | +2  |

**Implemented this pass.** `ClassJobLevelBonusLookup.asset` grants this flat bonus automatically once Swordman reaches Job Level 50, folded into every derived stat by `JobBonusStatsView` (mirrors how `EquippedStatsView` folds in equipment bonuses) — separate from normal spendable stat points, matching real Ragnarok Online. Real RO grants this incrementally across multiple Job Level milestones; this project only has the final Job 50 total documented, so the bonus is a single cliff-edge grant at Job 50 rather than a staged curve — add intermediate breakpoints if the real per-level schedule gets sourced later.

## Recommended Builds (reference only)

Two builds from the wiki, at character Level 40, useful as tuning targets once itemization and skill balance are revisited:

| Build | STR | AGI | VIT | INT | DEX | LUK |
|-------|-----|-----|-----|-----|-----|-----|
| Magnum Break | 40 | 1 | 30 | 16 | 20 | 1 |
| Bash & Auto-Attack | 26 | 50 | 1 | 1 | 20 | 1 |

## Skill Roster

| Skill | Real type / max level | Real effect | Status in this project |
|---|---|---|---|
| **Bash** | Offensive, Lv 1–10 | ATK ×130–400%, +5–50% accuracy | **Implemented.** Single-target physical damage (`Bash.asset`), max level 5 (project-wide skill level cap, not RO's 10). Now also grants a level-scaling Hit-rating bonus on its own damage roll (`SkillDefinition.GetAccuracyBonus`), matching the real accuracy bonus — tuned to reach the wiki's +50 upper value at this project's max level. |
| **Magnum Break** | Offensive, Lv 1–10 | Fire AoE, ATK ×120–300% to a 5×5 area | **Implemented this pass.** New `MagnumBreak.asset`: Fire-element physical damage to every living target within a radius of the caster, needing no pre-selected target at all (`SkillDefinition.IsAreaOfEffect` / `AreaRadius`, `PlayerSkillCaster.TryCastAreaDamage`) — this project's first area-of-effect skill. Damage curve tuned to reach the wiki's 300% upper value at this project's max level (5). No icon yet (see the shared skill-icon sprite sheet gap already tracked in the GDD). |
| **Provoke** | Active, Lv 1–10 | +5–32% target ATK, −10–55% target DEF (a taunt/debuff) | **Not implemented** — needs a buffs/debuffs system (temporary, timed stat modifiers on a target). Already tracked as planned-but-unbuilt in the GDD (Section 4.5). |
| **Endure** | Active, Lv 1–10 | Resists flinching for up to 7 hits / 10–37s, +1–10 MDEF | **Not implemented** — needs the same buffs/debuffs system as Provoke (self-targeted this time), plus a hit-flinch/stagger mechanic that doesn't exist in this project at all today. |
| **Increase HP Recovery** | Passive, Lv 1–10 | Boosts natural HP regen and healing-item effectiveness by 10–100% | **Not implemented** — the passive-skill mechanic itself now exists (`SkillEffectType.Passive`, see Sword Mastery below), but this specific skill also needs a base HP-regen-over-time system, which still doesn't exist: `HealthComponent` only ever changes HP in response to `TakeDamage`/`Heal` calls, never on a timer. |
| **Sword Mastery** | Passive, Lv 1–10 | +4–40 ATK with swords and daggers | **Implemented this pass.** New `SwordMastery.asset`: a `SkillEffectType.Passive` skill (`PlayerPassiveSkillController` sums it into `PlayerStatsController.CurrentSubStats.StatusAtk` whenever learned) that only applies while the equipped main-hand weapon's new `ItemDefinition.WeaponSubtype` is Dagger or One-Hand Sword (`SkillDefinition.PassiveRequiredWeaponSubtypes`). Never appears on the hotbar — a passive skill is never cast. Bonus tuned to reach the wiki's 40 ATK upper value at this project's max level (5, so +8/level). |
| **Two-Handed Sword Mastery** | Passive, Lv 1–10 | +4–40 ATK with two-handed swords | **Implemented this pass.** New `TwoHandSwordMastery.asset`, identical mechanism to Sword Mastery above, gated on `WeaponSubtype.TwoHandSword` instead. `TestTwoHandSword.asset` now exists to equip and test it against — still a generic test item, not a curated itemization pass. |
| **Berserk** (quest skill, Job Lv 30) | Passive | +32% ATK, −55% DEF while under 25% HP | **Not implemented** — the passive-skill mechanic exists now, but this skill also needs an HP-threshold conditional trigger, which doesn't exist yet. |
| **Fatal Blow** (quest skill, Job Lv 30) | Passive | Bash (Lv 6–10) gains a chance to stun | **Not implemented** — needs a stun/status-effect system; this project has no status-effect system of any kind yet. |
| **HP Recovery While Moving** (quest skill, Job Lv 35) | Passive | Removes the usual HP-regen-while-moving penalty | **Not implemented** — same missing regen system as Increase HP Recovery. |

Weapon-vs-monster-size damage scaling (daggers 100%/75%/50% vs. small/medium/large, one-handed swords 100%/75%/75% vs. medium/small+large, two-handed swords 100%/75%/75% vs. large/small+medium) is also real-RO Swordman behavior, but is a separate, broader gap: this project has no monster Size classification at all yet, for any class — see item 5 below, now narrowed to just that.

## Missing Systems (grouped — one build unblocks several skills at once)

1. ~~Passive skill effects~~ — **implemented this pass.** `SkillEffectType` gained a `Passive` value (never cast — `PlayerSkillCaster.TryCastSkill` and the Skill Book's hotbar drag both refuse it); `PlayerPassiveSkillController` sums every learned passive's bonus on demand. Sword Mastery and Two-Handed Sword Mastery are the first skills to use it. Still doesn't unblock Increase HP Recovery, HP Recovery While Moving or Berserk on its own — see items 2 and 3 below, each still needed for those.
2. **Buffs/debuffs** — temporary, timed stat modifiers on self or a target. Blocks Provoke and Endure. Already noted as planned in the GDD (Section 4.5), alongside ground-targeted no-fixed-target skills and full elemental resistances.
3. **HP regen-over-time** — a base "HP recovers gradually while idle/moving" system. Blocks Increase HP Recovery and HP Recovery While Moving. Doesn't exist at all today, for any class.
4. **Status effects** (stun, etc.) — blocks Fatal Blow. No status-effect system exists yet.
5. ~~Weapon subtypes~~ **+ monster Size** — **weapon subtypes implemented**, now covering every type on the [iRO Wiki weapon list](https://irowiki.org/wiki/Weapons): `ItemDefinition.WeaponSubtype` has Dagger, One/Two-Hand Sword, One/Two-Hand Axe, Mace, Spear, Bow, Book, Instrument, Whip, Rod, Knuckle, Katar, Gun and Huuma Shuriken, plus a project-specific `Shield` value for off-hand equipment with no attack of its own. Shield needed no new code: `EquipmentManager`'s existing slot-eviction logic already displaces a shield with a two-handed weapon and vice versa, the same way it already displaces one off-hand weapon with another. This unblocked Sword Mastery vs. Two-Handed Sword Mastery. The broader weapon-vs-monster-size damage table is still blocked, now solely by the missing monster Size classification (no enemy in this project has one yet).
6. ~~Class-specific Job Level stat bonuses~~ — **implemented this pass.** `ClassJobLevelBonusLookup` + `JobBonusStatsView` (see "Stat Bonuses at Job Level 50" above). Only Swordman's Job 50 entry is populated; add one per class as their real stat tables get researched.

Items 2-4 and the monster Size half of item 5 aren't Swordman-specific — each will very likely come up again for the other five classes, so building any one of them now front-loads work rather than only unblocking Swordman.
