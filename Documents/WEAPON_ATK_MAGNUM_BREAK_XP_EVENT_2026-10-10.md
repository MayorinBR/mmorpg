# Weapon ATK, Magnum Break, XP event, two-hand swords - 2026-10-10

Nothing here was compiled or run in Unity by the assistant. Victor confirmed in play: point-based spawn, Swordman, King Poporing and Two-Handed Sword Mastery working. Everything marked "untested" below still needs an Editor check.

## 1. Weapon ATK (untested)
- `ItemDefinition.weaponAttack` (int, min 0) with `WeaponAttack`. `EquipmentManager.GetMainHandWeaponAttack()` returns the main-hand (LeftHand) item's value, 0 if empty.
- `PlayerStatsController.CurrentSubStats` adds it to Status ATK: `StatusAtk + passive bonus + weapon ATK`, then the buff multiplier. So it shows in the sub-stats panel and feeds basic attacks and every skill that scales from Status ATK.
- Only the main hand counts. The Thief off-hand hit stays `StatusAtk x 0.5`. Staffs and books do not add MATK (Mage basic attack still uses Status MATK only).
- Temporary values (balance by feel):

| Weapon | ATK | Notes |
|---|---|---|
| IronDagger | 17 | |
| IronSword | 25 | |
| Rapier | 26 | |
| Machete | 28 | |
| OakBow | 30 | |
| Katana | 34 | |
| NordicAxe | 38 | |
| Zweihander | 55 | two-hand, Swordman only |
| GiantBlade | 70 | two-hand, Swordman only |
| ArcaneStaff | 10 | |
| AprenticeBook | 8 | |
| TestWeaponOff, TestDualHand | 0 | test items, untouched |

- Existing STR bonuses on weapons were kept, so weapons now differ by ATK plus STR. Revisit if the gap feels too large.

## 2. Two-hand swords and attack speed multiplier (done 2026-10-09, play-tested for mastery)
- GiantBlade: subtype TwoHandSword, STR +6, weight 14, buy 800 / sell 320, `attackSpeedMultiplier` 0.8, Swordman only. Zweihander: STR +5, weight 12, buy 600 / sell 240, multiplier 0.85, Swordman only. Both are heavier and dearer than the one-hand swords.
- `ItemDefinition.AttackSpeedMultiplier` (0.25 to 3, default 1). `AttackSpeedCalculator.GetAttackIntervalSeconds(int aspd, float weaponSpeedMultiplier)` divides the Aspd interval by it and keeps the 0.1 s floor. `PlayerCombatController.GetAttackIntervalSeconds()` reads the main-hand weapon and uses the result for both the cooldown and `SetAttackDuration`, so the swing animation stays in sync.
- The sub-stats panel still shows raw Aspd without the weapon multiplier.
- The `cref` in `EnemyAttackState` and `CharacterStatsDefinition` now names `GetAttackIntervalSeconds(int)` (fixes the CS0419 ambiguity warning).
- Still open: textures for Machete, Rapier, GiantBlade, Zweihander; prefab pivot and rotation checks; the `ReleaseProjectile` event on the two-hand clip; no shop or loot source for these weapons; no Large enemy to exercise the 2H size bonus.

## 3. Magnum Break per iRO Wiki (untested)
Source: irowiki.org/wiki/Magnum_Break, consulted 2026-10-10.

| Item | Before | Now |
|---|---|---|
| Max level | 5 | 10 |
| SP | 15 | 30 |
| Cooldown | 8 s | 2 s |
| Damage | 60% x level | 100% + 20% x level (120% to 300%) |
| Hit bonus | 0 | +10 per level |
| Knockback | 2 | 2 |
| HP cost | none | 20 at levels 1-2, minus 1 every two levels, 16 at levels 9-10 |
| After-effect | none | 10 s, +20% of each hit as Fire damage that ignores DEF |

Code:
- `SkillDefinition`: `damageBaseMultiplier` (damage = base + perLevel x level; zero for every other skill, so they are unchanged), `healthCost`, `healthCostReductionPerTwoLevels`, `GetHealthCost(level)`, `selfFireDamageBonusPercent`. The after-effect duration reuses `buffDurationSeconds` (10).
- `PlayerSkillCaster.TryCastAreaDamage`: refuses the cast if HP would not stay above the cost (the cost never kills), spends mana, then HP via `HealthComponent.TrySpendHealth` (no popup, no flinch), bursts, then applies the Fire buff. If `ownHealth` is not wired the HP cost is skipped.
- `BuffPayload.FireDamagePercent` (last optional constructor argument, summed in `+`), `BuffController.FireDamagePercent`, `PlayerStatsController.FireDamagePercent`.
- `ElementalBonusDamage.Apply(target, hitDamage, percent, element, attacker)` in `Project.Combat` calls `HealthComponent.TakePiercingDamage`, which skips physical and magical defense but still goes through the element modifier and incoming-damage buffs (minimum 1). Applied after the main hit in `PlayerCombatController.DealHit` and in the single-target and area branches of `PlayerSkillCaster`. Zone skills (Fire Wall) do not get it. The first Magnum Break burst does not get its own buff; later hits do.
- Not done: HP cost line in the skill tooltip, Fire damage popup colour.

## 4. XP decoupled from Project.AI (untested)
- `EnemyDeathEvents.ExperienceAwarded(int baseExp, int jobExp)` and `RaiseExperienceAwarded`. `EnemyDeathHandler` now only publishes the scaled rewards; `PlayerExperience` and `PlayerJobProgress` subscribe in `OnEnable` and unsubscribe in `OnDisable`.
- Removed from `EnemyDeathHandler`: `playerExperience`, `playerJobProgress`, `ResolvePlayerProgression` and the `Project.Character.Combat` using. `Project.AI.asmdef` no longer references `Project.Character.Combat`. The AI to UI dependency remains (it comes from `BossSkillController`).
- This replaces the kill-time `FindFirstObjectByType` fix described in `DUMMY_TARGET_AND_XP_FIX_2026-10-09.md`.

## 5. Spawn system
Point-based spawn is in `ENEMY_SPAWN_SYSTEM_PLAN_2026-10-09.md`: children of the `EnemySpawn` object are the points, `maxActive` limits enemies alive, per-monster respawn delay (default 30 s) and a random free point on respawn. In `Prototype_Map01` the entries must be Project prefabs, not scene instances.

## 6. Statements in FUTURE_IMPROVEMENTS.md superseded by this document
`FUTURE_IMPROVEMENTS.md` was last edited 2026-09-28 and is not rewritten here. These lines are out of date:
- "Still open - weapon's own stats" (real damage formula section): done, see section 1.
- Boss backlog bullet ("spawn in place of a regular creature"): the spawn area now supports a boss prefab with `bossChance` and `maxBosses`.
- Magnum Break notes in the Swordman detail pass: see section 3.

## 7. Checks to run in the Editor
1. Console free of errors; Unity creates the `.meta` for `ElementalBonusDamage.cs`.
2. Kill a Poring: base and job XP both rise.
3. Compare Giant Blade, Zweihander and Iron Sword in the sub-stats panel and in damage.
4. Magnum Break: HP drops by the cost, burst hits, then 10 s of extra Fire hits; at very low HP the cast is refused.
5. EditMode tests pass (`SpawnRulesTests` and the existing suites).
