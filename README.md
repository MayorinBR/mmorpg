# \# MMORPG Project

# 

# A Unity-based MMORPG prototype inspired by Ragnarok Online.

# 

# \---

# 

# \## 🇬🇧 English

# 

# \### About

# 

# Prototype of a 2.5D/3D MMORPG built in Unity, inspired by Ragnarok Online's classic PvE combat and exploration gameplay. Developed by a small team, currently focused on core gameplay systems before scaling to a full multiplayer backend.

# 

# \### Implemented so far

# 

# \- Player movement (WASD, gamepad, click-to-move), camera-relative

# \- Isometric camera with rotation and zoom

# \- Ragnarok-style base stats (STR, AGI, VIT, INT, DEX, LUK) and derived sub-stats (ATK, MATK, DEF, MDEF, HIT, FLEE, CRIT)

# \- Stat point allocation UI

# \- Enemy AI (idle, chase, attack states) with aggro range and leash range

# \- Player auto-attack combat with target selection

# \- Health system for player and enemies, including death and respawn

# \- Health bar UI (screen-space for the player, world-space for enemies) with a color gradient

# \- Experience (XP) and leveling system, granting stat points per level

# \- Experience bar UI

# \- Item drop system (loot tables rolled on enemy death) and inventory data structure (stacking, capacity)

# \- Inventory UI (slot grid with icon and quantity)

# \- Equipment system (9 slots including dual accessories, multi-slot items like two-handed weapons, stat bonuses reflected live in the stat panel)

# \- Weight-based inventory capacity (instead of slot count), with fixed-size paginated pages, Next/Back navigation, and a carried weight readout

# \- Item tooltips on hover (inventory, equipment, and world-dropped items) showing icon, name, description, stat bonuses, requirements, and weight

# \- Click-to-collect item pickups (walks into range automatically if needed) with drops spread out around the enemy's death position

# \- Ragnarok-style window UI system: toggle panels via HUD buttons or keyboard shortcuts, draggable and remembered per session, auto-cascading layout that reclaims freed slots, minimize/close controls, and bring-to-front on click

# \- Character class system: 6 base classes (Swordman, Archer, Merchant, Acolyte, Thief, Mage), dual Base/Job leveling (base grants stat points, job grants skill points for the future skill system), and mana (SP)

# \- Main player HUD: name, HP/SP bars, Base/Job level with experience bars, carried weight, Zeny (currency system with no in-game source yet), and class icon/name

# 

# \---

# 

# \## 🇧🇷 Português

# 

# \### Sobre

# 

# Protótipo de um MMORPG 2.5D/3D feito em Unity, inspirado no gameplay clássico de combate PvE e exploração do Ragnarok Online. Desenvolvido por uma equipe pequena, com foco atual nos sistemas centrais de gameplay antes de escalar para uma infraestrutura multiplayer completa.

# 

# \### Implementado até agora

# 

# \- Movimentação do player (WASD, controle, clique para mover), relativa à câmera

# \- Câmera isométrica com rotação e zoom

# \- Status base estilo Ragnarok (STR, AGI, VIT, INT, DEX, LUK) e sub-status derivados (ATK, MATK, DEF, MDEF, HIT, FLEE, CRIT)

# \- UI de distribuição de pontos de status

# \- IA de inimigos (estados idle, perseguição, ataque) com raio de aggro e raio de coleira (leash)

# \- Combate por auto-attack do player com seleção de alvo

# \- Sistema de vida para player e inimigos, incluindo morte e respawn

# \- UI de barra de vida (fixa na tela para o player, flutuante no mundo para os inimigos) com gradiente de cor

# \- Sistema de experiência (XP) e level up, concedendo pontos de status por nível

# \- UI de barra de experiência

# \- Sistema de drop de item (tabelas de loot roladas na morte do inimigo) e estrutura de dados de inventário (empilhamento, capacidade)

# \- UI de inventário (grade de slots com ícone e quantidade)

# \- Sistema de equipamento (9 slots incluindo dois acessórios simultâneos, itens que ocupam múltiplos slots como armas de duas mãos, bônus de status refletidos em tempo real no painel de status)

# \- Inventário por capacidade de peso (em vez de contagem de slots), com páginas de tamanho fixo, navegação Next/Back e indicador de peso carregado

# \- Tooltips de item ao passar o mouse (inventário, equipamento e itens dropados no chão) mostrando ícone, nome, descrição, bônus de status, requisitos e peso

# \- Coleta de item por clique (o player anda até o alcance automaticamente se necessário), com drops espalhados ao redor da posição de morte do inimigo

# \- Sistema de janelas de UI estilo Ragnarok: abrir/fechar painéis por botão na HUD ou atalho de teclado, arrastáveis e lembradas durante a sessão, layout em cascata que reaproveita espaço liberado, controles de minimizar/fechar, e traz pra frente ao clicar

# \- Sistema de classe do personagem: 6 classes base (Swordman, Archer, Merchant, Acolyte, Thief, Mage), progressão dupla Base/Job (base concede pontos de status, job concede pontos de skill para o futuro sistema de skills), e mana (SP)

# \- HUD principal do player: nome, barras de HP/SP, nível Base/Job com barras de experiência, peso carregado, Zeny (sistema de moeda ainda sem fonte no jogo) e ícone/nome da classe

# 

# \---

# 

# \## 🇯🇵 日本語

# 

# \### 概要

# 

# Ragnarok Onlineのクラシックな戦闘(PvE)と探索のゲームプレイにインスパイアされた、Unity製2.5D/3D MMORPGのプロトタイプです。小規模なチームによって開発されており、本格的なマルチプレイヤー基盤へ拡張する前に、コアとなるゲームプレイシステムの構築に注力しています。

# 

# \### 実装済みの機能

# 

# \- プレイヤーの移動(WASD、ゲームパッド、クリック移動)、カメラ基準

# \- 回転とズームが可能なアイソメトリックカメラ

# \- Ragnarok風の基本ステータス(STR、AGI、VIT、INT、DEX、LUK)と派生サブステータス(ATK、MATK、DEF、MDEF、HIT、FLEE、CRIT)

# \- ステータスポイント振り分けUI

# \- 敵AI(待機・追跡・攻撃の状態)、索敵範囲とリーシュ範囲付き

# \- ターゲット選択機能付きのプレイヤー自動攻撃戦闘

# \- プレイヤーと敵の体力システム(死亡とリスポーンを含む)

# \- 体力バーUI(プレイヤーは画面固定、敵はワールド空間に表示)、カラーグラデーション付き

# \- 経験値(XP)とレベルアップシステム、レベルごとにステータスポイントを付与

# \- 経験値バーUI

# \- アイテムドロップシステム(敵の死亡時に抽選されるルートテーブル)とインベントリのデータ構造(スタック、容量制限)

# \- インベントリUI(アイコンと数量を表示するスロットグリッド)

# \- 装備システム(アクセサリー2枠を含む9つの装備スロット、両手武器のような複数スロットを占有するアイテム、ステータスパネルにリアルタイムで反映される装備ボーナス)

# \- 重量ベースのインベントリ容量(スロット数ではなく重量で管理)、固定サイズのページ制、Next/Back操作によるページ移動、所持重量の表示

# \- マウスホバーで表示されるアイテムツールチップ(インベントリ、装備、地面のドロップアイテム)、アイコン・名前・説明・ステータスボーナス・条件・重量を表示

# \- クリックでアイテムを収集(必要に応じて自動で範囲内まで移動)、敵の死亡位置周辺にドロップが分散して出現

# \- Ragnarok風のウィンドウUIシステム: HUDボタンまたはキーボードショートカットでパネルの開閉、ドラッグ移動とセッション中の位置記憶、空いたスペースを再利用する自動カスケード配置、最小化・閉じるボタン、クリックで最前面に表示

# \- キャラクタークラスシステム: 6つの基本職(Swordman、Archer、Merchant、Acolyte、Thief、Mage)、ベース/ジョブの二重レベル制(ベースはステータスポイント、ジョブは将来のスキルシステム用のスキルポイントを付与)、マナ(SP)

# \- プレイヤーメインHUD: 名前、HP/SPバー、経験値バー付きのベース/ジョブレベル、所持重量、ゼニー(まだゲーム内に入手手段のない通貨システム)、職業アイコン・名前

