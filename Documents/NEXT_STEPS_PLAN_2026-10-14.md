# Plano de proximos passos (2026-10-14)

Substitui `NEXT_STEPS_PLAN_2026-10-09.md`. Estado atual em `PROGRESS_UPDATE_2026-10-14.md`. Marcadores: DONE, PARTIAL, OPEN. Ordem dentro de cada fase = prioridade.

## Fase 0 - Fechar pendencias (esta semana)
1. Historico reescrito: confirmar o push forcado do `main` e refazer outros clones/branches. OPEN.
2. Commitar o trabalho acumulado por branch de feature (Sourcetree): UI fixes, anel de alvo, animacao de morte, equipamentos novos. OPEN.
3. Rodar `Tools/Equipment/Build Armor Items` e depois `Apply Item Materials` para as 9 pecas novas do manifesto (Dark, Fisher, Ranger Shirt, Straw Hat, Turban, Villager) e `Archive Processed Sources`. OPEN.
4. Conferir no Editor: botao Novice na cena de criacao, `ClassIconLookup`, Skill Book com 5 colunas sem scroll, Quest Log e Quest Board, animacao de morte (male e female), anel de alvo. PARTIAL (varios confirmados em teste, sem lista formal).
5. Checklist de verificacao do ATK de arma, Magnum Break e evento de XP (secao 7 de `WEAPON_ATK_MAGNUM_BREAK_XP_EVENT_2026-10-10.md`). OPEN.
6. Drop do Wolf: Wolf Fang a 50% adicionado em `WolfStats.asset` (2026-10-14). DONE; falta testar o Novice Trial completo.
7. Documentar as 9 pecas novas no pipeline (origem, cobertura, rig). OPEN.

## Fase 1 - Conteudo (codigo e dados prontos em 2026-10-14, falta rodar no Unity)
Detalhes e passo a passo em `PHASE1_CONTENT_BUILD_2026-10-14.md`.
1. Mapas base: City = `Prototype_Map02` (ja existia), Field = `Prototype_Map01`, **Plains = `Prototype_Map03`**, **Cave = `Prototype_Map04`**. Menu `Tools/Maps/Phase 1/Build Everything` gera floresta, estrada, rochas, portais, spawns e NavMesh da Plains e a Cave inteira em greybox. PARTIAL (scripts escritos, nao executados nem compilados).
2. Iluminacao: shadow strength 0.5 aplicada em Map02, Map03 e Map04 (Map01 ja tinha) e ambiente em Gradient em Map03 e Map04. Neblina leve na Plains e atmosfera escura com tochas na Cave (via menu). DONE nos arquivos; conferir visualmente.
3. Roster: Drops, Marin, Dire Wolf e o boss Alpha Wolf (stats e drops prontos; prefabs criados pelo menu a partir de Poring e Wolf, com tint e escala). PARTIAL.
4. Ciclo Novice: Wolf Fang dropa a 50%. Falta testar do zero (apagar `playersave.json`). PARTIAL.
5. Loot dos inimigos novos: DONE nos assets.

## Fase 2 - Swordman e equipamentos
1. Espadas de duas maos: texturas (Machete, Rapier, GiantBlade, Zweihander), pivos de prefab, icones, fonte em loja ou loot. PARTIAL.
2. Icones de skill (Novice e Swordman primeiro). OPEN.
3. Mostrar multiplicador de velocidade da arma no painel de sub-stats e o custo de HP no tooltip do Magnum Break. OPEN.
4. Conjunto Bartender (prompts prontos, modelo pendente). OPEN.
5. Pendencias de regra: Endure (limite de 7 hits), Fatal Blow (pre-requisito Bash 6, que limita em 5), regen sentado. OPEN.

## Fase 3 - Mage e Archer (decidido)
Segunda fase de classes: **Mage** (14 skills no database, a mais completa depois do Swordman) e **Archer** (7 skills; arco, municao e ataque a distancia ja existem). Para cada uma: detalhar pela pagina da irowiki (`irowiki.org/wiki/Mage` e `Archer`), revisar dados e codigo, icones, equipamento da classe e teste em jogo. Ordem sugerida: Archer primeiro (menos skills, sistemas prontos), depois Mage.
Sistemas compartilhados a resolver antes ou durante, pelo que destravam:
1. Skill de area no chao (ground-targeted AoE): Fire Wall, Thunderstorm, Safety Wall (Mage) e Arrow Shower (Archer).
2. Profundidade de status effects: Stone Curse, Frost Diver (Mage), Sand Attack e afins.
3. Passivas por classe e tabelas de bonus por Job Level (Mage e Archer).
4. MATK de cajado e livro, que ainda nao soma (afeta so o Mage).
5. Fabricacao e tipos de municao (Arrow Crafting) para o Archer.
Merchant, Acolyte e Thief ficam para depois. OPEN.

## Fase 4 - Loop de jogo
Dia/noite com regras de spawn, refino/raridade/slots de carta, mais variedade de quest, polimento de dialogo, estado "MAX LEVEL", curvas reais de Job EXP. OPEN.

## Fase 5 - Divida tecnica
- Reorganizacao da hierarquia do Player (`PLAYER_HIERARCHY_REORGANIZATION.md`): fazer antes de adicionar mais componentes. OPEN.
- AI ainda referencia UI via `BossSkillController` (trocar por evento). OPEN.
- Player como prefab em `Prefabs/Characters`, ativacao de componentes por classe, throttling do raycast de `WorldItemHoverDetector`, DebugTools fora do build, popups de dano com pool, cache de `GetComponent` por ataque, dividir as maiores classes. OPEN.
- Testes unitarios das formulas puras (`SubStatsCalculator`, `HitChanceCalculator`, dano e custo de HP de `SkillDefinition`, `AttackSpeedCalculator`). OPEN, nenhum teste novo desde 10/09.
- `FUTURE_IMPROVEMENTS.md` (156 KB) desatualizado: reescrever em formato curto ou arquivar. OPEN.

## Ordem sugerida
Fase 0 inteira -> mapas base e iluminacao (1.1, 1.2) -> Wolf drop e ciclo Novice (1.4) -> 2 inimigos novos (1.3) -> icones e texturas pendentes (Fase 2) -> Archer e depois Mage completos (Fase 3). Meta de marco: uma **fatia vertical jogavel** (login, Novice, Trial no mapa de planicie, promocao, uma dungeon com boss) que sirva de portfolio para a busca de vaga, com video curto e README atualizado.
