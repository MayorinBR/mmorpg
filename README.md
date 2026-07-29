# MMORPG Project

A Unity-based MMORPG prototype inspired by Ragnarok Online.

---

## 🇬🇧 English

### About

Prototype of a 2.5D/3D MMORPG built in Unity, inspired by Ragnarok Online's classic PvE combat and exploration gameplay. Developed by a small team, currently focused on core gameplay systems before scaling to a full multiplayer backend.

### Implemented so far

- Player movement (WASD, gamepad, click-to-move), camera-relative
- Isometric camera with rotation and zoom
- Ragnarok-style base stats (STR, AGI, VIT, INT, DEX, LUK) and derived sub-stats (ATK, MATK, DEF, MDEF, HIT, FLEE, CRIT)
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
- Basic skill system: class-restricted, mana cost/cooldown/range, Damage or Heal effects, 10-slot hotbar, 4 test skills
- Skill Book and Skill Hotbar windows: drag skills onto hotbar slots; icons show ready/cooldown/unusable state; learn/upgrade by spending Job Level points
- Class differentiation in basic attacks: range comes from the equipped weapon; Archer uses ammo (weak infinite fallback when out); Thief dual-wields for a second hit; Swordman can block; Mage attacks cost mana and have a selectable element (not yet affecting damage)

---

## 🇧🇷 Português

### Sobre

Protótipo de um MMORPG 2.5D/3D feito em Unity, inspirado no gameplay clássico de combate PvE e exploração do Ragnarok Online. Desenvolvido por uma equipe pequena, com foco atual nos sistemas centrais de gameplay antes de escalar para uma infraestrutura multiplayer completa.

### Implementado até agora

- Movimentação do player (WASD, controle, clique para mover), relativa à câmera
- Câmera isométrica com rotação e zoom
- Status base estilo Ragnarok (STR, AGI, VIT, INT, DEX, LUK) e sub-status derivados (ATK, MATK, DEF, MDEF, HIT, FLEE, CRIT)
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
- Sistema básico de skills: restritas por classe, custo de mana/cooldown/alcance, efeitos de Dano ou Cura, hotbar de 10 slots, 4 skills de teste
- Janelas de Skill Book e Skill Hotbar: arraste skills pra hotbar; ícones mostram estado pronto/cooldown/indisponível; aprenda/upe gastando pontos de Job Level
- Diferenciação de classes no ataque básico: alcance vem da arma equipada; Archer usa munição (fallback fraco e infinito quando acaba); Thief dá um segundo golpe ao dual-wield; Swordman pode bloquear; ataque do Mage custa mana e tem elemento selecionável (ainda sem efeito no dano)

---

## 🇯🇵 日本語

### 概要

Ragnarok Onlineのクラシックな戦闘(PvE)と探索のゲームプレイにインスパイアされた、Unity製2.5D/3D MMORPGのプロトタイプです。小規模なチームによって開発されており、本格的なマルチプレイヤー基盤へ拡張する前に、コアとなるゲームプレイシステムの構築に注力しています。

### 実装済みの機能

- プレイヤーの移動(WASD、ゲームパッド、クリック移動)、カメラ基準
- 回転とズームが可能なアイソメトリックカメラ
- Ragnarok風の基本ステータス(STR、AGI、VIT、INT、DEX、LUK)と派生サブステータス(ATK、MATK、DEF、MDEF、HIT、FLEE、CRIT)
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
- 基本スキルシステム: 職業限定、マナコスト/クールダウン/射程、ダメージまたは回復効果、10スロットホットバー、テストスキル4種
- スキルブックとスキルホットバーのウィンドウ: スキルをホットバーへドラッグ、アイコンで使用可能/クールダウン/使用不可を表示、ジョブレベルのポイントを消費して習得・レベルアップ
- 基本攻撃における職業ごとの差別化: 攻撃レンジは装備武器から決定、Archerは矢を消費(尽きると弱い無限射撃にフォールバック)、Thiefは二刀流で2撃目、Swordmanはブロック可能、Mageの攻撃はマナ消費と属性選択(ダメージにはまだ影響なし)