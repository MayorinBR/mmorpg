# Plano de proximos passos (2026-10-21)

Substitui `NEXT_STEPS_PLAN_2026-10-16.md`. Marcadores: DONE, PARTIAL, OPEN. Ordem dentro de cada fase = prioridade.

## Onde estamos
- Target rings por prefab (hover branco, alvo vermelho) funcionando em todos os mapas. Ver `TARGET_RING_MARKERS_2026-10-21.md`. DONE.
- Restyle de Login, CharacterSelection e MainMenu: IDs de objeto fora do intervalo de 64 bits corrigidos em Login e MainMenu; falta a checagem visual em 1920x1080 e outro aspecto. PARTIAL.
- Archer e Mage fechados em codigo e dados, ainda nao testados em jogo. Boss da Cave: Skeleton Warrior (substitui o Alpha Wolf do plano anterior).
- Muitas mudancas sem commit desde 14/10 (world map, Archer/Mage, restyle, rings, Phase 1).

## Fase A - Validar e proteger (1 sessao)
1. Commits por branch de feature no Sourcetree: world map, Archer/Mage, restyle de UI, target rings, Phase 1. OPEN.
2. Console limpo; limpar os avisos conhecidos: `StrongHeal` com nome interno `Heal`, LightingData orfao, `New Terrain.asset` e `TerrainData_*.asset` corrompidos do Low Poly Tavern Pack. OPEN.
3. Apagar restos dos rings antigos (ver o doc dos rings). OPEN.
4. Teste em jogo do Archer e do Mage (lista em `ARCHER_MAGE_PHASE3_START_2026-10-15.md`). OPEN.
5. Rodar os menus de Editor pendentes: Stock Shops, Create Mage Gemstones, Build Armor Items, Apply Item Materials, Archive Processed Sources, Phase 1 Build Everything. OPEN.

## Fase B - Fechar o world map
1. Capturar miniaturas com `MapThumbnailArea` em cada cena (Cave: camera abaixo do teto). OPEN.
2. Conferir tooltip e marcador do mapa atual. OPEN.

## Fase C - Feedback e polimento de combate
1. Barra de cast na UI (`GetCastTime`/`IsCasting` ja existem). OPEN.
2. Icones de skill: Novice, Swordman, Archer, Mage. OPEN.
3. Tooltip do Magnum Break com custo de HP; multiplicador de velocidade da arma nos sub-stats. OPEN.
4. Efeitos visuais simples das skills. OPEN.

## Fase D - Conteudo da fatia vertical
1. Ciclo Novice completo do zero (apagar `playersave.json`). PARTIAL.
2. Roster novo testado, com loot. PARTIAL.
3. Cave jogavel com o Skeleton Warrior e portais de volta. PARTIAL.
4. Espadas de duas maos: texturas, pivos, icones, fonte em loja ou loot; `Zweihander.fbx` tem um poligono descartado no import. PARTIAL.
5. Regras do Swordman: Endure (7 hits), Fatal Blow (pre-requisito), regen sentado. OPEN.

## Fase E - Divida tecnica
1. Reorganizar a hierarquia do Player. OPEN.
2. Testes das formulas puras (`SubStatsCalculator`, `HitChanceCalculator`, `AttackSpeedCalculator`, `WeightCapacityCalculator`, dano e custo de HP, `GetCastTimeMultiplier`, `GetZoneMaxHits`). OPEN.
3. `PlayerSkillCaster` (~57 KB): separar cast, validacao e execucao de zona. OPEN.
4. `BossSkillController` referencia UI: trocar por evento. OPEN.
5. Reescrever ou arquivar `FUTURE_IMPROVEMENTS.md`. OPEN.

## Fase F - Proximas classes e loop
Merchant, Acolyte, Thief; WASD e gamepad; dia/noite e spawn; refino e slots de carta; curvas de Job EXP; estado MAX LEVEL. OPEN.

## Marco
Fatia vertical jogavel para portfolio: login, Novice, Trial na Plains, promocao, dungeon com boss, video curto e README atualizado.

## Ordem sugerida
A -> B -> D.1 a D.3 -> C -> E.1 e E.2 -> restante de D -> F.
