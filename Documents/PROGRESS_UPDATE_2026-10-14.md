# Atualizacao de progresso (2026-10-14)

Cobre o periodo desde `NOVICE_CLASS_2026-10-10.md` (ultima documentacao feita) ate hoje. Fontes: historico de commits do repositorio local (`.git/logs/HEAD`), arquivos modificados depois de 10/10 e o conteudo atual de `ArmorManifest.json`, scripts e assets. Nada foi compilado nem executado no Unity por quem escreveu este documento; o que depende de teste esta marcado.

## Linha do tempo (commits)
| Data | Commit | Conteudo |
|---|---|---|
| 10/09 | Wolf rigged | Wolf quadrupede com Idle/Walk/Attack, material toon e textura |
| 10/09 | Spawn por pontos | Spawn manual com respawn por monstro (ver `ENEMY_SPAWN_SYSTEM_PLAN_2026-10-09.md`) |
| 10/10 | Novice promotion flow | Novice, NPC de promocao, abas do Skill Book, cor de pele em NPC (ver `NOVICE_CLASS_2026-10-10.md`) |
| 10/10 | more equipaments | Conjunto Blacksmith (Shirt, Pants, Boots) e Crown |
| 10/13 | death animation fix, items update | Correcao da animacao de morte e novos itens (Apprentice) |
| 10/13 | terrain removed | Terreno removido do projeto |
| 10/14 | reset + filter-branch rewrite + Ignore terrain texture pack | Historico reescrito e pacote de texturas de terreno ignorado (ver "Atencao: historico reescrito") |
| 10/14 | novice icon, new equipment, UI/HUD fixes | Icone do Novice, mais equipamentos, correcoes de Quest Log, Quest Board e Skill Book, anel de alvo mais perto do chao |

## O que foi feito desde 10/10

### Combate e animacao
- **Animacao de morte**: `DeathAnimationImportSettings` (`Project.Character.Animation.EditorTools`, AssetPostprocessor). O Animator do player roda sem root motion, entao o deslocamento vertical da queda extraido do quadril era descartado e o corpo ficava deitado no ar na altura em que estava de pe. O postprocessor embute (bake) a raiz na pose dos clipes de morte. Vale para FBX Humanoid em `Assets/Animations/Player` cujo nome contenha `Dying` ou `Death`: `loopTime` desligado, `lockRootRotation`, `lockRootHeightY` e `lockRootPositionXZ` ligados, `heightFromFeet` desligado. Menu `Tools/Animation/Reimport Death Animations` forca a reimportacao sem abrir o Inspector. Teoria: com a raiz travada na pose, a queda fica no esqueleto em vez de virar movimento do objeto, que o Animator sem root motion ignora. Mesma causa e mesma familia de ajuste do NPC afundando (Root Transform Y, Bake Into Pose, Based Upon = Feet) documentada em `NOVICE_CLASS_2026-10-10.md`.
- **Anel de alvo** (`GroundRingFollower`, `Project.UI`): posiciona o anel no ponto mais baixo dos colliders do alvo mais `heightAboveGround` (padrao 0.1), em vez de usar o pivo do alvo. Funciona com pivo nos pes, no centro ou com offset do agente. Fica escondido sem alvo. Usado no alvo de combate e na escolha de alvo de skill. Ajuste do commit "target ring closer to the bottom". Se o chao esconder parte do anel, subir `heightAboveGround`.

### UI
- Correcoes em Quest Log, Quest Board e Skill Book (mensagem do commit de 10/14; os detalhes de cada correcao nao estao registrados, apenas `SkillBookEntryUI` foi alterado nos scripts depois de 10/10).
- Icone do Novice adicionado em `ClassIconLookup`.

### Equipamentos
- `ArmorManifest.json` tem hoje **29 entradas**. Documentadas antes: Plate (3), Low Leather Boots, Ranger Boots, Ranger Gloves, Ranger Gauntlet, Ranger Armor, Basic Pants, Maid Clothes, Black Shoes, Green Slops, Red Beret, Blacksmith (3), Crown, Apprentice (3, material compartilhado `Apprentice`).
- **Novas no manifesto e ainda sem documento proprio**: Dark Armor, Dark Legs, Dark Boots, Fisher Pants, Ranger Shirt, Straw Hat, Turban, Villager Tunic, Villager Boots. Stats e precos seguem placeholder. Pendente documentar origem, cobertura e rig de cada uma.
- **Etapa 2 do pipeline** (`EquipmentMaterialApplier`, menu `Tools/Equipment/Apply Item Materials`): para cada modelo em `Assets/Models/Items/Equipaments` que ja tenha `<Nome>_BaseColor.png` ao lado, limita o import da textura a 1024 px, cria ou atualiza o material toon a partir do template `BlacksmithPants_Toon.mat` (pasta `Assets/Materials/Equipament`, sufixo `_Toon.mat`, propriedade `_BaseMap`) e liga ao modelo. Resolve a pendencia de memoria registrada no pipeline (texturas de 4096). Separado em `EquipmentMaterialBuilder`, `EquipmentMaterialLinker` e `EquipmentTextureConfigurator`.
- **Prompts de geracao**: Blacksmith e Bartender (`BLACKSMITH_OUTFIT_PROMPTS_2026-10-10.md`, `BARTENDER_OUTFIT_PROMPTS_2026-10-10.md`). O conjunto Bartender ainda nao tem modelo no manifesto.
- Itens Apprentice (Robe, Pants, Boots) criados como assets em 10/13.

### Repositorio
- Terreno removido (10/13) porque as texturas nao estavam em uso, e pacote `Realistic Terrain Textures Lite` adicionado ao `.gitignore` (10/14).
- **Atencao: historico reescrito.** O reflog mostra `filter-branch: rewrite` em 10/14. Se o remoto (`MayorinBR/mmorpg`, `main`) ja tinha os commits antigos, o push precisa ser forcado e qualquer outro clone ou branch de feature deve ser refeito a partir do novo historico. Nao foi possivel confirmar daqui se o push ja foi feito. O sync do Project com o GitHub tambem so enxerga o novo historico depois do push.

## Estado consolidado do projeto
- Classes: Novice (inicial) + 6 base; 63 skills em 7 databases (Novice 10, Swordman 10, Mage 14, Acolyte 13, Archer 7, Thief 7, Merchant 2). Swordman completo; Merchant com so 2 skills; Archer e Thief com 7.
- Mundo: 4 mapas prototipo (`Prototype_Map01` a `04`), minimap rotativo, spawn por pontos, King Poporing, Wolf com stats (`WolfStats`), Dummy.
- Sistemas: status effects e buff/debuff, quests (Hunt Porings, Poporing Culling, entrega de carta, Novice Trial), dialogo de NPC, promocao de classe, login/selecao/criacao de personagem, save por personagem com mapa e posicao, peso, equipamento visual com pipeline automatizado.
- Testes EditMode existem para AI, Combat, Items, Skills, Stats (`*.Tests.csproj`). Nenhum teste novo foi adicionado neste periodo.

## Pendencias herdadas ainda abertas (resumo)
Detalhes e prioridade em `NEXT_STEPS_PLAN_2026-10-14.md`. Principais: drop do Wolf e Wolf Fang, Wolf4k UV, botao Novice na cena de criacao (conferir), mapas base e iluminacao, reorganizacao da hierarquia do Player, icones de skill, texturas das espadas de duas maos.

## Trechos de documentos antigos que ficaram desatualizados
- `README.md`: atualizado hoje (ver abaixo).
- `FUTURE_IMPROVEMENTS.md` (ultima edicao 2026-09-28, 156 KB, nao reescrito): ainda lista como abertos o ATK proprio da arma, o spawn de boss e as notas de Magnum Break; ver tambem `WEAPON_ATK_MAGNUM_BREAK_XP_EVENT_2026-10-10.md` secao 6.
- `PROJECT_STATUS_AND_LIGHTING_PLAN_2026-09-28.md`: o plano de iluminacao continua valido e nao executado; o resto do status foi superado por este documento.
- `EQUIPMENT_PIPELINE_2026-10-06.md`: nao cobre a Etapa 2 (`Apply Item Materials`) nem as 9 pecas novas; a descricao esta neste documento ate a proxima revisao do pipeline.
- `MMO_Project_Design_Data_Tracker.xlsx`: nao foi revisado nesta passada; conferir se os itens e skills novos estao la.
