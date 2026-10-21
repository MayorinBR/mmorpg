# Fase 1: mapas base, inimigos novos e iluminacao (2026-10-14)

Status: arquivos escritos, **nada foi compilado nem executado no Unity**. Os scripts passaram so por uma checagem de sintaxe. Os asmdefs do projeto nao foram alterados.

## Decisoes
- A cidade ja existe: `Prototype_Map02` (hub com Merchant, portais para Map01, Map03, Map04 e Tavern). `Prototype_Map01` e o campo com spawn. `Prototype_Map03` vira a **Plains** e `Prototype_Map04` a **Cave**; os dois estavam vazios (terreno, portal de volta para a cidade e bootstrap).
- Geracao por menu de editor, em greybox (primitivas coloridas), conforme `BASE_MAP_ENVIRONMENTS_PLAN.md`. Modelos finais entram depois trocando os prefabs.
- Os scripts ficam em `Assets/Scripts/Editor/Local/` (ignorado pelo Git, como os outros setups de uma vez so).
- A Cave nao usa o terreno: ele e desativado (componentes Terrain e TerrainCollider), nao apagado. Para voltar atras, reative os dois componentes do objeto `Terrain` e apague `Generated_Cave`.

## O que foi alterado nos arquivos
- `Assets/Data/Enemies/WolfStats.asset`: Wolf Fang a 50%, 1 unidade (fecha o Novice Trial: 10 Wolf, 5 Wolf Fang).
- Iluminacao: `Prototype_Map02` shadow strength 1 para 0.5; `Prototype_Map03` e `Prototype_Map04` shadow strength 1 para 0.5, ambiente de Skybox para Gradient com as mesmas cores do Map01.
- Stats novos em `Assets/Data/Enemies/`:

| Inimigo | Base | Lv | HP | ATK | DEF | EXP/Job | Local |
|---|---|---|---|---|---|---|---|
| Drops | Poring laranja | 3-6 | 55 | 8 | 2 | 22/18 | Plains |
| Marin | Poring azul | 8-12 | 140 | 14 | 5 | 55/40 | Cave |
| Dire Wolf | Wolf escuro, modelo x1.3 | 11-15 | 260 | 24 | 8 | 110/80 | Cave |
| Alpha Wolf (boss) | Wolf vermelho, modelo x1.8 | 18 | 900 | 38 | 12 | 600/300 | Cave, sala do boss |

  Drops: Drops (Bread 35%, Health Potion 15%, Mana Herb 10%); Marin (Health Potion 20%, Mana Potion 10%, Antidote 15%, Low Leather Boots 4%, Basic Pants 4%); Dire Wolf (Wolf Fang 30%, Health Potion 20%, Machete 3%, Iron Sword 3%); Alpha Wolf (Health Potion 100% x2-3, Mana Potion 60%, Machete 25%, Iron Sword 25%, Red Barret 15%, Ranger Armor 15%). Todos os valores sao provisorios.

## Como rodar (Unity)
1. Feche ou salve cenas abertas. Espere o Unity importar os novos scripts e assets e confirme que o Console esta sem erros.
2. `Tools > Maps > Phase 1 > Build Everything`. Ou, um por vez: `1 - Build Enemy Roster`, `2 - Build Plains`, `3 - Build Cave`, `4 - Register Maps`. Todos podem ser repetidos: o roster pula prefabs que ja existem; Plains e Cave apagam e refazem o que geraram.
3. O menu abre e salva `Prototype_Map03` e `Prototype_Map04`, bake do NavMesh incluido (o asset de NavMesh de cada cena e substituido).

## O que cada menu gera
- **Roster** (`EnemyRosterBuilder`): copia `Poring.prefab` (Drops, Marin) e `Wolf.prefab` (Dire Wolf, Alpha Wolf) para `Assets/Prefabs/Enemies/`, liga o stats certo, cria materiais com tint em `Assets/Materials/Enemies/` e, nos lobos, aumenta `Model`, `Hitbox`, offsets da barra de vida e do numero de dano, e o agente. Rodar de novo nao recria prefabs existentes (para refazer, apague o prefab).
- **Plains** (`PlainsBlockout`, Map03): ate 70 arvores (tronco + copa, copa a 2.1 m para nao bloquear o NavMesh), 12 rochas, estrada de terra de 3 m ligando os dois portais, portal leste para a Cave (clone do portal da cidade, com ponto de chegada proprio), 10 pontos de spawn no miolo (Poring peso 3, Drops 2, Wolf 2; max 7 ativos; respawn 30/30/45 s) e neblina leve. Posicoes seguem a altura do terreno e uma semente fixa.
- **Cave** (`CaveBlockout`, Map04): entrada (10x10), corredor A (14 m), salao com 4 pilares (14x12), corredor B (12 m) e sala do boss (22x22) com plataforma; paredes de 4 m com aberturas de 4 m, 5 tochas (luz pontual), "void" escuro por baixo, 13 pontos de spawn (Marin peso 3, Dire Wolf 2; max 10 ativos; respawn 60/90 s) e uma area separada `EnemySpawn_Boss` com o Alpha Wolf (respawn 300 s). Portal para a Plains ao lado do portal da cidade. Atmosfera: sem skybox, ambiente escuro, neblina exponencial, luz direcional a 0.4.
- **Register Maps** (`MapRegistry`): nomes Field, City, Plains, Cave e posicoes no World Map (0, 60, 120 e 180 no eixo X).

## Conexoes de mapa
City (Map02) liga a Field (Map01), Plains (Map03), Cave (Map04) e Tavern. Novo: Plains (leste) <-> Cave (entrada). O portal City -> Cave continua existindo; se quiser forcar a progressao City -> Plains -> Cave, remova `WarpPortal_ToMap04` do Map02.

## Verificacao no Editor
1. Console sem erros de compilacao e sem avisos de "spawn point is not on the NavMesh".
2. Play em Map03: andar pela estrada, aggro dos inimigos, respawn, portal leste. Play em Map04: chegar pelo portal, corredores, boss, portal de volta.
3. Matar Wolf: Wolf Fang aparece; completar o Novice Trial do zero (apague `playersave.json`).
4. Ajustes provaveis: altura dos portais e dos pontos se o terreno tiver relevo, contraste da Cave, quantidade de spawns, balanceamento de HP/ATK.
5. Se `NavMeshBaker` avisar que nao achou `BuildNavMesh`, faca o Bake pelo Inspector do `NavMeshSurface` no objeto `Terrain`.

## Fora do escopo desta passada
Modelos finais de ambiente e inimigos, quest de matar o Alpha Wolf, NPCs novos na cidade, interior da Tavern, Map01 (campo) sem alteracao de layout.
