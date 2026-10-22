# MMORPG Project

A Unity-based MMORPG prototype inspired by Ragnarok Online.

---

## 🇬🇧 English

### About

Prototype of a 2.5D/3D MMORPG built in Unity, inspired by Ragnarok Online's classic PvE combat and exploration gameplay. Developed solo, currently focused on core gameplay systems before scaling to a full multiplayer backend.

### Implemented so far

- Player movement (WASD, gamepad, click-to-move), camera-relative
- Isometric camera with rotation and zoom
- Ragnarok-style base stats (STR, AGI, VIT, INT, DEX, LUK), matching real Ragnarok Online's own point curves (48 points at level 1, scaling per-level gains) and sub-stat formulas — STR or DEX drives physical damage depending on the equipped weapon (melee vs. bow/ranged), INT drives the Mage's damage and both skills' Magical damage, and VIT/INT grant bonus max HP/SP
- Stat point allocation UI
- Enemy AI (idle, chase, attack) with aggro and leash range
- Player auto-attack with target selection
- Health system for player and enemies, with death and respawn
- Health bar UI (screen-space for the player, world-space for enemies)
- Experience (XP) and leveling, granting stat points per level
- Experience bar UI
- Item drop system (loot tables) and inventory data structure (stacking, capacity)
- Inventory UI (slot grid with icon and quantity)
- Equipment system: 9 slots (dual accessories, multi-slot items like two-handed weapons), stat bonuses reflected live
- Weight-based inventory capacity, paginated, with a carried weight readout
- Item tooltips on hover (inventory, equipment, world drops): icon, name, description, bonuses, requirements, weight
- Click-to-collect item pickups, drops spread around the enemy's death position
- Ragnarok-style window UI: toggle via HUD/shortcuts, draggable, auto-cascading layout, minimize/close, bring-to-front
- Character class system: 6 base classes, dual Base/Job leveling, mana (SP)
- Main player HUD: name, HP/SP, Base/Job level with XP bars, weight, Zeny, class
- Equipment level/class requirement validation
- Mob variety: Aggressive/Passive behavior, Poring/Poporing as Prefab Variants
- `CharacterStatsHolder`: single shared stats reference per character
- Basic skill system: class-restricted, mana cost/cooldown/range, Damage or Heal effects, 10-slot hotbar, skills for every class (many still missing icons)
- Skill Book and Skill Hotbar windows: drag skills onto hotbar slots; icons show ready/cooldown/unusable state; learn/upgrade by spending Job Level points
- Class differentiation in basic attacks: range comes from the equipped weapon, except Mage, which always attacks at a fixed ranged distance with the cast animation regardless of what's equipped; Archer uses ammo (weak infinite fallback when out); Thief dual-wields for a second hit; Swordman can block; Mage attacks cost mana and carry a selectable element
- Elemental damage and resistance: any attack carries an element, including plain physical damage (modeled as Neutral, a real resistable element, not "no element"); an optional per-character resistance/weakness table scales incoming damage of any element, wired to Poring (resists Water, weak to Fire) and Poporing (resists Ground, weak to Electric)
- Target rings: each enemy prefab carries a TargetRingMarker with its own white hover ring (cursor over the enemy, for auto-attacks and targeted skills) and red current-target ring, sized per prefab; casting a Damage skill with no valid target lets you pick one by hovering
- Aggro indicator: a brief overhead "!" flashes above an enemy on fresh detection, retaliation, or re-aggro
- Floating damage numbers rise above the player and enemies on every hit
- Updated 3D models for the Player and NPCs (Blacksmith), replacing the original placeholder meshes with custom Humanoid-rigged assets
- Weapon-aware melee attack animation: unarmed (or dagger, as a temporary placeholder) plays a punching swing, while other melee weapons (sword, axe, etc.) play a dedicated weapon swing — selected automatically from the equipped weapon's subtype
- Classes: Novice start class (at Job Level 10 the Trial Master NPC in the city offers one trial per class, each asking to defeat 10 Skeletons and 1 Skeleton Warrior; one trial at a time, abandoning it resets progress, and turning the completed trial in to the NPC promotes the player) plus 6 base classes; Skill Book with one tab per class tier; 63 skills across 7 class databases (Swordman complete, including Magnum Break and two-handed sword mastery)
- Weapons carry their own ATK and an attack speed multiplier; two-handed swords are Swordman-only
- Status effects and buff/debuff system (poison, stun, Fatal Blow and others), knockback, Zeny skill costs, Merchant weight limit, extra elements (Ghost, Holy, Poison)
- Quest system with quest log and quest board, NPC dialogue, courier and kill/collect quests
- Login, character selection and character creation flow with accounts; per-character save of map and position; multi-map warping and a rotating minimap
- Character customization: head shape, skin tone, hair and facial expressions; equipment shown on the model through skinned equipment modules
- Automated equipment pipeline (Blender rigging, manifest, prefab, item asset, materials) with 29 equipment pieces
- Point-based enemy spawns with per-monster respawn delay and optional boss; King Poporing boss; rigged Wolf enemy
- Death animation root baked into the pose; target rings stay on the ground under the enemy

---

## 🇧🇷 Português

### Sobre

Protótipo de um MMORPG 2.5D/3D feito em Unity, inspirado no gameplay clássico de combate PvE e exploração do Ragnarok Online. Desenvolvido por uma equipe pequena, com foco atual nos sistemas centrais de gameplay antes de escalar para uma infraestrutura multiplayer completa.

### Implementado até agora

- Movimentação do player (WASD, controle, clique para mover), relativa à câmera
- Câmera isométrica com rotação e zoom
- Status base estilo Ragnarok (STR, AGI, VIT, INT, DEX, LUK), seguindo as curvas de pontos do Ragnarok Online de verdade (48 pontos no nível 1, ganho por nível escalonado) e as fórmulas reais de sub-status — STR ou DEX define o dano físico dependendo da arma equipada (corpo a corpo vs. arco/à distância), INT define o dano do Mage e o dano Mágico das skills, e VIT/INT concedem bônus de HP/SP máximos
- UI de distribuição de pontos de status
- IA de inimigos (idle, perseguição, ataque) com raio de aggro e de coleira (leash)
- Combate por auto-attack do player com seleção de alvo
- Sistema de vida para player e inimigos, com morte e respawn
- UI de barra de vida (fixa na tela pro player, flutuante no mundo pros inimigos)
- Sistema de experiência (XP) e level up, concedendo pontos de status por nível
- UI de barra de experiência
- Sistema de drop de item (loot tables) e estrutura de inventário (empilhamento, capacidade)
- UI de inventário (grade de slots com ícone e quantidade)
- Sistema de equipamento: 9 slots (dois acessórios simultâneos, itens de múltiplos slots como armas de duas mãos), bônus refletidos em tempo real
- Inventário por capacidade de peso, paginado, com indicador de peso carregado
- Tooltips de item ao passar o mouse (inventário, equipamento, drops no chão): ícone, nome, descrição, bônus, requisitos, peso
- Coleta de item por clique, drops espalhados ao redor da posição de morte do inimigo
- Sistema de janelas estilo Ragnarok: abrir/fechar por HUD ou atalho, arrastável, cascata automática, minimizar/fechar, traz pra frente
- Sistema de classe: 6 classes base, progressão dupla Base/Job, mana (SP)
- HUD principal: nome, HP/SP, nível Base/Job com barras de XP, peso, Zeny, classe
- Validação de requisito de nível/classe pra equipar itens
- Variedade de mob: comportamento Aggressive/Passive, Poring/Poporing como Prefab Variants
- `CharacterStatsHolder`: referência única de status por personagem
- Sistema básico de skills: restritas por classe, custo de mana/cooldown/alcance, efeitos de Dano ou Cura, hotbar de 10 slots, skills para todas as classes (muitas ainda sem ícone)
- Janelas de Skill Book e Skill Hotbar: arraste skills pra hotbar; ícones mostram estado pronto/cooldown/indisponível; aprenda/upe gastando pontos de Job Level
- Diferenciação de classes no ataque básico: alcance vem da arma equipada, exceto o Mage, que sempre ataca a uma distância fixa com a animação de conjuração, não importa o que estiver equipado; Archer usa munição (fallback fraco e infinito quando acaba); Thief dá um segundo golpe ao dual-wield; Swordman pode bloquear; ataque do Mage custa mana e tem elemento selecionável
- Dano e resistência elemental: todo ataque carrega um elemento, incluindo o dano físico comum (modelado como Neutral, um elemento resistível de verdade, não "sem elemento"); uma tabela opcional de resistência/fraqueza por personagem escala o dano recebido de qualquer elemento, já aplicada ao Poring (resiste a Água, fraco contra Fogo) e ao Poporing (resiste a Terra, fraco contra Elétrico)
- Anéis de alvo: cada prefab de inimigo tem um TargetRingMarker com anel branco de hover (cursor sobre o inimigo, para auto-attack e skills com alvo) e anel vermelho de alvo atual, com tamanho ajustável por prefab; ao usar uma skill de Dano sem alvo válido, basta passar o mouse para escolher um
- Indicador de aggro: um "!" flutuante pisca sobre o inimigo ao detectar, revidar ou reengajar o player
- Números de dano flutuantes sobem sobre o player e os inimigos a cada acerto
- Modelos 3D atualizados para o Player e NPCs (Blacksmith), substituindo os meshes placeholder originais por assets Humanoid customizados
- Animação de ataque corpo a corpo sensível à arma: desarmado (ou adaga, como placeholder temporário) toca uma animação de soco, enquanto outras armas corpo a corpo (espada, machado, etc.) tocam um golpe dedicado — escolhido automaticamente a partir do subtipo da arma equipada
- Classes: Novice como classe inicial (no Job Level 10 o NPC Trial Master da cidade oferece uma prova por classe, cada uma pedindo para derrotar 10 Skeletons e 1 Skeleton Warrior; uma prova por vez, abandonar zera o progresso e entregar a prova concluida ao NPC promove o jogador) mais 6 classes base; Skill Book com uma aba por nivel de classe; 63 skills em 7 databases de classe (Swordman completo, incluindo Magnum Break e Two-Handed Sword Mastery)
- Armas com ATK proprio e multiplicador de velocidade de ataque; espadas de duas maos exclusivas do Swordman
- Sistema de status effects e buff/debuff (veneno, stun, Fatal Blow e outros), knockback, custo de Zeny em skills, limite de peso do Merchant, elementos extras (Ghost, Holy, Poison)
- Sistema de quests com quest log e quest board, dialogo de NPC, quests de entrega e de matar/coletar
- Fluxo de login, selecao e criacao de personagem com contas; save por personagem com mapa e posicao; warp entre mapas e minimap rotativo
- Customizacao de personagem: formato da cabeca, cor de pele, cabelo e expressoes faciais; equipamentos mostrados no modelo por modulos skinned
- Pipeline automatizado de equipamentos (rig no Blender, manifesto, prefab, item asset, materiais) com 29 pecas
- Spawn de inimigos por pontos com tempo de respawn por monstro e boss opcional; boss King Poporing; inimigo Wolf com rig
- Raiz da animacao de morte embutida na pose; anéis de alvo ficam no chão sob o inimigo

---

## 🇯🇵 日本語

### 概要

Ragnarok Onlineのクラシックな戦闘(PvE)と探索のゲームプレイにインスパイアされた、Unity製2.5D/3D MMORPGのプロトタイプです。小規模なチームによって開発されており、本格的なマルチプレイヤー基盤へ拡張する前に、コアとなるゲームプレイシステムの構築に注力しています。

### 実装済みの機能

- プレイヤーの移動(WASD、ゲームパッド、クリック移動)、カメラ基準
- 回転とズームが可能なアイソメトリックカメラ
- Ragnarok風の基本ステータス(STR、AGI、VIT、INT、DEX、LUK)。実際のRagnarok Onlineのポイント曲線(レベル1で48ポイント、レベルごとの獲得量も段階的に増加)とサブステータス計算式を採用 — 物理ダメージは装備武器(近接か弓/遠距離か)に応じてSTRまたはDEXが決定し、INTはMageのダメージとスキルの魔法ダメージを、VIT/INTは最大HP/SPのボーナスを決定
- ステータスポイント振り分けUI
- 敵AI(待機・追跡・攻撃)、索敵範囲とリーシュ範囲付き
- ターゲット選択付きのプレイヤー自動攻撃
- プレイヤーと敵の体力システム(死亡・リスポーン含む)
- 体力バーUI(プレイヤーは画面固定、敵はワールド空間)
- 経験値(XP)とレベルアップ、レベルごとにステータスポイントを付与
- 経験値バーUI
- アイテムドロップシステム(ルートテーブル)とインベントリ構造(スタック、容量)
- インベントリUI(アイコンと数量のスロットグリッド)
- 装備システム: 9スロット(アクセサリー2枠、両手武器などの複数スロット占有アイテム)、ボーナスはリアルタイム反映
- 重量ベースのインベントリ容量、ページ制、所持重量表示
- ホバー時のアイテムツールチップ(インベントリ・装備・地面ドロップ): アイコン・名前・説明・ボーナス・条件・重量
- クリックでアイテム収集、敵の死亡位置周辺にドロップが分散
- Ragnarok風のウィンドウUI: HUD/ショートカットで開閉、ドラッグ移動、自動カスケード配置、最小化・閉じる、最前面表示
- キャラクタークラスシステム: 6基本職、ベース/ジョブ二重レベル制、マナ(SP)
- プレイヤーメインHUD: 名前、HP/SP、ベース/ジョブレベルとXPバー、所持重量、ゼニー、職業
- 装備のレベル・職業条件バリデーション
- モブのバリエーション: Aggressive/Passive行動、PoringとPoporingのPrefab Variant
- `CharacterStatsHolder`: キャラクターごとに単一のステータス参照
- 基本スキルシステム: 職業限定、マナコスト/クールダウン/射程、ダメージまたは回復効果、10スロットホットバー、全職業のスキル(多くはアイコン未設定)
- スキルブックとスキルホットバーのウィンドウ: スキルをホットバーへドラッグ、アイコンで使用可能/クールダウン/使用不可を表示、ジョブレベルのポイントを消費して習得・レベルアップ
- 基本攻撃における職業ごとの差別化: 攻撃レンジは装備武器から決定(Mageのみ例外で、装備に関係なく常に固定の遠距離レンジと詠唱アニメーションで攻撃)、Archerは矢を消費(尽きると弱い無限射撃にフォールバック)、Thiefは二刀流で2撃目、Swordmanはブロック可能、Mageの攻撃はマナ消費と属性選択付き
- 属性ダメージと耐性: あらゆる攻撃が属性を持つ(通常の物理ダメージもNeutralという実在する耐性可能な属性として扱われ、「属性なし」ではない)。キャラクターごとの耐性/弱点テーブル(任意設定)がどの属性のダメージも増減させ、Poring(水に耐性、火に弱い)とPoporing(地に耐性、電気に弱い)に設定済み
- スキルのターゲット選択: 有効な対象がない状態でダメージスキルを使うと、地面のリング表示で対象を選択できる。別のリングは現在の自動攻撃対象を常に示す
- 索敵アイコン: 敵が索敵・反撃・再索敵した際に頭上に「!」が一瞬表示される
- ダメージ数値: プレイヤーと敵が被弾するたびに数値が上昇しながら表示される
- 3Dモデルを更新: プレイヤーとNPC(鍛冶屋)のプレースホルダーメッシュを、カスタムのHumanoidリグ付きモデルに置き換え
- 武器に応じた近接攻撃アニメーション: 素手(または一時的なプレースホルダーとしてダガー)はパンチの振りを再生し、それ以外の近接武器(剣・斧など)は専用の振りアニメーションを再生 — 装備中の武器サブタイプから自動選択
- 職業: Noviceが初期職(ジョブレベル10で街のトライアルマスターNPCが職業ごとの試練を提示し、各試練はSkeleton 10体とSkeleton Warrior 1体の討伐。試練は同時に1つまで、放棄すると進行がリセットされ、達成した試練をNPCに報告すると転職)と6つの基本職。スキルブックは職業段階ごとのタブ。7つの職業データベースに63スキル(Swordmanは完成、Magnum BreakとTwo-Handed Sword Masteryを含む)
- 武器は固有のATKと攻撃速度倍率を持つ。両手剣はSwordman専用
- ステータス異常とバフ/デバフシステム(毒、スタン、Fatal Blowなど)、ノックバック、スキルのゼニーコスト、Merchantの重量上限、追加属性(Ghost、Holy、Poison)
- クエストログとクエストボード付きのクエストシステム、NPC会話、配達および討伐/収集クエスト
- アカウント付きのログイン・キャラクター選択・キャラクター作成フロー、キャラクターごとのマップと位置のセーブ、マップ間ワープ、回転ミニマップ
- キャラクターカスタマイズ: 頭の形、肌の色、髪、表情。装備はスキン付きモジュールでモデルに表示
- 自動化された装備パイプライン(Blenderでのリグ、マニフェスト、プレハブ、アイテムアセット、マテリアル)、29点の装備
- ポイント指定の敵スポーン(モンスターごとのリスポーン時間、任意でボス)、ボスKing Poporing、リグ付きのWolf
- 死亡アニメーションのルートをポーズに焼き込み、ターゲットリングを対象の足元に配置
