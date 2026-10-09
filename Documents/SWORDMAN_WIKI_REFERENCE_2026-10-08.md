# Swordman: valores e formulas da iRO Wiki vs implementacao (2026-10-08)

Fontes (consultadas em 2026-10-08): iRO Wiki (Renewal) e iRO Wiki Classic: Swordman, Bash, Magnum Break, Provoke, Endure, Increase HP Recovery, Sword Mastery, Two-Handed Sword Mastery, Berserk, Fatal Blow, HP Recovery While Moving, Swordman Skill Quest, Status Effects, Size, Weight Limit, Job Level, Stats, ATK, DEF, MDEF, ASPD, HP Recovery, SP Recovery, Max HP, Max SP.

Status: levantamento feito lendo os assets e scripts do repo (Bash.asset, Provoke.asset, PlayerHealthRegenController, WeaponSizeModifiers, PlayerStatsController etc.). **Nada foi alterado no codigo ou nos assets**, so documentacao. Codigo ainda nao compilado/executado no Unity nesta sessao.

Convencao do projeto: nivel maximo de skill = 5 (RO = 10). Valores da wiki no nivel 10 sao comprimidos linearmente para o nivel 5. `SkillDefinition.CalculateDamage = ATK x damageMultiplierPerLevel x nivel` (sem termo base), por isso o nivel 1 sempre fica abaixo do da wiki.

---

## 1. Bugs e divergencias que mudam comportamento

| # | Onde | Problema | Correcao proposta |
|---|---|---|---|
| 1 | `EnlargeWeightLimit.asset` + `PlayerStatsController` linha ~335 | O bonus de skill (`passiveWeightLimitBonusPerLevel: 1000`, +5000 no nivel 5) e somado cru, mas a capacidade base e multiplicada por `carryWeightScale` (0.05). Base de um Swordman com STR baixo fica perto de 140; o bonus sozinho e 5000. Wiki: +200 por nivel (+2000 no nivel 10). | Escalar o bonus pelo mesmo `carryWeightScale` e usar 400 por nivel (2000 no nivel 5): bonus efetivo 20 por nivel. |
| 2 | `Bash.asset` | `damageMultiplierPerLevel: 1` da 100% x nivel: 500% no nivel 5. Wiki chega a 400% no nivel 10. | `damageMultiplierPerLevel: 0.8` (400% no nivel 5, 80% no nivel 1 contra 130% da wiki). |
| 3 | `PlayerHealthRegenController` | Intervalo 10s e 1% do HP maximo (inventados). Wiki: a cada 6s em pe, 3s sentado; quantidade = max(1, floor(MaxHP/200)) + floor(VIT/5). | Ver secao 4. |
| 4 | `PlayerManaRegenController` | Intervalo 10s e 1% da mana maxima (inventados). Wiki: 8s em pe, 4s sentado; 1 + floor(MaxSP/100) + floor(INT/6). | Ver secao 4. |
| 5 | Regen em movimento | Projeto: sem HP Recovery While Moving a regen cai para 50%; com a skill volta a 100%. Wiki: sem a skill nao ha regen em movimento; com a skill e metade da taxa em pe (intervalo de 12s). | Sem skill: 0 em movimento. Com skill: 50% da taxa em pe. |
| 6 | `WeaponSizeModifiers` | Knuckle esta 100/75/50 (Small/Medium/Large); wiki 100/100/75. Huuma Shuriken ausente (cai em 100% sempre); wiki 75/75/100. | Corrigir a linha do Knuckle e adicionar Huuma Shuriken. |
| 7 | `Increase HP Recovery` | Implementada como multiplicador (+20% por nivel) sobre a regen natural. Na wiki e bonus fixo mais porcentagem do HP maximo (secao 3.5), e so vale parado. | Ver secao 3.5. |
| 8 | Job bonus de Job Level 50 | Entregue num salto unico no Job 50. A wiki lista o nivel de Job exato de cada +1. | Ver secao 2 (tabela pronta para virar assets). |
| 9 | Documentacao: "nao existe caminho de usar consumivel" | **Desatualizado.** `PlayerConsumableUser.TryConsume` existe (restaura HP/MP instantaneo ou ao longo do tempo, remove 1 unidade, limpa debuffs se `CuresPoison`). | A metade "bonus em itens de cura" de Increase HP Recovery deixou de estar bloqueada. |
| 10 | Documentacao: "flinch = interrupcao de cast" | A wiki classica descreve Endure como remover "o atraso de ataque e movimento que normalmente ocorre quando o personagem recebe dano" (hit delay / damage motion), nao interrupcao de conjuracao. Nem a pagina Renewal nem a Classic citam contagem de 7 hits. | O pre-requisito real e um sistema de hit-reaction (travar ataque/movimento por instantes ao receber dano), nao cast time. Remover a afirmacao "7 hits" ate achar fonte. |

---

## 2. Bonus de Job Level do Swordman (curva completa)

Fonte: pagina Swordman, tabela "Job Bonuses". A pagina nao rotula as colunas; interpretacao: cada numero e o Job Level em que a stat ganha +1. Os totais batem com o resumo da pagina (STR +7, AGI +2, VIT +4, INT +0, DEX +3, LUK +2), e com o que o projeto ja usa no Job 50.

| Stat | +1 no Job Level |
|---|---|
| STR | 2, 14, 33, 40, 47, 49, 50 |
| AGI | 30, 46 |
| VIT | 6, 18, 38, 42 |
| INT | nenhum |
| DEX | 10, 22, 36 |
| LUK | 26, 44 |

Bonus acumulado por faixa de Job Level (para um `ClassBonusEntry` por faixa em `ClassJobLevelBonusLookup`, que hoje so aceita um unico `requiredJobLevel`):

| Job Level | STR | AGI | VIT | DEX | LUK |
|---|---|---|---|---|---|
| 2 | 1 | 0 | 0 | 0 | 0 |
| 6 | 1 | 0 | 1 | 0 | 0 |
| 10 | 1 | 0 | 1 | 1 | 0 |
| 14 | 2 | 0 | 1 | 1 | 0 |
| 18 | 2 | 0 | 2 | 1 | 0 |
| 22 | 2 | 0 | 2 | 2 | 0 |
| 26 | 2 | 0 | 2 | 2 | 1 |
| 30 | 2 | 1 | 2 | 2 | 1 |
| 33 | 3 | 1 | 2 | 2 | 1 |
| 36 | 3 | 1 | 2 | 3 | 1 |
| 38 | 3 | 1 | 3 | 3 | 1 |
| 40 | 4 | 1 | 3 | 3 | 1 |
| 42 | 4 | 1 | 4 | 3 | 1 |
| 44 | 4 | 1 | 4 | 3 | 2 |
| 46 | 4 | 2 | 4 | 3 | 2 |
| 47 | 5 | 2 | 4 | 3 | 2 |
| 49 | 6 | 2 | 4 | 3 | 2 |
| 50 | 7 | 2 | 4 | 3 | 2 |

Codigo: o lookup atual retorna um unico bonus se `jobLevel >= requiredJobLevel`. Para a curva e preciso trocar por uma lista de breakpoints ordenada (bonus = ultimo breakpoint com `requiredJobLevel <= jobLevel`) ou somar deltas.

Outras regras de Job Level (pagina Job Level): primeira classe tem cap 50; cada Job Level da 1 skill point; trocar de classe no nivel 50 da +1 skill point por nivel acima de 40.

---

## 3. Skills

### 3.1 Bash

Wiki (Renewal; Classic difere em 3 niveis, ver nota):

| Nivel | Dano (ATK) | Bonus de precisao | Chance de stun (Fatal Blow) | SP |
|---|---|---|---|---|
| 1 | 130% | +5% | - | 8 |
| 2 | 160% | +10% | - | 8 |
| 3 | 190% | +15% | - | 8 |
| 4 | 220% (Classic 210%) | +20% | - | 8 |
| 5 | 250% (Classic 240%) | +25% | - | 8 |
| 6 | 280% (Classic 270%) | +30% | 5% | 15 |
| 7 | 310% (Classic 300%) | +35% | 10% | 15 |
| 8 | 340% (Classic 330%) | +40% | 15% | 15 |
| 9 | 370% (Classic 360%) | +45% | 20% | 15 |
| 10 | 400% | +50% | 25% | 15 |

- Dano renewal: 100% + 30% x nivel.
- Precisao: porcentagem **do Hit do atacante somada ao proprio Hit** (exemplo da wiki: 67% de chance de acerto com +50% vira 67 x 1.5 = 100%). O projeto soma um valor flat ao Hit (`accuracyBonusPerLevel: 10`), aproximacao diferente.
- Stun: so ocorre com Fatal Blow aprendida. "Base Level aumenta a chance" (as duas wikis dizem isso sem numeros). **Duracao do stun nao e citada em nenhuma pagina**; os 3s do projeto continuam sendo placeholder.
- Projeto (nivel 5 = nivel 10 da wiki): dano proposto `damageMultiplierPerLevel: 0.8`; SP proposto 8 (hoje 10 flat); stun 5% x nivel de Bash (ja correto).

### 3.2 Magnum Break

| Nivel | Dano (ATK) | Bonus de Hit | Custo de HP |
|---|---|---|---|
| 1 | 120% | +10% | 20 |
| 3 | 160% | +30% | 19 |
| 5 | 200% | +50% | 18 |
| 7 | 240% | +70% | 17 |
| 9 | 280% | +90% | 16 |
| 10 | 300% | +100% | (infobox: 20 a 16) |

- SP fixo 30 (projeto: 15). Area 5x5 em volta do usuario. Knockback de 2 celulas.
- Buff de propriedade Fogo: 10 segundos, +20% de dano de Fogo em todos os ataques (nao da propriedade a arma).
- Exemplo da wiki no nivel maximo: 250% + 20% x 250% = 300% de dano fisico, com 1/6 desse valor sendo dano de Fogo que ignora DEF.
- Projeto hoje: `damageMultiplierPerLevel: 0.6` (300% no nivel 5, correto), `manaCost: 15`, **sem** bonus de Hit (`accuracyBonusPerLevel: 0`), **sem** custo de HP, **sem** knockback (existe `KnockbackUtility`/`knockbackDistance`, so falta configurar), **sem** buff de Fogo.
- Proposto: `accuracyBonusPerLevel: 20` (100 no nivel 5), `knockbackDistance` para 2 celulas, `manaCost: 30`; custo de HP exige campo novo em `SkillDefinition` (`hpCost`).

### 3.3 Provoke

| Nivel | Queda de DEF | Aumento de ATK | Taxa de sucesso | SP |
|---|---|---|---|---|
| 1 | 10% | 5% | 53% | 4 |
| 2 | 15% | 8% | 56% | 5 |
| 3 | 20% | 11% | 59% | 6 |
| 4 | 25% | 14% | 62% | 7 |
| 5 | 30% | 17% | 65% | 8 |
| 6 | 35% | 20% | 68% | 9 |
| 7 | 40% | 23% | 71% | 10 |
| 8 | 45% | 26% | 74% | 11 |
| 9 | 50% | 29% | 77% | 12 |
| 10 | 55% | 32% | 80% | 13 |

- Duracao 30s (nao escala). SP = nivel + 3. Contra jogadores reduz so a DEF "soft".
- **Imunes: monstros Undead (propriedade) e Boss.**
- Formulas: queda de DEF = 5% + 5% x nivel; ATK = 2% + 3% x nivel; sucesso = 50% + 3% x nivel.
- Projeto: `buffAtkPercentPerLevel: 0.064` e `buffDefPercentPerLevel: -0.11` chegam aos valores do nivel 10 no nivel 5 (bom), duracao 30 (correto), mas **sem taxa de sucesso** (`successChancePerLevel` nao definido, entao sempre acerta) e sem imunidade Undead/Boss (o projeto ja tem `MonsterRace` e `IsBoss`). SP fixo 10 (wiki 4 a 13).
- Proposto: `successChancePerLevel: 0.16` (80% no nivel 5); checar `Race == Undead` e `IsBoss` em `TryCastBuff` ramo Enemy.

### 3.4 Endure

| Nivel | Duracao | MDEF |
|---|---|---|
| 1 | 10s | +1 |
| 2 | 13s | +2 |
| 3 | 16s | +3 |
| 4 | 19s | +4 |
| 5 | 22s | +5 |
| 6 | 25s | +6 |
| 7 | 28s | +7 |
| 8 | 31s | +8 |
| 9 | 34s | +9 |
| 10 | 37s | +10 |

- Duracao = 7 + 3 x nivel segundos. MDEF = nivel. SP 10 fixo. Cooldown 10s.
- Projeto: `buffDurationPerLevel: 7.4` (37s no nivel 5), `buffMdefBonusPerLevel: 2` (10), SP 10 e cooldown 10: tudo coerente.
- Efeito de flinch: ver secao 1, item 10. Precisa de sistema de hit-reaction no projeto.

### 3.5 Increase HP Recovery

| Nivel | Regen base (HP a cada 10s) | Bonus de porcentagem | Bonus em itens de cura |
|---|---|---|---|
| 1 | 5 | 0.2% | 10% |
| 5 | 25 | 1.0% | 50% |
| 10 | 50 | 2.0% | 100% |

- Formulas (inferidas da tabela): regen = 5 x nivel; porcentagem = 0.2% x nivel (a pagina nao diz sobre o que incide; a leitura natural e o HP maximo); itens = +10% x nivel, **cumulativo com o bonus de VIT**.
- Acontece a cada 10s **enquanto o usuario nao se move** (continua valendo atacando parado na mesma celula). Nao cura se a regen natural estiver bloqueada (sobrepeso). Nao se acumula com HP Recovery While Moving: o bonus extra so entra quando para ou senta.
- Projeto: `passiveRegenMultiplierPerLevel: 0.2` multiplica a regen natural (100% no nivel 5). Sugestao: tratar como segundo tick de 10s parado com `HP = 5 x nivel x 2 + floor(MaxHP x 0.002 x nivel x 2)` (niveis comprimidos), e multiplicar `HealthRestore` em `PlayerConsumableUser.RestoreOnce` por `1 + 0.2 x nivel` (100% no nivel 5).

### 3.6 Sword Mastery e Two-Handed Sword Mastery

ATK bonus = 4 x nivel (4 a 40). Sword Mastery vale para Dagger e One-Hand Sword; Two-Handed vale para Two-Hand Sword. Nao se aplica a skills que ignoram ATK da arma ou masteries. Projeto: 8 por nivel ate o nivel 5 (40), correto. `passiveRequiredWeaponSubtypes` Sword Mastery = Dagger(1)+OneHandSword(2); TwoHand = 3. Correto.

### 3.7 Quest skills

| Skill | Job Level | Tipo / efeito | Pedido da quest (itens consumidos) | NPC / local |
|---|---|---|---|---|
| Fatal Blow | 30 (e Bash nivel 6, pagina da skill) | Passiva: Bash nivel 6 a 10 pode atordoar | 1 Banana Juice, 10 Fire Arrow, 10 Silver Arrow, 30 Tentacle, 5 Royal Jelly | Leon Von Frich, Prontera Chivalry (prt_in 75/88) |
| Berserk | 30 | HP < 25% do maximo: ATK +32%, DEF -55% (equivale a Provoke nivel 10); dura ate o HP voltar acima de 25% | 10 Decayed Nail, 10 Honey, 10 Horrendous Mouth, 35 Butterfly Powder | Juan, Prontera Upgrade Shop (prt_in 94/57) |
| HP Recovery While Moving | 35 | Passiva: ativa regen natural em movimento | 200 Empty Bottle, 1 Moth Wing (consumidos); 1 Padded Armor (nao consumido) | Knight De Thomas, Swordsman Guild (izlude_in 117/172) |

Nenhuma lista Zeny, Base Level minimo ou skills pre-requisito alem do citado. Nota: a pagina Renewal de Berserk lista como ativa (1 SP, nao pode ser usada com 0 SP, liga/desliga); a pagina Swordman e a Classic dizem passiva. O projeto segue a versao passiva (gatilho automatico a HP < 25%). O codigo usa `HpThresholdRatio = 0.25f` (o texto do asset diz "25% ou menos"; a wiki diz "abaixo de 25%").

Modelagem do gate de Job Level: `PlayerJobProgress.JobLevel` ja existe e e lido pelo bonus de stats. Falta um campo `requiredJobLevel` em `SkillDefinition` e a checagem em `PlayerSkillBook.TryLearn`. Itens/NPCs da quest ficam fora do escopo do prototipo.

---

## 4. Formulas de recuperacao natural

HP (pagina HP Recovery):
- Intervalo: 6s em pe, 3s sentado, 12s andando com HP Recovery While Moving.
- Quantidade: `max(1, floor(MaxHP / 200)) + floor(VIT / 5)`, depois multiplicado por `(1 + soma dos modificadores)`. Cada passo e arredondado para baixo.
- Sem a skill: nenhuma regen em movimento. Sobrepeso 70%+: sem regen (ja implementado).

SP (pagina SP Recovery):
- Intervalo: 8s em pe/andando, 4s sentado.
- `SPR = 1 + floor(MaxSP / 100) + floor(INT / 6)`; depois `floor(SPR x (1 + SPR_MOD x 0.01))`; se INT >= 120, soma `4 + floor(INT / 2 - 60)`.
- Veneno desliga a regen de SP.

Max HP (pagina Max HP):
1. `base = 35 + BaseLv x HP_JOB_B` (B = 5 se nao listado)
2. para cada nivel de 2 ate BaseLv: `base += round(HP_JOB_A x nivel)`
3. `base = floor(base x (1 + 0.01 x VIT))` (x1.25 se transcendente)
4. soma modificadores aditivos, depois `floor(x (1 + 0.01 x modificadores multiplicativos))`
- HP_JOB_A do Swordman: 0.7. Archer 0.5, Thief 0.5, Acolyte 0.4, Merchant 0.4, Mage 0.3.

Max SP (pagina Max SP):
1. `base = 10 + BaseLv x SP_JOB`
2. `floor(base x (1 + 0.01 x INT))`, depois x1.25 se transcendente, soma aditivos, multiplica `(1 + 0.01 x mods)`
- SP_JOB: Swordman 2, Archer 2, Thief 2, Acolyte 5, Merchant 3, Mage 6.

Conferir no codigo: formulas de MaxHP/MaxSP em `PlayerStatsController`/`CharacterStatsDefinition` (nao comparadas nesta passada).

---

## 5. Combate (nao apenas Swordman)

Hit/Flee (pagina Stats):
- `HIT = 175 + BaseLv + DEX + floor(LUK/3) + bonus`
- `FLEE = 100 + BaseLv + AGI + floor(LUK/5) + bonus`
- Contra monstros: chance = `100 + Hit - (200 + NivelMonstro + AGIMonstro)`; minimo 5%. Dodge maximo 95%.
- Projeto: `hit = baseLevel + dex`, `flee = agi + baseLevel`, chance = `clamp(100 + hit - flee, 5, 95)`. As constantes 175 e 100 nao existem, entao o equilibrio so e coerente se os valores de Hit/Flee dos monstros forem definidos na mesma escala.
- Critico: taxa real = `LUK x 0.3 + bonus` (projeto soma +1).

Status ATK (pagina ATK, versao Renewal):
- Corpo a corpo: `floor(BaseLv/4) + STR + floor(DEX/5) + floor(LUK/3)`; distancia: `floor(BaseLv/4) + floor(STR/5) + DEX + floor(LUK/3)`.
- Projeto (formula pre-Renewal): `STR + (STR/10)^2 + LUK/5`, sem DEX/5 no melee. Decidir qual era seguir; a Classic usa a formula com quadrado.
- Bonus de arma por stat: `ArmaBase x STR / 200` (DEX/200 para arco etc.). Penalidade de tamanho multiplica o ATK da arma.

DEF (pagina DEF):
- DEF dura: `dano x (4000 + DEF) / (4000 + DEF x 10)`. DEF mole (jogador): `floor(VIT/2 + AGI/5 + BaseLv/2)`; (monstro) `floor((VIT + BaseLv)/2)`. Dura primeiro, depois mole subtrai.
- Projeto: `statusDef = (int)(vit x 0.8)`.

MDEF (pagina MDEF):
- MDEF dura: `dano x (1000 + MDEF) / (1000 + MDEF x 10)`. MDEF mole (jogador): `INT + VIT/5 + DEX/5 + BaseLv/4` (igual ao projeto).

ASPD (pagina ASPD): formula completa em 5 passos (penalidade de job, correcao `(sqrt(205) - sqrt(AGI))/7.15`, `sqrt(AGI x 9.999 + DEX x 0.19212)`), cap 190. Hits por segundo = `50 / (200 - ASPD)`. Projeto usa pesos lineares (AGI 0.25, DEX 0.1) e cap 190. Valores de ASPD base por arma do Swordman nao estao nas paginas lidas.

---

## 6. Tamanho de monstro x arma (pagina Size)

| Arma | Small | Medium | Large | Projeto |
|---|---|---|---|---|
| Bare Handed | 100 | 100 | 100 | igual (default) |
| Dagger | 100 | 75 | 50 | igual |
| 1H Sword | 75 | 100 | 75 | igual |
| 2H Sword | 75 | 75 | 100 | igual |
| Spear | 75 | 75 | 100 | igual |
| Spear (montado em Peco/Gryphon) | 75 | 100 | 100 | nao modelado |
| Spear (montado em Dragon) | 100 | 100 | 100 | nao modelado |
| Axe | 50 | 75 | 100 | igual |
| Mace | 75 | 100 | 100 | igual |
| Rod | 100 | 100 | 100 | igual (default) |
| Bow | 100 | 100 | 75 | igual |
| Katar | 75 | 100 | 75 | igual |
| Book | 100 | 100 | 50 | igual |
| Knuckle | 100 | 100 | 75 | **diferente** (100/75/50) |
| Instrument | 75 | 100 | 75 | igual |
| Whip | 75 | 100 | 50 | igual |
| Gun | 100 | 100 | 100 | igual (default) |
| Huuma Shuriken | 75 | 75 | 100 | **ausente** |

As novas espadas (Machete, Rapier = One-Hand Sword; GiantBlade, Zweihander = Two-Hand Sword) herdam essas linhas automaticamente.

---

## 7. Peso (pagina Weight Limit)

```
MAX_WGT = 2000
MAX_WGT += 30 * BASE_STR
MAX_WGT += WGT_JOB
MAX_WGT *= 1 + WGT_MULT
MAX_WGT += WGT_MOD
```

- Swordman 800, Merchant 800, Archer 600, Thief 400, Acolyte 400, Mage 200, Novice 0 (todos conferem com `WeightCapacityCalculator`).
- Enlarge Weight Limit: +200 por nivel (soma em `WGT_MOD`). Peco Peco: +1000. Dragon Training +300 por nivel. Carrinho nao aumenta o limite.
- 70% ou mais: sobrepeso leve: cancela HP Recovery, SP Recovery, Increase HP Recovery e Increase SP Recovery; bloqueia skills de criacao (Find Stone, Arrow Crafting). Implementado.
- 90% ou mais: sobrepeso grave: nao ataca nem usa skills. Implementado.
- Conferido: a pagina nao tem limiar de 50%.

---

## 8. Status Stun (e afins)

Nem a pagina Renewal nem a Classic de Status Effects trazem duracao ou formula de chance do Stun.
- Stun (Classic): flee e anulado; impossivel mover, atacar, pegar itens, usar skills, sentar ou usar itens. VIT reduz muito a duracao, LUK pouco. Base Level reduz levemente a chance. Imunidade: Orc Hero Card.
- Silence: nao usa skills ativas. VIT e LUK reduzem chance e duracao.
- Blind: Hit e Flee -25% (o projeto ainda nao le `IsBlinded`, entao esse valor agora esta sourced).
- Poison: DEF "status" -25%; se HP acima de 25%, perde 1.5% do HP maximo + 2 por segundo; desliga regen de SP.
- Freezing (Renewal): base 40s menos `(VIT + DEX)/20` s, minimo 10s.

Duracoes de Stun/Silence continuam sem fonte.

---

## 9. Pendencias que continuam sem valor na wiki

- Duracao do stun do Fatal Blow; escala de Base Level da chance de stun.
- Curva de atraso de ataque/movimento ao levar dano (damage motion) para Endure.
- Contagem de hits do Endure (nenhuma das duas paginas cita).
- ASPD base de cada arma do Swordman.
- Duracoes e chances de Stun/Silence por VIT/LUK.
