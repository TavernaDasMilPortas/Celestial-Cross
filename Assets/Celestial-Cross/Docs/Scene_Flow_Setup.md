# Configuração do Fluxo de Cenas (Menu/Hub → Preparação → Batalha)

Este guia ensina a configurar, no Editor da Unity, o ciclo jogável atual:

- Menu ou Hub (seleção de fase)
- Preparação (seleção de unidades que você vai levar)
- Batalha (fase de posicionamento no grid + início do combate)
- Finalização da fase (recompensa + retorno opcional ao Hub)

## 1) Pré-requisitos (Assets)

### 1.1 UnitData: campos obrigatórios
Em cada `UnitData`:

- `UnitID`
  - É gerado automaticamente (GUID do asset) e não é editável no Inspector.
  - Usado para: salvar conta, seleção de units, spawn.
- `displayName`
- `icon` (Sprite)

Se `UnitID` estiver vazio, o bootstrap e/ou spawn podem falhar com warnings.

### 1.2 UnitCatalog (mapeamento UnitID → UnitData)
Crie um asset `UnitCatalog`:

- Menu: `Create → RPG → Unit Catalog`
- Para cada unit, adicione uma Entry:
  - `UnitData` (arraste o asset)

Este catálogo é usado por:
- `PreparationSceneController` (converte `OwnedUnitIDs` da conta em `UnitData` para exibir nomes)
- `PlacementManager` (converte `SelectedUnitIDs` em `UnitData` para gerar os botões/ícones de posicionamento)

Observação: o spawn em batalha **não** depende mais de um prefab por UnitID. Agora usamos prefabs “molde”.

### 1.3 Prefabs “Molde” (Player/Enemy)
Você precisa de dois prefabs genéricos:

- `PlayerUnit_Mold.prefab`
- `EnemyUnit_Mold.prefab`

Ambos devem ter, no mínimo:
- `UnitRuntimeConfigurator` (responsável por aplicar `UnitData`/`PetData` em runtime)
- Um componente `Unit` (ex: `PlayerUnit` no molde do player, `EnemyUnit` no molde do enemy)
- `SpriteRenderer` (referenciado no `UnitRuntimeConfigurator`)
- Os componentes base exigidos por `Unit` (ex: `Health`, `Collider`, etc.)

### 1.4 RewardPackage (recompensas)
Crie um ou mais assets de recompensa:

- Menu: `Create → RPG → Reward Package`
- Configure `Money` e `Energy`.

### 1.5 LevelData e LevelCatalog (fases)
Crie um `LevelData` por fase:

- Menu: `Create → RPG → Level Data`
- Preencha:
  - `LevelName`
  - `SceneName` (nome exato da cena de batalha no Build Settings)
  - `PhaseMap` (layout do grid/tiles a ser usado na batalha)
  - `Waves` (lista de waves; por enquanto a Wave 0 é usada no spawn inicial)
    - cada wave contém `Enemies` (lista de `UnitData` + `GridPosition`)
  - `Enemies (Legacy)` (use apenas se `Waves` estiver vazio)
  - `VictoryRewards` (RewardPackage)

Crie também um `LevelCatalog`:

- Menu: `Create → RPG → Level Catalog`
- Adicione seus `LevelData` na lista `Levels`.

### 1.6 Conta inicial (para testar rápido)
#### Opção A — Bootstrap (primeira execução)
Crie um `AccountBootstrapConfig`:

- Menu: `Create → Account → Bootstrap Config`
- Configure:
  - `StartingMoney`, `StartingEnergy`
  - `StartingUnits` (lista de UnitData iniciais)

O bootstrap é aplicado **apenas quando não existe** `account.json` ainda.

#### Opção B — Perfil de debug (simular contas diferentes)
Crie um ou mais `AccountProfile`:

- Menu: `Create → Account → Profile`
- Configure `Money`, `Energy`, `OwnedUnits` e `OwnedPets` (arraste os assets).

No `AccountManager`, habilite:
- `useDebugProfile = true`
- `debugProfile = (seu perfil)`

## 2) Build Settings (obrigatório)
Em `File → Build Settings…`, adicione as cenas:

1. `MenuScene` (se existir)
2. `HubScene` (se existir)
3. `PreparationScene`
4. Todas as cenas de batalha referenciadas em `LevelData.SceneName`

## 3) Cena: HubScene
Objetivo: mostrar fases disponíveis e ir para a Preparação.

### 3.1 Managers
Crie um GameObject vazio `_Managers` e adicione:

- `AccountManager`
  - `bootstrapConfig` (opcional, recomendado)
  - `useDebugProfile` e `debugProfile` (opcional)
- `GameFlowManager`

### 3.2 UI mínima
Crie um `Canvas` e, dentro dele:

- Dois `Text` (UnityEngine.UI):
  - Money
  - Energy
- Um container para botões de fase (ex.: `VerticalLayoutGroup`)
- Um prefab de `Button` (com um `Text` filho)

### 3.3 Controller
Crie um GameObject `_Hub` e adicione `HubSceneController`.
No Inspector, configure:

- `levelCatalog` (LevelCatalog)
- `levelsContainer` (container)
- `levelButtonPrefab` (prefab do botão)
- `moneyText`, `energyText`
- `preparationSceneName = PreparationScene`

## 4) Cena: PreparationScene
Objetivo: **selecionar quais unidades** você vai levar para a batalha.

Nesta cena não existe mais posicionamento 3×3. O posicionamento agora acontece na própria cena de batalha (Placement Phase).

### 4.1 UI — lista de units possuídas
Crie um `Canvas` e:

- Container para lista de units possuídas (ex.: `VerticalLayoutGroup`)
- Prefab de `Button` para cada unit (com `Text` filho)

### 4.2 UI — contador (opcional, recomendado)
Adicione um `Text` para exibir algo como “Selecionadas: 0/3”.

### 4.3 Botão de iniciar
Crie um `Button` “Start Battle” (fica desativado até selecionar pelo menos 1 unit).

### 4.4 Controller
Crie um GameObject `_Preparation` e adicione `PreparationSceneController`.
No Inspector, configure:

- `unitCatalog` (UnitCatalog)
- `ownedUnitsContainer`, `ownedUnitButtonPrefab`
- `selectedCountText` (opcional)
- `startBattleButton`
- `maxUnitsToBring` (ex.: 3)

## 5) Cena(s): Batalha
Objetivo: spawn dos inimigos (LevelData) + **fase de posicionamento do player** + iniciar combate.

### 5.1 Grid
Garanta que existe um `GridMap` na cena.

#### 5.1.1 Zonas de spawn do player (obrigatório)
O `PlacementManager` só permite posicionar unidades em tiles cujo `TileDefinition.isPlayerSpawnZone` esteja marcado.

Então, nos seus assets de `TileDefinition` (os que representam “tile de início do player”), marque:
- `isPlayerSpawnZone = true`

O `BattleLevelBuilder` aplica automaticamente o `LevelData.PhaseMap` no `GridMap` e regenera o grid.

Observação: o `PhaseMap` pode conter `unitSpawns` fixos. Como o spawn agora é dinâmico via `BattleLevelBuilder`, recomenda-se:
- durante testes: deixar `BattleLevelBuilder.clearExistingUnits = true`
- mais tarde: remover/ignorar `unitSpawns` do `PhaseMap` para evitar confusão (ou ativar/desativar no `GridMap.spawnUnitsFromPhaseMap`).

### 5.2 Managers obrigatórios
Na cena de batalha, crie:

- `_PhaseManager` com `PhaseManager`
- `_GraveyardManager` com `GraveyardManager`
- `_BattleLevelBuilder` com `BattleLevelBuilder`
  - `playerUnitMold` (PlayerUnit_Mold.prefab)
  - `enemyUnitMold` (EnemyUnit_Mold.prefab)
  - `placementManager` (referência ao PlacementManager da cena)
  - `autoStartCombatAfterBuild = true`

- `_PlacementManager` com `PlacementManager`
  - `playerUnitMoldPrefab` (PlayerUnit_Mold.prefab)
  - `tileLayer` (LayerMask dos tiles do grid)
  - `placementActionBar` (ActionBarUI que será usado para os ícones de posicionamento)
  - `unitCatalog` (UnitCatalog)

#### 5.2.1 Botão “Confirmar Posicionamento” (obrigatório)
O `BattleLevelBuilder` **espera** o fim do posicionamento antes de iniciar o combate.

Crie um `Button` na UI da cena de batalha e conecte o `OnClick()` para chamar:
- `PlacementManager.EndPlacementPhase()`

Sem isso, a cena vai ficar travada na Placement Phase.

### 5.3 CombatInitializer
Se houver um `CombatInitializer` na cena:
- Defina `autoStart = false`.

O `BattleLevelBuilder` chama `CombatInitializer.StartCombat()` ao final do build.

### 5.4 Retorno ao Hub (opcional)
No `PhaseManager`:

- `autoReturnToHub = true`
- `hubSceneName = HubScene`
- `returnDelaySeconds` (ex.: 1.0)

## 6) Verificação rápida (checklist)

- HubScene abre e lista fases
- Clicar numa fase abre PreparationScene
- PreparationScene lista units (da conta) e permite selecionar até `maxUnitsToBring`
- Start Battle carrega a cena de batalha configurada no LevelData
- Na batalha: inimigos spawnam (LevelData) e começa a Placement Phase
- Placement Phase: ActionBar mostra ícones das units selecionadas e você posiciona apenas em tiles `isPlayerSpawnZone`
- Ao confirmar o posicionamento (botão), o combate inicia
- Ao vencer, recompensa é aplicada em `AccountManager` (Money/Energy) e salva
- Ao voltar ao Hub, os valores atualizados aparecem

## 6.1) Checklist de Inspector (atalho)

Use esta seção como “colar e conferir” para evitar esquecer referências.

### PreparationScene
- `PreparationSceneController`
  - `unitCatalog` → (asset UnitCatalog)
  - `ownedUnitsContainer` → (Transform container)
  - `ownedUnitButtonPrefab` → (Button prefab)
  - `selectedCountText` → (Text, opcional)
  - `startBattleButton` → (Button)
  - `maxUnitsToBring` → (ex: 3)

### Battle Scene (LevelData.SceneName)
- `BattleLevelBuilder`
  - `playerUnitMold` → PlayerUnit_Mold.prefab
  - `enemyUnitMold` → EnemyUnit_Mold.prefab
  - `placementManager` → (referência ao PlacementManager da cena)
  - `clearExistingUnits` → true (recomendado durante testes)
  - `autoStartCombatAfterBuild` → true

- `PlacementManager`
  - `playerUnitMoldPrefab` → PlayerUnit_Mold.prefab
  - `tileLayer` → LayerMask onde estão os colliders dos tiles
  - `placementActionBar` → ActionBarUI da HUD
  - `unitCatalog` → (asset UnitCatalog)

- UI “Confirmar posicionamento”
  - Button OnClick → `PlacementManager.EndPlacementPhase()`

## 7) Reset de teste (limpar save)
O save fica em `Application.persistentDataPath/account.json`.

Para simular “primeira execução” novamente:
- apague o arquivo `account.json`, ou
- ative `useDebugProfile` e escolha um `AccountProfile`.
